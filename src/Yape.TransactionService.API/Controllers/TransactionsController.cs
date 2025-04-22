using Microsoft.AspNetCore.Mvc;
using Yape.TransactionService.Application.UseCases;

namespace Yape.TransactionService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly Application.Services.TransactionService _service;

    public TransactionsController(Application.Services.TransactionService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransactionCommand command)
    {
        var id = await _service.CreateTransactionAsync(command);
        return Ok(new { TransactionId = id });
    }
}