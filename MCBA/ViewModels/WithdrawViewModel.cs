using System.ComponentModel.DataAnnotations;
using MCBA.Models;

namespace MCBA.ViewModels;

public class WithdrawViewModel
{
    [Required(ErrorMessage = "Please select an account")]
    public int AccountNumber { get; set; }

    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
    public decimal Amount { get; set; }

    [StringLength(30, ErrorMessage = "Comment cannot exceed 30 characters")]
    public string Comment { get; set; }

    public string AccountType { get; set; }
    public decimal ServiceCharge { get; set; }
    public decimal TotalAmount { get; set; }
    
    public List<Account> Accounts { get; set; }
}