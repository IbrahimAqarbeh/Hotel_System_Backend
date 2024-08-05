using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotel_system_backend.Models;

public class Record
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long RecordNumber { get; set; }
    
    [Required]
    public long UserId { get; set; }
    
    [ForeignKey("UserId")]
    public User User { get; set; }
    
    
    [Required]
    public DateTime DateTimeOfRecord { get; set; }
    
    
    
    [Required]
    public string Message { get; set; }

    public Record(long userId,  string message)
    {
        UserId = userId;
        DateTimeOfRecord = DateTime.Now;
        Message = message;
    }
}