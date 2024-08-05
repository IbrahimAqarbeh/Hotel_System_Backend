using System.ComponentModel.DataAnnotations;

namespace hotel_system_backend.Models.DTOs;

public class UpdateRoomDTO
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
}
