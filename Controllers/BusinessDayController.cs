using System.Security.Claims;
using hotel_system_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace hotel_system_backend.Controllers;

public class BusinessDayController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly RecordController _recordController;

    public BusinessDayController(ApplicationDbContext context, RecordController recordController)
    {
        _context = context;
        _recordController = recordController;
    }
    
    [Authorize]
    [HttpPost]
    [Route("api/[controller]/performNightAudit")]
    public async Task<IActionResult> PerformNightAudit()
    {
        var userEmail = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Email)?.Value;
        var user = await _context.User.FirstOrDefaultAsync(u => u.Email.ToLower().Equals(userEmail.ToLower()));
        if (user == null) return Unauthorized("User isn't authorized");
        var currentBusinessDay = _context.BusinessDay.FirstOrDefault(b => !b.IsClosed);
        if (currentBusinessDay == null)
        {
           var myInitialBusinessDay = new BusinessDay { Date = DateTime.Today, IsClosed = false };
           myInitialBusinessDay.Date = DateTime.SpecifyKind(myInitialBusinessDay.Date, DateTimeKind.Utc);
           _context.BusinessDay.Add(myInitialBusinessDay);
           await _context.SaveChangesAsync();
           return Ok("Initial business day is opened");
        }
        currentBusinessDay.IsClosed = true;
        await _context.SaveChangesAsync();
        var newBusinessDay = new BusinessDay { Date = DateTime.Today, IsClosed = false };
        newBusinessDay.Date = DateTime.SpecifyKind(newBusinessDay.Date, DateTimeKind.Utc);
        _context.BusinessDay.Add(newBusinessDay);
        await _context.SaveChangesAsync();
        return Ok("Night audit performed successfully");
    }
}