using System.ComponentModel.DataAnnotations;
using MCBA.Models;

namespace MCBA.ViewModels;

public class DepositViewModel
{
    [Required(ErrorMessage = "Please select an account")]
    public int AccountNumber { get; set; }

    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
    public decimal Amount { get; set; }

    [StringLength(30, ErrorMessage = "Comment cannot exceed 30 characters")]
    public string Comment { get; set; }

    // For display purposes
    public string AccountType { get; set; }
    
    // List of available accounts for the dropdown
    public List<Account> Accounts { get; set; }
}