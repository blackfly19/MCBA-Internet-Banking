using MCBA_AdminWebApi.Models;
using MCBA_AdminWebApi.Models.Repository;
using MCBA_AdminWebApi.Filters;
using Microsoft.AspNetCore.Mvc;

namespace MCBA_AdminWebApi.Controllers;

[ApiKeyAuth]
[Route("api/[controller]")]
[ApiController]
public class BillPayController : ControllerBase
{
    private readonly IBillPayRepository _billPayRepository;

    public BillPayController(IBillPayRepository billPayRepository)
    {
        _billPayRepository = billPayRepository;
    }

    [HttpGet]
    public ActionResult<List<BillPay>> GetAllBillPays()
    {
        return Ok(_billPayRepository.GetAllBillPays());
    }

    [HttpGet("{id}")]
    public ActionResult<BillPay> GetBillPay(int id)
    {
        var billPay = _billPayRepository.GetBillPayById(id);
        if(billPay == null)
            return NotFound();
        
        return Ok(billPay);
    }

    [HttpPost("block/{id}")]
    public ActionResult BlockBillPay(int id)
    {
        try
        {
            var billPay = _billPayRepository.BlockBillPay(id);
            return Ok(billPay);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
    
    [HttpPost("unblock/{id}")]
    public ActionResult UnblockBillPay(int id)
    {
        try
        {
            var billPay = _billPayRepository.UnblockBillPay(id);
            return Ok(billPay);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}