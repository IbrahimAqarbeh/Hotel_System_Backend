using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using hotel_system_backend.Models.DTOs;

namespace hotel_system_backend.Models;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long UserId { get; set; } 
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Name { get; set; }
    
    
    [Required]
    [JsonIgnore]
    public string Password { get; set; }
    
    [Required]
    public string[] Authorities { get; set; }
    
    [Required]
    public String Role { get; set; }

    public string UserInfo()
    
    {
        string singleAuthoritiesString = string.Join(", ", Authorities);
        return $"Email: {this.Email}, Name: {this.Name}, Password: {this.Password}, Authorities: {singleAuthoritiesString}";
    }

    public User(CreateUserDTO myUser)
    {
        this.Authorities = myUser.Authorities;
        this.Email = myUser.Email;
        this.Password = myUser.Password;
        this.Name = myUser.Name;
        this.Role = myUser.Role;
    }
    public User()
    {
        
    }
    public User (User u)
    {
        this.Authorities = u.Authorities;
        this.Email = u.Email;
        this.Password = u.Password;
        this.Name = u.Name;
        this.Role = u.Role;
    }
}