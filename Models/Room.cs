using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotel_system_backend.Models;

public class Room
{
    [Key]
    public string RoomNumber { get; set; }
    
    [Required]
    public string Description { get; set; }
    
    [Required]
    public string Type { get; set; }
    
    [Required]
    public string Note { get; set; }
    
    [Required]
    public bool isOccupied { get; set; }
    
    [Required]
    public bool isDirty { get; set; }
    
    [Required]
    public bool isOutOfOrder { get; set; }
    
    [Required]
    public bool isReserved { get; set; }
    
    [Required]
    public long ReservationNumber { get; set; }
    
    [ForeignKey("ReservationNumber")]
    public Reservation? Reservation { get; set; }
}