using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotel_system_backend.Models;

public class Guest
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long GuestId { get; set; } 
    
    [Required]
    public string Name { get; set; }
    
    [Required]
    public string Nationality { get; set; }
    
    [Required]
    public string DocumentId { get; set; }
    
    [Required]
    public string DocumentType { get; set; }

    [Required]
    public string Gender { get; set; }
    
    [Required]
    public string BirthPlace { get; set; }
    
    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    public long ReservationNumber { get; set; }
    
    [ForeignKey("ReservationNumber")]
    public Reservation? Reservation { get; set; }
    
}