using hotel_system_backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace hotel_system_backend.Controllers;

public class RecordController : Controller
{
    private readonly ApplicationDbContext _context;

    public RecordController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> InsertToRecordAsync(long userId, string message)
    {
        var user = await _context.User.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user != null)
        {
            Record r = new Record(user.UserId, message);
            var businessDay = await _context.BusinessDay.FirstOrDefaultAsync(b => b.IsClosed == false);
            if (businessDay == null) return BadRequest("No business day opened");
            r.DateTimeOfRecord = BusinessDay.UpdateToActualTime(businessDay.Date);
            _context.Add(r);
            await _context.SaveChangesAsync();
        }

        return BadRequest("User cannot be null");
    }

}