using JamunaERP.Application.Common.Security;
using JamunaERP.Application.DealerReturns.Commands.CreateDealerReturn;
using JamunaERP.Application.DealerReturns.DTOs;
using JamunaERP.Application.DealerReturns.Queries.GetDealerReturns;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JamunaERP.WebApi.Controllers
{
    [Authorize(Roles = Roles.AdminOrManager)]
    [Route("api/[controller]")]
    [ApiController]
    public class DealerReturnsController : ControllerBase
    {
        private readonly ISender _mediator;

        public DealerReturnsController(ISender mediator)
        {
            _mediator = mediator;
        }

        // GET: api/dealerreturns/dealer/{dealerId}
        [HttpGet("dealer/{dealerId:int}")]
        public async Task<ActionResult<IEnumerable<DealerReturnDto>>> GetByDealer(int dealerId)
        {
            var result = await _mediator.Send(new GetDealerReturnsQuery(dealerId));
            return Ok(result);
        }

        // POST: api/dealerreturns
        [HttpPost]
        public async Task<ActionResult<int>> CreateDealerReturn([FromBody] CreateDealerReturnCommand command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetByDealer), new { dealerId = command.DealerId }, id);
        }
    }
}