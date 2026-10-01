using MCBA_AdminWebApi.Data;
using MCBA_AdminWebApi.Models.Repository;

namespace MCBA_AdminWebApi.Models.DataManager;

public class PayeeManager : IPayeeRepository
{
    private readonly MCBAContext _context;

    public PayeeManager(MCBAContext context)
    {
        _context = context;
    }

    public List<Payee> GetAllPayees()
    {
        return _context.Payees.ToList();
    }

    public List<Payee> GetPayeesByPostcode(string postcode)
    {
        return _context.Payees.Where(p => p.Postcode == postcode).ToList();
    }

    public Payee? GetPayeeById(int id)
    {
        return _context.Payees.Find(id);
    }

    public Payee UpdatePayee(Payee payee)
    {
        _context.Payees.Update(payee);
        _context.SaveChanges();
        return payee;
    }
}