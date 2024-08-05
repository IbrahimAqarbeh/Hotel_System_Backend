using System.Security.Claims;
using hotel_system_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace hotel_system_backend.Controllers;

public class GuestController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly RecordController _recordController;

    public GuestController(ApplicationDbContext context, RecordController recordController)
    {
        _context = context;
        _recordController = recordController;
    }
    
    [Authorize]
    [HttpPost]
    [Route("api/[controller]/create")]
    public async Task<IActionResult> CreateGuest([FromBody] Guest guest)
    {
        if (!ModelState.IsValid) return BadRequest("Invalid data");
        var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var myUser = _context.User.FirstOrDefault(u => u.Email.ToLower().Equals(userEmail!.ToLower()));
        if (myUser == null) return BadRequest("Invalid data");
        guest.DateOfBirth = DateTime.SpecifyKind(guest.DateOfBirth, DateTimeKind.Utc);
        _context.Guest.Add(guest);
        await _context.SaveChangesAsync();
        await _recordController.InsertToRecordAsync(myUser.UserId,
            $"Guest Created: {guest.GuestId}, {guest.Name}");
        return Ok("Guest created successfully");
    }

    [Authorize]
    [HttpGet]
    [Route("api/[controller]/allguests")]
    public IActionResult RetrieveAllGuests()
    {
        var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var myUser = _context.User.FirstOrDefault(u => u.Email.ToLower().Equals(userEmail.ToLower()));
        if (myUser != null)
        {
            return Ok(_context.Guest.Include(g=>g.Reservation).ToList());
        }

        return Unauthorized("User isn't authorized");
    }

    [Authorize]
    [HttpGet]
    [Route("api/[controller]/{id}")]
    public IActionResult GetGuest(long id)
    {
        var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var myUser = _context.User.FirstOrDefault(u => u.Email.ToLower().Equals(userEmail!.ToLower()));
        if (myUser != null)
        {
            var myGuest = _context.Guest.Include(g=>g.Reservation).FirstOrDefault(u => u.GuestId == id);
            if (myGuest != null)
            {
                return Ok(myGuest);
            }

            return BadRequest("Guest wasn't found");
        }

        return Unauthorized("User isn't authorized");
    }

    [Authorize]
    [HttpPost]
    [Route("api/[controller]/update/{id}")]
    public async Task<IActionResult> UpdateGuest([FromBody] Guest guest, long id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("Invalid info");}
        var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var myUser = _context.User.FirstOrDefault(u => u.Email.ToLower().Equals(userEmail.ToLower()));
        if (myUser != null)
        {
            var myGuest = _context.Guest.FirstOrDefault(u => u.GuestId.Equals(id));
            if (myGuest != null)
            {
                myGuest.Name = guest.Name;
                myGuest.Nationality = guest.Nationality;
                myGuest.DocumentId = guest.DocumentId;
                myGuest.DocumentType = guest.DocumentType;
                myGuest.Gender = guest.Gender;
                myGuest.BirthPlace = guest.BirthPlace;
                myGuest.DateOfBirth = guest.DateOfBirth;
                myGuest.ReservationNumber = guest.ReservationNumber;
                await _context.SaveChangesAsync();
                await _recordController.InsertToRecordAsync(myUser.UserId, $"Guest updated: {guest.GuestId}, {guest.Name}");
                return Ok("Guest updated successfully");
            }

            return BadRequest("Guest wasn't found");
        }

        return Unauthorized("User isn't authorized");
    }
}