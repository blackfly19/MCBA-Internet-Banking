using MCBA_AdminWebApi.Models.DataManager;

namespace MCBA_AdminWebApi.Models.Repository;

public interface IPayeeRepository
{
    List<Payee> GetAllPayees();
    List<Payee> GetPayeesByPostcode(string postcode);
    Payee? GetPayeeById(int id);
    Payee UpdatePayee(Payee payee);
}