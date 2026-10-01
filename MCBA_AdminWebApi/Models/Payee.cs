using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCBA_AdminWebApi.Models;

public class Payee
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PayeeID { get; set; }
    
    [Required, StringLength(50)]
    public string Name { get; set; }
    
    [Required, StringLength(50)]
    public string Address { get; set; }
    
    [Required, StringLength(40)]
    public string City { get; set; }
    
    [Required, StringLength(3), RegularExpression(@"^(VIC|NSW|QLD|TAS|SA|WA|ACT|NT)$", 
        ErrorMessage = "Must be a 2 or 3 lettered Australian state")]
    public string State { get; set; }
    
    [Required, StringLength(4), RegularExpression(@"^\d{4}$", 
         ErrorMessage = "Postcode must be 4 digits")]
    public string Postcode { get; set; }
    
    [Required, StringLength(14), RegularExpression(@"^\(0\d\) \d{4} \d{4}$", 
        ErrorMessage = "Phone must be in format (0X) XXXX XXXX")]
    public string Phone { get; set; }
}