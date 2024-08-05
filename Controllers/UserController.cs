using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using hotel_system_backend.Models;
using hotel_system_backend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace hotel_system_backend.Controllers;

public class UserController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly RecordController _recordController;

    public UserController(ApplicationDbContext context, IConfiguration configuration, RecordController recordController)
    {
        _context = context;
        _configuration = configuration;
        _recordController = recordController;
    }

    [HttpPost]
    [Route("api/[controller]/signup")]
    public async Task<IActionResult> SignUp([FromBody] CreateUserDTO user)
    {
        if (ModelState.IsValid)
        {
            user.Password = SecurityUtils.HashPassword(user.Password);
            User myUser = new User(user);
            await _context.User.AddAsync(myUser);
            await _context.SaveChangesAsync();
            var jwtSecret = _configuration["Jwt:Secret"];
            var token = SecurityUtils.GenerateJwtToken(user.Email, jwtSecret);
            await _recordController.InsertToRecordAsync(user.UserId,"User signed up");
            return Ok(new { Token = token });
        }

        return BadRequest("Invalid data");
    }

    [HttpPost]
    [Route("api/[controller]/signin")]
    public async Task<IActionResult> SignIn([FromBody] CommonTypes.SignInRequest request)
    {
        if (ModelState.IsValid)
        {
            var user = _context.User.FirstOrDefault(u =>
                u.Password.Equals(SecurityUtils.HashPassword(request.Password)) &&
                u.Email.ToLower().Equals(request.Email.ToLower()));
            if (user != null)
            {
                var jwtSecret = _configuration["Jwt:Secret"];
                var token = SecurityUtils.GenerateJwtToken(user.Email, jwtSecret);
               await _recordController.InsertToRecordAsync(user.UserId,"User signed in");
                return Ok(new { Token = token });
            }
            await _recordController.InsertToRecordAsync(0,$"Failed sign in attempt: {request.Email}, {request.Password}");
            return BadRequest("Email or password are invalid");
        }

        return BadRequest("Invalid data");
    }
    [HttpPut]
    [Route("api/[controller]/update/{id}")]
    public async Task<IActionResult>  UpdateUser ([FromBody] User user, long id)
    {
        if (ModelState.IsValid)
        {
            var myUser = _context.User.FirstOrDefault(u => u.UserId.Equals(id));
            if (myUser != null)
            {
                User oldUser = new User(myUser);
                myUser.Password = SecurityUtils.HashPassword(user.Password);
                myUser.Email = user.Email;
                myUser.Name = user.Name;
                myUser.Authorities = user.Authorities;
                myUser.Role = user.Role;
                await _context.SaveChangesAsync();
                await _recordController.InsertToRecordAsync(myUser.UserId,$"User info updated as follows: Before => {oldUser.UserInfo()} --- After => {myUser.UserInfo()}");
                return Ok("Changes saved successfully");
            }
        }

        return BadRequest("Invalid data");
    }

    public class UserAndBusinessDayResponse
    {
        public User User { get; set; }
        public DateTime BusinessDay { get; set; }
    }
    [Authorize]
    [HttpGet]
    [Route("api/[controller]/getUser")]
    public IActionResult getUser()
    {
        var userEmail = User.Claims.FirstOrDefault(t => t.Type == ClaimTypes.Email)?.Value;
        var user = _context.User.FirstOrDefault(u => u.Email.ToLower().Equals(userEmail.ToLower()));
        var businessDay = _context.BusinessDay.FirstOrDefault(b => b.IsClosed == false);
        if (businessDay == null) return BadRequest("No business day opened");
        if (user != null)
        {
            var request = new UserAndBusinessDayResponse { BusinessDay = businessDay.Date.Date, User = user };
            
            return Ok(request);
        }

        return NotFound("User wasn't found");
    }
}