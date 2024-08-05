using System.Security.Claims;
using hotel_system_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace hotel_system_backend.Controllers;

public class TransactionsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly RecordController _recordController;

    public TransactionsController(ApplicationDbContext context, RecordController recordController)
    {
        _context = context;
        _recordController = recordController;
    }

    [Authorize]
    [HttpPost]
    [Route("api/[controller]/roomNightCharge")]
    public async Task<IActionResult> RoomNightCharge(long resNum, double value)
    {
        var userEmail = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Email)?.Value;
        var user = await _context.User.FirstOrDefaultAsync(u => u.Email.ToLower().Equals(userEmail!.ToLower()));
        if (user == null) return Unauthorized("User isn't authorized");
        var myReservarion = await _context.Reservation.Where(r => r.Status!.ToLower().Equals("in"))
            .FirstOrDefaultAsync(r => r.ReservationNumber == resNum);
        if (myReservarion == null) return BadRequest("Invalid reservation number");
        var businessDay = await _context.BusinessDay.FirstOrDefaultAsync(b => b.IsClosed == false);
        if (businessDay == null) return BadRequest("No business day opened");
        var charge = new Transactions
        {
            Status = "OK", ReservationNumber = myReservarion.ReservationNumber, ExactDate = DateTime.Now,
            BusinessDayId = businessDay.Id, UserId = user.UserId, Value = value, Description = "Room charge",
            CreditOrDebit = "Debit",
            TransactionType = "Room Charge"
        };
        charge.ExactDate = DateTime.SpecifyKind(charge.ExactDate,DateTimeKind.Utc);
        await _context.AddAsync(charge);
        await _context.SaveChangesAsync();
        await _recordController.InsertToRecordAsync(user.UserId,$"Room charge on reservation: {myReservarion.ReservationNumber}");
        return Ok("Room charged successfully");
    }
    
    [Authorize]
    [HttpPost]
    [Route("api/[controller]/outletCharge")]
    public async Task<IActionResult> OutletCharge(long resNum, double value, string description)
    {
        var userEmail = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Email)?.Value;
        var user = await _context.User.FirstOrDefaultAsync(u => u.Email.ToLower().Equals(userEmail!.ToLower()));
        if (user == null) return Unauthorized("User isn't authorized");
        var myReservarion = await _context.Reservation.Where(r => r.Status!.ToLower().Equals("in"))
            .FirstOrDefaultAsync(r => r.ReservationNumber == resNum);
        if (myReservarion == null) return BadRequest("Invalid reservation number");
        var businessDay = await _context.BusinessDay.FirstOrDefaultAsync(b => b.IsClosed == false);
        if (businessDay == null) return BadRequest("No business day opened");
        var charge = new Transactions
        {
            Status = "OK", ReservationNumber = myReservarion.ReservationNumber, ExactDate = DateTime.Now,
            BusinessDayId = businessDay.Id, UserId = user.UserId, Value = value, Description = description,
            CreditOrDebit = "Debit",
            TransactionType = "Outlet Charge"
        };
        charge.ExactDate = DateTime.SpecifyKind(charge.ExactDate,DateTimeKind.Utc);
        await _context.AddAsync(charge);
        await _context.SaveChangesAsync();
        await _recordController.InsertToRecordAsync(user.UserId,$"Outlet charge on reservation: {myReservarion.ReservationNumber}");
        return Ok("Outlet charged successfully");
    }
    
    [Authorize]
    [HttpPost]
    [Route("api/[controller]/CashPayment")]
    public async Task<IActionResult> CashPayment(long resNum, double value)
    {
        var userEmail = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Email)?.Value;
             var user = await _context.User.FirstOrDefaultAsync(u => u.Email.ToLower().Equals(userEmail!.ToLower()));
             if (user == null) return Unauthorized("User isn't authorized");
        var myReservarion = await _context.Reservation.Where(r => r.Status!.ToLower().Equals("in"))
            .FirstOrDefaultAsync(r => r.ReservationNumber == resNum);
        if (myReservarion == null) return BadRequest("Invalid reservation number");
        
        var businessDay = await _context.BusinessDay.FirstOrDefaultAsync(b => b.IsClosed == false);
        if (businessDay == null) return BadRequest("No business day opened");
        var charge = new Transactions
        {
            Status = "OK", ReservationNumber = myReservarion.ReservationNumber, ExactDate = DateTime.Now,
            BusinessDayId = businessDay.Id, UserId = user.UserId, Value = value * -1, Description = "Cash Payment",
            CreditOrDebit = "Credit",
            TransactionType = "Cash Payment"
        };
        charge.ExactDate = DateTime.SpecifyKind(charge.ExactDate,DateTimeKind.Utc);
        await _context.AddAsync(charge);
        await _context.SaveChangesAsync();
        await _recordController.InsertToRecordAsync(user.UserId,$"Cash payment on reservation: {myReservarion.ReservationNumber}");
        return Ok("Cash payment recorded successfully");
    }

    [Authorize]
    [HttpPut]
    [Route("api/[controller]/{transId}/delete")]
    public async Task<IActionResult> DeleteTransaction(long transId)
    {
        var userEmail = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Email)?.Value;
        var user = await _context.User.FirstOrDefaultAsync(u => u.Email.ToLower().Equals(userEmail!.ToLower()));
        if (user == null) return Unauthorized("User isn't authorized");
        var businessDay = await _context.BusinessDay.FirstOrDefaultAsync(b => b.IsClosed == false);
        if (businessDay == null) return BadRequest("No business day opened");
        var myTrans = await _context.Transactions.FirstOrDefaultAsync(t => t.TransactionId == transId &&
                                                                           t.Status == "OK");
        if (myTrans == null) return NotFound("Transaction wasn't found");
        var reservation = await _context.Reservation.FirstOrDefaultAsync(r =>
            r.ReservationNumber == myTrans.ReservationNumber && r.Status == "In");
        if (reservation == null) return BadRequest("Reservation is invalid or not found");
        myTrans.Status = "Deleted";
        var deletedTrans = new DeletedTransactions
        {
            TransactionId = myTrans.TransactionId,
            ExactDate = DateTime.Now,
            BusinessDayId = businessDay.Id,
            UserId = user.UserId,
            };
        deletedTrans.ExactDate = DateTime.SpecifyKind(deletedTrans.ExactDate, DateTimeKind.Utc);
        await _context.DeletedTransactions.AddAsync(deletedTrans);
        await _context.SaveChangesAsync();
        await _recordController.InsertToRecordAsync(user.UserId, $"Transaction deleted: {deletedTrans.TransactionId}");
        return Ok("Transaction deleted successfully");
    }
    
    [Authorize]
    [HttpPost]
    [Route("api/[controller]/CardPayment")]
    public async Task<IActionResult> CardPayment(long resNum, double value, string description)
    {
        var userEmail = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Email)?.Value;
        var user = await _context.User.FirstOrDefaultAsync(u => u.Email.ToLower().Equals(userEmail!.ToLower()));
        if (user == null) return Unauthorized("User isn't authorized");
        var myReservarion = await _context.Reservation.Where(r => r.Status!.ToLower().Equals("in"))
            .FirstOrDefaultAsync(r => r.ReservationNumber == resNum);
        if (myReservarion == null) return BadRequest("Invalid reservation number");
        var businessDay = await _context.BusinessDay.FirstOrDefaultAsync(b => b.IsClosed == false);
        if (businessDay == null) return BadRequest("No business day opened");
        var charge = new Transactions
        {
            Status = "OK", ReservationNumber = myReservarion.ReservationNumber, ExactDate = DateTime.Now,
            BusinessDayId = businessDay.Id, UserId = user.UserId, Value = value * -1, Description = description,
            CreditOrDebit = "Credit",
            TransactionType = "Card Payment"
        };
        charge.ExactDate = DateTime.SpecifyKind(charge.ExactDate,DateTimeKind.Utc);
        await _context.AddAsync(charge);
        await _context.SaveChangesAsync();
        await _recordController.InsertToRecordAsync(user.UserId,$"Card payment on reservation: {myReservarion.ReservationNumber}");
        return Ok("Card payment recorded successfully");
    }
    
    [Authorize]
    [HttpPost]
    [Route("api/[controller]/refund")]
    public async Task<IActionResult> Refund(long resNum, double value)
    {
        var userEmail = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Email)?.Value;
        var user = await _context.User.FirstOrDefaultAsync(u => u.Email.ToLower().Equals(userEmail!.ToLower()));
        if (user == null) return Unauthorized("User isn't authorized");
        var myReservarion = await _context.Reservation.Where(r => r.Status!.ToLower().Equals("in"))
            .FirstOrDefaultAsync(r => r.ReservationNumber == resNum);
        if (myReservarion == null) return BadRequest("Invalid reservation number");
        var businessDay = await _context.BusinessDay.FirstOrDefaultAsync(b => b.IsClosed == false);
        if (businessDay == null) return BadRequest("No business day opened");
        var charge = new Transactions
        {
            Status = "OK", ReservationNumber = myReservarion.ReservationNumber, ExactDate = DateTime.Now,
            BusinessDayId = businessDay.Id, UserId = user.UserId, Value = value, Description = "Refund",
            CreditOrDebit = "Debit",
            TransactionType = "Refund"
        };
        charge.ExactDate = DateTime.SpecifyKind(charge.ExactDate,DateTimeKind.Utc);
        await _context.AddAsync(charge);
        await _context.SaveChangesAsync();
        await _recordController.InsertToRecordAsync(user.UserId,$"Refund on reservation: {myReservarion.ReservationNumber}");
        return Ok("Refund recorded successfully");
    }
    
    [Authorize]
    [HttpPost]
    [Route("api/[controller]/discount")]
    public async Task<IActionResult> Discount(long resNum, double value)
    {
        var userEmail = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Email)?.Value;
        var user = await _context.User.FirstOrDefaultAsync(u => u.Email.ToLower().Equals(userEmail!.ToLower()));
        if (user == null) return Unauthorized("User isn't authorized");
        var myReservarion = await _context.Reservation.Where(r => r.Status!.ToLower().Equals("in"))
            .FirstOrDefaultAsync(r => r.ReservationNumber == resNum);
        if (myReservarion == null) return BadRequest("Invalid reservation number");
        var businessDay = await _context.BusinessDay.FirstOrDefaultAsync(b => b.IsClosed == false);
        if (businessDay == null) return BadRequest("No business day opened");
        var charge = new Transactions
        {
            Status = "OK", ReservationNumber = myReservarion.ReservationNumber, ExactDate = DateTime.Now,
            BusinessDayId = businessDay.Id, UserId = user.UserId, Value = value * -1, Description = "Discount",
            CreditOrDebit = "Credit",
            TransactionType = "Discount"
        };
        charge.ExactDate = DateTime.SpecifyKind(charge.ExactDate,DateTimeKind.Utc);
        await _context.AddAsync(charge);
        await _context.SaveChangesAsync();
        await _recordController.InsertToRecordAsync(user.UserId,$"Discount on reservation: {myReservarion.ReservationNumber}");
        return Ok("Discount recorded successfully");
    }
}