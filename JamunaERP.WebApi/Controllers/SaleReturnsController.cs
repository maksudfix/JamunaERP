using JamunaERP.Application.Common.Security;
using JamunaERP.Application.SaleReturns.Commands.CreateSaleReturn;
using JamunaERP.Application.SaleReturns.DTOs;
using JamunaERP.Application.SaleReturns.Queries.GetSaleReturns;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JamunaERP.WebApi.Controllers
{
    // Returns move money and stock, so only Admin / Manager can process them.
    [Authorize(Roles = Roles.AdminOrManager)]
    [Route("api/[controller]")]
    [ApiController]
    public class SaleReturnsController : ControllerBase
    {
        private readonly ISender _mediator;

        public SaleReturnsController(ISender mediator)
        {
            _mediator = mediator;
        }

        // GET: api/salereturns            (all returns)
        // GET: api/salereturns?saleId=5   (returns of one sale)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SaleReturnDto>>> GetSaleReturns([FromQuery] int? saleId)
        {
            var result = await _mediator.Send(new GetSaleReturnsQuery(saleId));
            return Ok(result);
        }

        // POST: api/salereturns
        [HttpPost]
        public async Task<ActionResult<SaleReturnResult>> CreateSaleReturn([FromBody] CreateSaleReturnCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}