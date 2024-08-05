using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotel_system_backend.Models;

public class Authorities
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long AuthorityId { get; set; } 
    
    [Required]
    public string Name { get; set; }
}