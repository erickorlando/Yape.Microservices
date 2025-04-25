using MediatR;
using Microsoft.AspNetCore.Mvc;
using Yape.TransactionService.API.Models;
using Yape.TransactionService.Application.UseCases;
using Yape.TransactionService.Application.UseCases.CreateTransaction;

namespace Yape.TransactionService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var command = new CreateTransactionCommand()
        {
            SourceAccountId = request.SourceAccountId,
            TargetAccountId = request.TargetAccountId,
            TranferTypeId = request.TranferTypeId,
            Value = request.Value
        };

        var result = await _mediator.Send(command);
        
        return Ok(result);
    }
}