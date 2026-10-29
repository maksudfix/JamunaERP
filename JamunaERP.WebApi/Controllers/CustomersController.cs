using JamunaERP.Application.Common.Models;
using JamunaERP.Application.Common.Security;
using JamunaERP.Application.Customers.Commands.CreateCustomer;
using JamunaERP.Application.Customers.Commands.ManageCustomer;
using JamunaERP.Application.Customers.DTOs;
using JamunaERP.Application.Customers.Queries.GetAllCustomers;
using JamunaERP.Application.Customers.Queries.GetCustomersPaged;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JamunaERP.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ISender _mediator;

        public CustomersController(ISender mediator)
        {
            _mediator = mediator;
        }

        // GET: api/customers?includeInactive=true
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAllCustomers([FromQuery] bool includeInactive = false)
        {
            // Only Admin / Manager may see deactivated customers
            var canSeeInactive = includeInactive &&
                (User.IsInRole(Roles.Admin) || User.IsInRole(Roles.Manager));

            var customers = await _mediator.Send(new GetAllCustomersQuery(canSeeInactive));
            return Ok(customers);
        }

        // GET: api/customers/paged?page=1&pageSize=20&search=rahim
        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<CustomerDto>>> GetCustomersPaged(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null, [FromQuery] bool includeInactive = false)
        {
            var canSeeInactive = includeInactive &&
                (User.IsInRole(Roles.Admin) || User.IsInRole(Roles.Manager));
            var result = await _mediator.Send(new GetCustomersPagedQuery(page, pageSize, search, canSeeInactive));
            return Ok(result);
        }

        // POST: api/customers
        [HttpPost]
        public async Task<ActionResult<int>> CreateCustomer([FromBody] CreateCustomerCommand command)
        {
            var customerId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetAllCustomers), new { id = customerId }, customerId);
        }

        // PUT: api/customers/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerCommand command)
        {
            await _mediator.Send(command with { Id = id });
            return NoContent();
        }

        // DELETE: api/customers/5  (soft delete = deactivate)
        [Authorize(Roles = Roles.AdminOrManager)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeactivateCustomer(int id)
        {
            await _mediator.Send(new SetCustomerActiveCommand(id, false));
            return NoContent();
        }

        // POST: api/customers/5/reactivate
        [Authorize(Roles = Roles.AdminOrManager)]
        [HttpPost("{id:int}/reactivate")]
        public async Task<IActionResult> ReactivateCustomer(int id)
        {
            await _mediator.Send(new SetCustomerActiveCommand(id, true));
            return NoContent();
        }
    }
}