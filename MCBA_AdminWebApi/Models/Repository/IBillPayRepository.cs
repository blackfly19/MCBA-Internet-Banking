namespace MCBA_AdminWebApi.Models.Repository;

public interface IBillPayRepository
{
    List<BillPay> GetAllBillPays();
    BillPay? GetBillPayById(int id);
    BillPay BlockBillPay(int id);
    BillPay UnblockBillPay(int id);
}