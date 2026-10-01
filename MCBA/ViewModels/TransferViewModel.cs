using System.ComponentModel.DataAnnotations;
using MCBA.Models;

namespace MCBA.ViewModels;

public class TransferViewModel
{
    [Required(ErrorMessage = "Please select a source account")]
    public int SourceAccountNumber { get; set; }

    [Required(ErrorMessage = "Destination account number is required")]
    [Range(1000, 9999, ErrorMessage = "Account number must be 4 digits")]
    public int DestinationAccountNumber { get; set; }

    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
    public decimal Amount { get; set; }

    [StringLength(50, ErrorMessage = "Comment cannot exceed 50 characters")]
    public string Comment { get; set; }

    public string SourceAccountType { get; set; }
    public string DestinationAccountType { get; set; }
    public decimal ServiceCharge { get; set; }
    public decimal TotalAmount { get; set; }
    
    // List of available source accounts
    public List<Account> Accounts { get; set; }
}