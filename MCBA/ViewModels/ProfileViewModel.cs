using System.ComponentModel.DataAnnotations;

namespace MCBA.ViewModels;

public class ProfileViewModel
{
    public int CustomerID { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
    public string Name { get; set; }

    [StringLength(11, ErrorMessage = "TFN must be 11 characters")]
    [RegularExpression(@"^\d{3} \d{3} \d{3}$", ErrorMessage = "TFN must be in format: XXX XXX XXX")]
    public string TFN { get; set; }

    [StringLength(50, ErrorMessage = "Address cannot exceed 50 characters")]
    public string Address { get; set; }

    [StringLength(40, ErrorMessage = "City cannot exceed 40 characters")]
    public string City { get; set; }

    [StringLength(3, ErrorMessage = "State must be 2-3 characters")]
    [RegularExpression(@"^(ACT|NSW|NT|QLD|SA|TAS|VIC|WA)$", ErrorMessage = "Must be a valid Australian state")]
    public string State { get; set; }

    [StringLength(4, MinimumLength = 4, ErrorMessage = "Postcode must be 4 digits")]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "Postcode must be 4 digits")]
    public string PostCode { get; set; }

    [StringLength(12, ErrorMessage = "Mobile must be 12 characters")]
    [RegularExpression(@"^04\d{2} \d{3} \d{3}$", ErrorMessage = "Mobile must be in format: 04XX XXX XXX")]
    public string Mobile { get; set; }
}