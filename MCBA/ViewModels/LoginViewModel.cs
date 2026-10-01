using System.ComponentModel.DataAnnotations;

namespace MCBA.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Login ID is required")]
    [StringLength(8, ErrorMessage = "Login ID must be 8 characters long ")]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "Login ID must be 8 digits")]
    public string LoginID { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
    
}