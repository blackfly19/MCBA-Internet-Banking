using MCBA.Models;

namespace MCBA.ViewModels;

public class StatementViewModel
{
    public int AccountNumber { get; set; }
    public char AccountType { get; set; }
    public decimal Balance { get; set; }
    
    // Available balance considers minimum balance rules
    public decimal AvailableBalance { get; set; }
    
    public List<Transaction> Transactions { get; set; } = new();
    
    // Pagination
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int PageSize { get; set; } = 4;
    
    // For account selection
    public List<Account>? Accounts { get; set; }
}