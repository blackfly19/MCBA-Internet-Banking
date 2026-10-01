using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCBA_AdminWebApi.Models;

public class BillPay
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int BillPayID { get; set; }
    
    [ForeignKey("Account")]
    public int AccountNumber { get; set; }
    
    [ForeignKey("Payee")]
    public int PayeeID { get; set; }
    
    [Required, Column(TypeName = "money")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be a positive value")]
    public decimal Amount { get; set; }
    
    [Required, Column(TypeName = "datetime2")]
    public DateTime ScheduleTimeUtc {  get; set; }
    
    [Required]
    public char Period { get; set; }

    [Required] 
    public string Status { get; set; } = "Pending";
}