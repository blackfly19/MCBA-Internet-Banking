using MCBA_AdminWebApi.Models;
using MCBA_AdminWebApi.Models.DataManager;
using Microsoft.EntityFrameworkCore;

namespace MCBA_AdminWebApi.Data;

public class MCBAContext : DbContext
{
    public MCBAContext(DbContextOptions<MCBAContext> options) : base(options) { }
    
    public DbSet<Payee> Payees { get; set; }
    public DbSet<BillPay> BillPays { get; set; }
}