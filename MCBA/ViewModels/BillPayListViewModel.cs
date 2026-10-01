using System.ComponentModel.DataAnnotations;
using MCBA.Models;

namespace MCBA.ViewModels;

public class BillPayListViewModel
{
    public List<BillPay> ScheduledBills { get; set; } = new();
    public List<Account> Accounts { get; set; } = new();
}