using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotel_system_backend.Models;

public class Reservation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ReservationNumber { get; set; } 
    
    public long? UserId { get; set; }
    
    [ForeignKey("UserId")]
    public User? User { get; set; }
    
    
    public int countOfRoomsReserved { get; set; }
    
    public DateTime? ReservedOn { get; set; }
    
    
    
    [Required]
    public DateTime CheckIn { get; set; }

    [Required] 
    public DateTime CheckOut { get; set; }
    
    public string? Status { get; set; }
    
    public string? StatusMessage { get; set; }
    
    
    [Required]
    public string MealPlan { get; set; }
    
    [Required]
    public string Source { get; set; }
    
    [Required]
    public Double Price { get; set; }
    
    [Required]
    public int Pax { get; set; }
}