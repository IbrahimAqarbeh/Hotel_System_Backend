using System.Security.Claims;
using hotel_system_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace hotel_system_backend.Controllers;

public class ReservationController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly RecordController _recordController;

    public ReservationController(ApplicationDbContext context, RecordController recordController)
    {
        _context = context;
        _recordController = recordController;
    }

    [Authorize]
    [HttpPost]
    [Route("api/[controller]/create")]
    public async Task<IActionResult> CreateReservation([FromBody] Reservation reservation)
    {
        if (!ModelState.IsValid) return BadRequest("Invalid data");
        var userEmail = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Email)?.Value;
        var user = _context.User.FirstOrDefault(u => u.Email.ToLower().Equals(userEmail!.ToLower()));
        if (user == null) return Unauthorized("User isn't authorized");
        reservation.UserId = user.UserId;
        reservation.Status = "R";
        var businessDay = _context.BusinessDay.FirstOrDefault(b => b.IsClosed == false);
        if (businessDay == null) return BadRequest("No business day opened");
        reservation.ReservedOn = BusinessDay.UpdateToActualTime(businessDay.Date);
        _context.Reservation.Add(reservation);
        await _context.SaveChangesAsync();
        await _recordController.InsertToRecordAsync(user.UserId,
            $"Reservation: {reservation.ReservationNumber} created");

        return Ok(reservation.ReservationNumber);
    }
 
    [Authorize]
    [HttpPost]
    [Route("api/[controller]/createAndCheckIn")]
    public async Task<IActionResult> CreateAndCheckInReservation([FromBody] Reservation reservation)
    {
        // Pre-conditions: 1. Authorized / 2. Room != null. 3 / check-in is on current business day
        if (!ModelState.IsValid) return BadRequest("Invalid data");
        var userEmail = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Email)?.Value;
        var user = _context.User.FirstOrDefault(u => u.Email.ToLower().Equals(userEmail!.ToLower()));
        if (user == null) return Unauthorized("User isn't authorized");
        reservation.UserId = user.UserId;
        if (reservation.countOfRoomsReserved == 0) return BadRequest("Assign room first");
        var businessDay = _context.BusinessDay.FirstOrDefault(b => b.IsClosed == false);
        if (businessDay == null) return BadRequest("No business day opened");
        if (reservation.CheckIn.Date != businessDay.Date.Date) return BadRequest("check-in isn't today");
        reservation.CheckIn = BusinessDay.UpdateToActualTime(businessDay.Date);
        reservation.Status = "In";
        reservation.ReservedOn = BusinessDay.UpdateToActualTime(businessDay.Date);
        _context.Reservation.Add(reservation);
        var reservedRooms = await _context.Room
            .Where(r => r.ReservationNumber == reservation.ReservationNumber && !r.isOccupied)
            .ToListAsync();
        if (reservedRooms.Count < reservation.countOfRoomsReserved) return BadRequest("Check reserved rooms");
        foreach (var VARIABLE in reservedRooms)
        {
            VARIABLE.isOccupied = true;
            VARIABLE.isReserved = false;
            VARIABLE.ReservationNumber = reservation.ReservationNumber;
        }

        await _context.SaveChangesAsync();
        await _recordController.InsertToRecordAsync(user.UserId,
            $"Reservation: {reservation.ReservationNumber} created and checked-in");

        return Ok("Reservation created and checked-in successfully");
    }

    [Authorize]
    [HttpPost]
    [Route("api/[controller]/{resNum}/checkin")]
    public async Task<IActionResult> CheckIn(string roomNumber, long resNum)
    {
        if (!ModelState.IsValid) return BadRequest("Invalid info");
        var userEmail = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Email)?.Value;
        var user = _context.User.FirstOrDefault(u => u.Email.ToLower().Equals(userEmail!.ToLower()));
        if (user == null) return Unauthorized("User isn't authorized");
        var myReservation = _context.Reservation.FirstOrDefault(r => r.ReservationNumber == resNum);
        if (myReservation == null) return NotFound("Reservation not found");
        if (myReservation.Status != "R") return BadRequest("Reservation cannot be checked-in, status = R");
        var businessDay = _context.BusinessDay.FirstOrDefault(b => b.IsClosed == false);
        if (businessDay == null) return BadRequest("No business day opened");
        if (myReservation.CheckIn.Date != businessDay.Date.Date) return BadRequest("check-in isn't today");
        myReservation.CheckIn = BusinessDay.UpdateToActualTime(businessDay.Date);
        myReservation.Status = "In";
        var reservedRooms = await _context.Room
            .Where(r => !r.isOccupied && r.ReservationNumber == myReservation.ReservationNumber)
            .ToListAsync();
        if (myReservation.countOfRoomsReserved < reservedRooms.Count)
        {
            foreach (var reservedRoom in reservedRooms)
            {
                reservedRoom.isOccupied = true;
                reservedRoom.isReserved = false;
                reservedRoom.ReservationNumber = myReservation.ReservationNumber;
            }
        }
        else
            return BadRequest("Reserved rooms cannot be checked in, please check");

        await _context.SaveChangesAsync();
        await _recordController.InsertToRecordAsync(user.UserId,
            $"Reservation: {myReservation.ReservationNumber} checked-in");
        return Ok("Reservation checked-in successfully");
    }

    [Authorize]
    [HttpPost]
    [Route("api/[controller]/{resNum}/checkout")]
    public async Task<IActionResult> CheckOutReservation(long resNum)
    {
        if (!ModelState.IsValid) return BadRequest("Invalid info");
        var userEmail = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Email)?.Value;
        var user = _context.User.FirstOrDefault(u => u.Email.ToLower().Equals(userEmail!.ToLower()));
        if (user == null) return Unauthorized("User isn't authorized");
        var myReservation = _context.Reservation.FirstOrDefault(r => r.ReservationNumber == resNum);
        if (myReservation == null) return NotFound("Reservation not found");
        if (myReservation.Status != "In") return BadRequest("Reservation isn't In");
        var valueOfTransactions = await _context.Transactions.Where(t => t.Status.ToLower().Equals("ok"))
            .SumAsync(t => t.Value);
        if (valueOfTransactions != 0) return BadRequest("Balance has to be zero");
        myReservation.Status = "Out";
        var businessDay = await _context.BusinessDay.FirstOrDefaultAsync(b => b.IsClosed == false);
        if (businessDay == null) return BadRequest("No business day opened");
        myReservation.CheckOut = BusinessDay.UpdateToActualTime(businessDay.Date);
        var reservedRooms = await _context.Room
            .Where(r => r.ReservationNumber == myReservation.ReservationNumber && r.isOccupied)
            .ToListAsync();

        foreach (var reservedRoom in reservedRooms)
        {
            reservedRoom.isOccupied = false;
            reservedRoom.isReserved = false;
            reservedRoom.isDirty = true;
        }

        await _context.SaveChangesAsync();
        await _recordController.InsertToRecordAsync(user.UserId,
            $"Reservation: {myReservation.ReservationNumber} checked out");
        return Ok("Reservation checked-out successfully");
    }

    [Authorize]
    [HttpPut]
    [Route("api/[controller]/update/{resNum}")]
    public async Task<IActionResult> UpdateReservation(long resNum, [FromBody] Reservation reservation)
    {
        if (!ModelState.IsValid) return BadRequest("Invalid info");
        var userEmail = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Email)?.Value;
        var user = _context.User.FirstOrDefault(u => u.Email.ToLower().Equals(userEmail!.ToLower()));
        if (user == null) return Unauthorized("User isn't authorized");
        var myReservation = _context.Reservation.FirstOrDefault(r => r.ReservationNumber == resNum);
        if (myReservation == null) return NotFound("Reservation not found");
        if (myReservation.Status != "In") return BadRequest("Reservation isn't In");
        myReservation.CheckOut = reservation.CheckOut;
        myReservation.Price = reservation.Price;
        myReservation.Source = reservation.Source;
        myReservation.CheckIn = reservation.CheckIn;
        myReservation.MealPlan = reservation.MealPlan;
        myReservation.StatusMessage = reservation.StatusMessage;
        myReservation.Pax = reservation.Pax;

        await _context.SaveChangesAsync();
        await _recordController.InsertToRecordAsync(user.UserId,
            $"Reservation updated: {myReservation.ReservationNumber}");

        return Ok("Reservation updated successfully");
    }

    [Authorize]
    [HttpGet]
    [Route("api/[controller]/allreservations")]
    public async Task<IActionResult> GetAllReservations()
    {
        var userEmail = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Email)?.Value;
        var user = await _context.User.FirstOrDefaultAsync(u => u.Email.ToLower().Equals(userEmail!.ToLower()));
        if (user == null) return Unauthorized("User isn't authorized");
        var reservationList = await _context.Reservation.ToListAsync();
        return Ok(reservationList);
    }

    [Authorize]
    [HttpGet]
    [Route("api/[controller]/{resNum}")]
    public async Task<IActionResult> GetReservation(long resNum)
    {
        var userEmail = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Email)?.Value;
        var user = _context.User.FirstOrDefault(u => u.Email.ToLower().Equals(userEmail!.ToLower()));
        if (user == null) return Unauthorized("User isn't authorized");
        var reservation = await _context.Reservation
            .FirstOrDefaultAsync(u => u.ReservationNumber == resNum);
        return Ok(reservation);
    }

    [Authorize]
    [HttpGet]
    [Route("api/[controller]/transactions/{resNum}")]
    public async Task<IActionResult> GetReservationTransactions(long resNum)
    {
        var userEmail = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Email)?.Value;
        var user = _context.User.FirstOrDefault(u => u.Email.ToLower().Equals(userEmail!.ToLower()));
        if (user == null) return Unauthorized("User isn't authorized");
        var myReservation = await _context.Reservation.FirstOrDefaultAsync(r => r.ReservationNumber == resNum);
        if (myReservation == null) return NotFound("Reservation wasn't found");
        var listOfTransactions = await _context.Transactions.Where(t => t.ReservationNumber == resNum)
            .Include(r => r.BusinessDay)
            .Include(r => r.User)
            .ToListAsync();
        return Ok(listOfTransactions);
    }

    [Authorize]
    [HttpGet]
    [Route("api/[controller]/getDailyCounts")]
    public async Task<IActionResult> GetReservationCounts()
    {
        var businessDay = await _context.BusinessDay.FirstOrDefaultAsync(b => b.IsClosed == false);
        if (businessDay == null) return BadRequest("No business day opened");
        var allRooms = await _context.Room.ToListAsync();
        foreach (var VARIABLE in allRooms)
        {
            VARIABLE.isReserved = false;
        }

        var roomList = await _context.Room
            .Where(r => r.Reservation.Status.Equals("R") && r.Reservation.CheckIn.Date == businessDay.Date.Date)
            .ToListAsync();

        foreach (var v in roomList)
        {
            v.isReserved = true;
        }

        await _context.SaveChangesAsync();

        double averagePrices = 0;
        var totalBBGuests = 0;
        var totalBOGuests = 0;
        var totalHBGuests = 0;
        var totalFBGuests = 0;
        var totalArrivals = 0;
        var totalInReservations = await _context.Room.Where(r => r.isOccupied).CountAsync();
        var totalInTodayReservations = await _context.Room.Join(
                _context.Reservation,
                room => room.ReservationNumber,
                reservation => reservation.ReservationNumber,
                (room, reservation) => new { room, reservation }
            ).Where(joined => joined.room.isOccupied && joined.reservation.CheckIn.Date == businessDay.Date.Date)
            .CountAsync();
        var totalOutTodayReservations = await _context.Room.Join(
                _context.Reservation,
                room => room.ReservationNumber,
                reservation => reservation.ReservationNumber,
                (room, reservation) => new { room, reservation }
            ).Where(joined => !joined.room.isOccupied && joined.reservation.CheckOut.Date == businessDay.Date.Date 
            && joined.reservation.Status.Equals("Out"))
            .CountAsync();
        var expectedCheckOut = await _context.Reservation
            .Where(r => r.Status != null && r.Status.Equals("In") && r.CheckOut.Date == businessDay.Date.Date)
            .CountAsync();
        var totalArrivalsList = await _context.Reservation
            .Where(r => r.Status != null && r.Status.Equals("R") && r.CheckIn.Date == businessDay.Date.Date)
            .ToListAsync();
        if (totalArrivalsList.Any())
        {
            foreach (var VARIABLE in totalArrivalsList)
            {
                totalArrivals += VARIABLE.countOfRoomsReserved;
            }
        }
        var avaeragePricesReservations =
            await _context.Reservation.Where(r => r.Status != null && r.Status.Equals("In")).ToListAsync();
        if (avaeragePricesReservations.Any())
        {
            double sum = 0;
            foreach (var VARIABLE in avaeragePricesReservations)
            {
                sum += VARIABLE.Price;
            }

            averagePrices = sum / avaeragePricesReservations.Count;
        }

        var totalBBGuestsReservations = await _context.Reservation
            .Where(r => r.Status != null && r.Status.Equals("In") && r.MealPlan.Equals("BB"))
            .ToListAsync();
        if (totalBBGuestsReservations.Any())
        {
          foreach (var VARIABLE in totalBBGuestsReservations)
          {
              totalBBGuests += VARIABLE.Pax;
          }
        }

        var totalBoGuestsReservations = await _context.Reservation
            .Where(r => r.Status != null && r.Status.Equals("In") && r.MealPlan.Equals("BO"))
            .ToListAsync();
        if (totalBoGuestsReservations.Any())
        {
            foreach (var VARIABLE in totalBoGuestsReservations)
            {
                totalBOGuests += VARIABLE.Pax;
            }
        }

        var totalHBGuestsReservations = await _context.Reservation
            .Where(r => r.Status != null && r.Status.Equals("In") && r.MealPlan.Equals("HB"))
            .ToListAsync();
        if (totalHBGuestsReservations.Any())
        {
            foreach (var VARIABLE in totalHBGuestsReservations)
            {
                totalHBGuests += VARIABLE.Pax;
            }
        }

        var totalFBGuestsReservations = await _context.Reservation
            .Where(r => r.Status != null && r.Status.Equals("In") && r.MealPlan.Equals("FB"))
            .ToListAsync();
        if (totalFBGuestsReservations.Any())
        {
            foreach (var VARIABLE in totalFBGuestsReservations)
            {
                totalFBGuests += VARIABLE.Pax;
            }
        }

        var totalNumOfGuests = totalBBGuests + totalBOGuests + totalHBGuests + totalFBGuests;
        var totalVacantRooms = await _context.Room.Where(r => !r.isOccupied && !r.isReserved).CountAsync();
        var totalReservedRooms = await _context.Room.Where(r => r.isReserved).CountAsync();
        var totalDirtyRooms = await _context.Room.Where(r => r.isDirty).CountAsync();
        var totalSoldYesterday = await _context.Transactions.Where(t =>
                t.TransactionType.ToLower().Equals("room charge") && t.BusinessDay.Date == businessDay.Date)
            .CountAsync();
        var totalSleepingOverNight = await _context.Reservation.Where(r =>
                r.Status.Equals("In") && r.CheckIn.Date != businessDay.Date && r.CheckOut.Date != businessDay.Date.Date)
            .CountAsync();
        var totalBoRooms = _context.Room
            .Join(_context.Reservation,
                room => room.ReservationNumber,
                reservation => reservation.ReservationNumber,
                (room, reservation) => new { room, reservation })
            .Where(joined => joined.reservation.MealPlan == "BO" && joined.reservation.Status.Equals("In")
            && joined.room.isOccupied)
            .Select(joined => joined.room)
            .Count();

        var totalBbRooms =  _context.Room
            .Join(_context.Reservation,
                room => room.ReservationNumber,
                reservation => reservation.ReservationNumber,
                (room, reservation) => new { room, reservation })
            .Where(joined => joined.reservation.MealPlan == "BB" && joined.reservation.Status.Equals("In")
                                                                 && joined.room.isOccupied)
            .Select(joined => joined.room)
            .Count();

        var totalHbRooms =  _context.Room
            .Join(_context.Reservation,
                room => room.ReservationNumber,
                reservation => reservation.ReservationNumber,
                (room, reservation) => new { room, reservation })
            .Where(joined => joined.reservation.MealPlan == "HB" && joined.reservation.Status.Equals("In")
                                                                 && joined.room.isOccupied)
            .Select(joined => joined.room)
            .Count();

        var totalFbRooms = _context.Room
            .Join(_context.Reservation,
                room => room.ReservationNumber,
                reservation => reservation.ReservationNumber,
                (room, reservation) => new { room, reservation })
            .Where(joined => joined.reservation.MealPlan == "FB" && joined.reservation.Status.Equals("In")
                                                                 && joined.room.isOccupied)
            .Select(joined => joined.room)
            .Count();

        var counts = new Dictionary<string, double>
        {
            { "inReservations", totalInReservations },
            { "checkedInReservations", totalInTodayReservations },
            { "checkedOutReservations", totalOutTodayReservations },
            { "expectedCheckedOutReservations", expectedCheckOut },
            { "totalArrivals", totalArrivals },
            { "averagePrices", averagePrices },
            { "totalBBGuests", totalBBGuests },
            { "totalBOGuests", totalBOGuests },
            { "totalHBGuests", totalHBGuests },
            { "totalFBGuests", totalFBGuests },
            { "totalNumOfGuests", totalNumOfGuests },
            { "totalVacantRooms", totalVacantRooms },
            { "totalReservedRooms", totalReservedRooms },
            { "totalDirtyRooms", totalDirtyRooms },
            { "totalSleepingOverNight", totalSleepingOverNight },
            { "totalSoldYesterday", totalSoldYesterday },
            { "totalBoRooms", totalBoRooms },
            { "totalBbRooms", totalBbRooms },
            { "totalHbRooms", totalHbRooms },
            { "totalFbRooms", totalFbRooms }
        };

        return Ok(counts);
    }

    [Authorize]
    [HttpGet]
    [Route("api/[controller]/search")]
    public async Task<IActionResult> SearchReservations([FromQuery] long? userId, [FromQuery] long? guestId,
        [FromQuery] string? roomNumber, [FromQuery] DateTime? reservedOn, [FromQuery] DateTime? checkIn,
        [FromQuery] DateTime? checkOut, [FromQuery] string? status, [FromQuery] string? mealPlan,
        [FromQuery] string? source, [FromQuery] double? price, [FromQuery] string? guestName)
    {
        var userEmail = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Email)?.Value;
        var user = _context.User.FirstOrDefault(u => u.Email.ToLower().Equals(userEmail!.ToLower()));
        if (user == null) return Unauthorized("User isn't authorized");
        var query = _context.Reservation.AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(r => r.UserId == userId);
        }

        if (guestId.HasValue)
        {
            var guest = _context.Guest.FirstOrDefault(g => g.GuestId == guestId);
            query = query.Where(r => r.ReservationNumber == guest.ReservationNumber);
        }

       // if (!string.IsNullOrEmpty(roomNumber))
        //{
            // make it invoice
          //  query = query.Where(r => r.RoomNumber == roomNumber);
        //}

        if (reservedOn.HasValue)
        {
            query = query.Where(r => r.ReservedOn.HasValue && r.ReservedOn.Value.Date == reservedOn.Value.Date);
        }

        if (checkIn.HasValue)
        {
            query = query.Where(r => r.CheckIn.Date == checkIn.Value.Date);
        }

        if (checkOut.HasValue)
        {
            query = query.Where(r => r.CheckOut.Date == checkOut.Value.Date);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(r => r.Status == status);
        }

        if (!string.IsNullOrEmpty(status))
        {
            var guestList = await _context.Guest.Where(g => g.Name.ToLower().Contains(guestName.ToLower()))
                .ToListAsync();
            query = guestList.Aggregate(query,
                (current, g) => current.Where(r => r.ReservationNumber == g.ReservationNumber));
        }

        if (!string.IsNullOrEmpty(mealPlan))
        {
            query = query.Where(r => r.MealPlan == mealPlan);
        }

        if (!string.IsNullOrEmpty(source))
        {
            query = query.Where(r => r.Source == source);
        }

        if (price.HasValue)
        {
            query = query.Where(r => r.Price == price);
        }

        var reservations = await query.ToListAsync();

        return Ok(reservations);
    }
}