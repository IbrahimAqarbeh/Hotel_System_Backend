using System.Security.Claims;
using hotel_system_backend.Models;
using hotel_system_backend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hotel_system_backend.Controllers;

public class RoomController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly RecordController _recordController;

    public RoomController(ApplicationDbContext context, RecordController recordController)
    {
        _context = context;
        _recordController = recordController;
    }

    [Authorize]
    [HttpPost]
    [Route("api/[controller]/create")]
    public async Task<IActionResult> CreateRoom([FromBody]Room room)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("Invalid data");}
        
            var userEmail = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Email)?.Value;
            var user = _context.User.FirstOrDefault(u => u.Email.ToLower().Equals(userEmail!.ToLower()));
            if (user != null)
            {
                room.isOutOfOrder = false;
                room.isDirty = false;
                room.isReserved = false;
                room.isOccupied = false;
                _context.Room.Add(room);
                await _context.SaveChangesAsync();
                await _recordController.InsertToRecordAsync(user.UserId, $"Room {room.RoomNumber} added");
                return Ok("Room added successfully");
            }

            return Unauthorized("User isn't authorized");

    }

    [Authorize]
    [HttpPost]
    [Route("api/[controller]/update/{rNum}")]
    public async Task<IActionResult> UpdateRoom([FromBody] UpdateRoomDTO room, string rNum)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("invalid data");
        }
        var userEmail = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Email)?.Value;
        var user = _context.User.FirstOrDefault(u => u.Email.ToLower().Equals(userEmail!.ToLower()));
        if (user != null)
        {
            var myRoom = _context.Room.FirstOrDefault(u => u.RoomNumber.ToLower().Equals(rNum.ToLower()));
            if (myRoom != null)
            {
                myRoom.Description = room.Description;
                myRoom.isDirty = room.isDirty;
                myRoom.isOccupied = room.isOccupied;
                myRoom.isOutOfOrder = room.isOutOfOrder;
                myRoom.isReserved = room.isReserved;
                myRoom.Type = room.Type;
                await _context.SaveChangesAsync();
                await _recordController.InsertToRecordAsync(user.UserId, $"Room: {myRoom.RoomNumber} was updated");
                return Ok("Room updated successfully");
            }

            return NotFound("Room wasn't found");
        }

        return Unauthorized("User isn't authorized");
    }

    [Authorize]
    [HttpGet]
    [Route("api/[controller]/allrooms")]
    public IActionResult GetAllRooms()
    {
        var userEmail = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Email)?.Value;
        var user = _context.User.FirstOrDefault(u => u.Email.ToLower().Equals(userEmail!.ToLower()));
        if (user != null)
        {
            var roomList = _context.Room.ToList();
            return Ok(roomList);
        }

        return Unauthorized("User isn't authorized");
    }

    [Authorize]
    [HttpGet]
    [Route("api/[controller]/{rNum}")]
    public IActionResult GetRoom(string rNum)
    {
        var userEmail = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Email)?.Value;
        var user = _context.User.FirstOrDefault(u => u.Email.ToLower().Equals(userEmail!.ToLower()));
        if (user != null)
        {
            var room = _context.Room.FirstOrDefault(u=>u.RoomNumber.ToLower().Equals(rNum.ToLower()));
            if (room != null)
            {
                return Ok(room);
            }

            return NotFound("Room wasn't found");
        }

        return Unauthorized("User isn't authorized");
    }
}