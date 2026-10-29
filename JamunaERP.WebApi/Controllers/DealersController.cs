using JamunaERP.Application.Common.Security;
using JamunaERP.Application.Dealers.Commands.CreateDealer;
using JamunaERP.Application.Dealers.Commands.ManageDealer;
using JamunaERP.Application.Dealers.DTOs;
using JamunaERP.Application.Dealers.Queries.GetAllDealers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JamunaERP.WebApi.Controllers
{
    [Authorize(Roles = Roles.AdminOrManager)]
    [Route("api/[controller]")]
    [ApiController]
    public class DealersController : ControllerBase
    {
        private readonly ISender _mediator;

        public DealersController(ISender mediator)
        {
            _mediator = mediator;
        }

        // GET: api/dealers?includeInactive=true
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DealerDto>>> GetAllDealers([FromQuery] bool includeInactive = false)
        {
            var dealers = await _mediator.Send(new GetAllDealersQuery(includeInactive));
            return Ok(dealers);
        }

        // POST: api/dealers
        [HttpPost]
        public async Task<ActionResult<int>> CreateDealer([FromBody] CreateDealerCommand command)
        {
            var dealerId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetAllDealers), new { id = dealerId }, dealerId);
        }

        // PUT: api/dealers/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateDealer(int id, [FromBody] UpdateDealerCommand command)
        {
            await _mediator.Send(command with { Id = id });
            return NoContent();
        }

        // DELETE: api/dealers/5  (soft delete = deactivate)
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeactivateDealer(int id)
        {
            await _mediator.Send(new SetDealerActiveCommand(id, false));
            return NoContent();
        }

        // POST: api/dealers/5/reactivate
        [HttpPost("{id:int}/reactivate")]
        public async Task<IActionResult> ReactivateDealer(int id)
        {
            await _mediator.Send(new SetDealerActiveCommand(id, true));
            return NoContent();
        }
    }
}