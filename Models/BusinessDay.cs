using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotel_system_backend.Models;

public class BusinessDay
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    [Required]
    public DateTime Date { get; set; }
    [Required]
    public bool IsClosed { get; set; }

    public static DateTime UpdateToActualTime(DateTime time)
    {
        time = new DateTime(time.Year, time.Month, time.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute,
            DateTime.UtcNow.Second,DateTime.UtcNow.Millisecond,DateTimeKind.Utc);

        return time;
    }
}