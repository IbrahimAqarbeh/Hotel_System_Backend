using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotel_system_backend.Models;

public class DeletedTransactions
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; } 
    
    [Required]
    public long TransactionId { get; set; }
    
    [Required]
    [ForeignKey("TransactionId")]
    public Transactions Transactions { get; set; }
    
    [Required]
    public long UserId { get; set; }
    
    [ForeignKey("UserId")]
    [Required]
    public User User { get; set; }
    
    [Required]
    public DateTime ExactDate { get; set; }
    
    [Required]
    public long BusinessDayId { get; set; }
    
    [ForeignKey("BusinessDayId")]
    public BusinessDay BusinessDay { get; set; }
}