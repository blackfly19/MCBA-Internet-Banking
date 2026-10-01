using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCBA.Models;

// Model to handle accounts
public class Account
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Range(1000,9999, ErrorMessage = "Account Number must be 4 digits")]
    public int AccountNumber { get; set; }
    
    [Required]
    public char AccountType { get; set; }
    
    [ForeignKey(nameof(Customer))]
    public int CustomerID { get; set; }
    
    [Required, Column(TypeName = "money"), DataType(DataType.Currency)]
    public decimal Balance { get; set; }
    
    public int FreeTransactions { get; set; }
    
    public virtual Customer Customer { get; set; }

    [InverseProperty(nameof(Transaction.Account))]
    public virtual ICollection<Transaction> Transactions { get; set; }

    public virtual ICollection<BillPay> BillPays { get; set; }
}