using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCBA.Models;

// Model to handle login
public class Login
{
    [Key, StringLength(8)]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "LoginID must be 8 digits")]
    public string LoginID { get; set; }
    
    [ForeignKey(nameof(Customer))]
    public int CustomerID { get; set; }
    
    [Required, StringLength(94)]
    public string PasswordHash { get; set; }

    [Required] 
    public virtual Customer Customer { get; set; }
}