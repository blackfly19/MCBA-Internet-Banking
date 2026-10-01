using System.ComponentModel.DataAnnotations;
using MCBA.Models;

namespace MCBA.ViewModels;

public class BillPayViewModel
{
    public int BillPayID { get; set; }

    [Required(ErrorMessage = "Please select an account")]
    public int AccountNumber { get; set; }

    [Required(ErrorMessage = "Please select a payee")]
    public int PayeeID { get; set; }

    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Schedule date and time is required")]
    public DateTime ScheduleTimeUtc { get; set; }

    [Required(ErrorMessage = "Please select payment frequency")]
    public char Period { get; set; }

    // For display
    public string AccountType { get; set; }
    public string PayeeName { get; set; }
    public string Status { get; set; }
    
    // For dropdowns
    public List<Account>? Accounts { get; set; }
    public List<Payee>? Payees { get; set; }
}