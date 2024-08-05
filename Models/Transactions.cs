using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotel_system_backend.Models;

public class Transactions
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long TransactionId { get; set; } 
    
    [Required]
    public DateTime ExactDate { get; set; }

    [Required]
    public long BusinessDayId { get; set; }
    
    [ForeignKey("BusinessDayId")]
    public BusinessDay BusinessDay { get; set; }
    
    [Required]
    public string TransactionType { get; set; }
    
    [Required]
    public string Description { get; set; }
    
    [Required]
    public long ReservationNumber { get; set; }
    
    [ForeignKey("ReservationNumber")]
    public Reservation Reservation { get; set; }
    
    [Required]
    public long UserId { get; set; }
    
    [ForeignKey("UserId")]
    public User User { get; set; }
    
    [Required]
    public string CreditOrDebit { get; set; }
    
    [Required]
    public double Value { get; set; }
    
    public string? Status { get; set; }
}