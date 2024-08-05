using System.ComponentModel.DataAnnotations;

namespace hotel_system_backend.Models.DTOs;

public class CreateUserDTO
{
    public long UserId { get; set; } 
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Name { get; set; }
    
    
    [Required]
    public string Password { get; set; }
    
    [Required]
    public string[] Authorities { get; set; }
    
    [Required]
    public String Role { get; set; }

}