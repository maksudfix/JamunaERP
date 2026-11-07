using JamunaERP.Application.Common.Security;
using JamunaERP.Application.StockAdjustments.Commands.AdjustStock;
using JamunaERP.Application.StockAdjustments.DTOs;
using JamunaERP.Application.StockAdjustments.Queries.GetStockAdjustments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JamunaERP.WebApi.Controllers
{
    [Authorize(Roles = Roles.AdminOrManager)]
    [Route("api/[controller]")]
    [ApiController]
    public class StockAdjustmentsController : ControllerBase
    {
        private readonly ISender _mediator;

        public StockAdjustmentsController(ISender mediator)
        {
            _mediator = mediator;
        }

        // GET: api/stockadjustments?productId=5
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockAdjustmentDto>>> GetAdjustments([FromQuery] int? productId)
        {
            var result = await _mediator.Send(new GetStockAdjustmentsQuery(productId));
            return Ok(result);
        }

        // POST: api/stockadjustments
        [HttpPost]
        public async Task<ActionResult<int>> AdjustStock([FromBody] AdjustStockCommand command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetAdjustments), new { productId = command.ProductId }, id);
        }
    }
}