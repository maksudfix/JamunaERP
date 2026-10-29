using JamunaERP.Application.Common.Security;
using JamunaERP.Application.Expenses.Commands.CreateExpense;
using JamunaERP.Application.Expenses.Commands.DeleteExpense;
using JamunaERP.Application.Expenses.Commands.UpdateExpense;
using JamunaERP.Application.Expenses.DTOs;
using JamunaERP.Application.Expenses.Queries.GetExpenses;
using JamunaERP.Application.Expenses.Queries.GetExpensesPaged;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JamunaERP.WebApi.Controllers;

[Authorize(Roles = Roles.AdminOrManager)]
[Route("api/[controller]")]
[ApiController]
public class ExpensesController(ISender mediator) : ControllerBase
{
    // GET: api/expenses?from=2026-10-01&to=2026-10-31&category=Rent
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpenseDto>>> GetExpenses(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? category)
    {
        var result = await mediator.Send(
            new GetExpensesQuery(from, to, category));

        return Ok(result);
    }

    // GET: api/expenses/paged?page=1&pageSize=20
    // Optional: search, category, from, to
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged(
        [FromQuery] GetExpensesPagedQuery query,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            query,
            cancellationToken);

        return Ok(result);
    }

    // POST: api/expenses
    [HttpPost]
    public async Task<ActionResult<int>> CreateExpense(
        [FromBody] CreateExpenseCommand command)
    {
        var id = await mediator.Send(command);

        return CreatedAtAction(
            nameof(GetExpenses),
            new { id },
            id);
    }

    // PUT: api/expenses/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateExpense(
        int id,
        [FromBody] UpdateExpenseCommand command)
    {
        await mediator.Send(command with { Id = id });

        return NoContent();
    }

    // DELETE: api/expenses/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteExpense(int id)
    {
        await mediator.Send(
            new DeleteExpenseCommand(id));

        return NoContent();
    }
}