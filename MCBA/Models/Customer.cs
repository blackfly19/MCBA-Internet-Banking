using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCBA.Models;

// Model to handle Customer
public class Customer
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Range(1000,9999, ErrorMessage = "Customer ID must be 4 digits")]
    public required int CustomerID { get; set; }
    
    [Required(ErrorMessage = "Name is required"), StringLength(50, 
         ErrorMessage = "Name cannot be longer than 50 characters")]
    public required string Name { get; set; }
    
    [StringLength(11), RegularExpression(@"^\d{3} \d{3} \d{3}$", 
        ErrorMessage = "TFN must be in the format XXX XXX XXX")]
    public string TFN { get; set; }
    
    [StringLength(50)]
    public string Address { get; set; }
    
    [StringLength(40)]
    public string City { get; set; }
    
    [StringLength(3), RegularExpression(@"^(VIC|NSW|QLD|TAS|SA|WA|ACT|NT)$", 
        ErrorMessage = "Must be a 2 or 3 lettered Australian state")]
    public string State { get; set; }
    
    [StringLength(4), RegularExpression(@"^\d{4}$", ErrorMessage = "Postcode must be 4 digits")]
    public string PostCode { get; set; }
    
    [StringLength(12), RegularExpression(@"^04\d{2} \d{3} \d{3}$",
        ErrorMessage = "Mobile number must be in the format of 04XX XXX XXX")]
    public string Mobile { get; set; }

    [Required] 
    public virtual ICollection<Account> Accounts { get; set; }
    
    [Required]
    public virtual Login Login { get; set; }

}