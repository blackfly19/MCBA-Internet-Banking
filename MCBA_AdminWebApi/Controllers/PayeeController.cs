using MCBA_AdminWebApi.Filters;
using MCBA_AdminWebApi.Models;
using MCBA_AdminWebApi.Models.DataManager;
using MCBA_AdminWebApi.Models.Repository;
using Microsoft.AspNetCore.Mvc;

namespace MCBA_AdminWebApi.Controllers;

[ApiKeyAuth]
[Route("api/[controller]")]
[ApiController]
public class PayeeController : ControllerBase
{
    private readonly IPayeeRepository _payeeRepository;

    public PayeeController(IPayeeRepository payeeRepository)
    {
        _payeeRepository = payeeRepository;
    }

    [HttpGet]
    public ActionResult<List<Payee>> GetAllPayees()
    {
        return Ok(_payeeRepository.GetAllPayees());
    }

    [HttpGet("postcode/{postcode}")]
    public ActionResult<List<Payee>> GetAllPayeesByPostcode(string postcode)
    {
        return Ok(_payeeRepository.GetPayeesByPostcode(postcode));
    }

    [HttpGet("{id}")]
    public ActionResult<Payee> GetPayeeById(int id)
    {
        var payee = _payeeRepository.GetPayeeById(id);
        if(payee == null)
            return NotFound();
        
        return Ok(payee);
    }
    
    [HttpPut("{id}")]
    public IActionResult UpdatePayee(int id, Payee payee)
    {
        if (id != payee.PayeeID)
            return BadRequest("ID mismatch");

        try
        {
            var updated = _payeeRepository.UpdatePayee(payee);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}