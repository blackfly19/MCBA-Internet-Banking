using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Transactions;

namespace MCBA.Models;

// Model to handle transactions
public class Transaction
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TransactionID { get; set; }
    
    [Required]
    public char TransactionType { get; set; }
    
    [ForeignKey(nameof(Account))]
    public int AccountNumber { get; set; }
    
    [ForeignKey(nameof(DestinationAccount))]
    public int? DestinationAccountNumber { get; set; }
    
    [Required, Column(TypeName = "money")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be a positive value")]
    public decimal Amount {get; set;}
    
    [StringLength(50)]
    public string Comment {get; set;}

    [Required, Column(TypeName = "datetime2")]
    public DateTime TransactionTimeUtc {get; set;}
    
    [Required]
    public virtual Account Account { get; set; }
    
    public virtual Account DestinationAccount { get; set; }
}