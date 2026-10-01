using MCBA_AdminWebApi.Data;
using MCBA_AdminWebApi.Models.Repository;
using Microsoft.EntityFrameworkCore;

namespace MCBA_AdminWebApi.Models.DataManager;

public class BillPayManager : IBillPayRepository
{
    private readonly MCBAContext _context;

    public BillPayManager(MCBAContext context)
    {
        _context = context;
    }

    public List<BillPay> GetAllBillPays()
    {
        return _context.BillPays.ToList();
    }

    public BillPay? GetBillPayById(int id)
    {
        return _context.BillPays.FirstOrDefault(b => b.BillPayID == id);
    }

    public BillPay BlockBillPay(int id)
    {
        var billPay = _context.BillPays.Find(id);
        if (billPay == null)
            throw new KeyNotFoundException($"BillPay with ID {id} not found");
        
        billPay.Status = "Blocked";
        _context.SaveChanges();
        return billPay;
    }

    public BillPay UnblockBillPay(int id)
    {
        var billPay = _context.BillPays.Find(id);
        if (billPay == null)
            throw new KeyNotFoundException($"BillPay with ID {id} not found");
        
        billPay.Status = "Pending";
        _context.SaveChanges();
        return billPay;
    }
}