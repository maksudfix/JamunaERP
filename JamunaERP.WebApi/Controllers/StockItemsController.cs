using JamunaERP.Application.StockItems.Commands.CreateStockItem;
using JamunaERP.Application.StockItems.DTOs;
using JamunaERP.Application.Common.Security;
using JamunaERP.Application.StockItems.Commands.MoveStock;
using JamunaERP.Application.StockItems.Queries.GetAllStockItems;
using JamunaERP.Application.StockItems.Queries.GetStockByLocation;
using JamunaERP.Application.StockItems.Queries.GetStockByProduct;
using JamunaERP.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JamunaERP.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StockItemsController : ControllerBase
    {
        private readonly ISender _mediator;

        public StockItemsController(ISender mediator)
        {
            _mediator = mediator;
        }

        // GET: api/stockitems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockItemDto>>> GetAllStockItems()
        {
            var result = await _mediator.Send(new GetAllStockItemsQuery());
            return Ok(result);
        }

        // GET: api/stockitems/product/{productId}
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<StockItemDto>>> GetStockByProduct(int productId)
        {
            var result = await _mediator.Send(new GetStockByProductQuery(productId));
            return Ok(result);
        }

        // GET: api/stockitems/location/{location}
        [HttpGet("location/{location}")]
        public async Task<IActionResult> GetByLocation(string location)
        {
            if (!Enum.TryParse<StockLocationType>(location, true, out var locationType))
            {
                return BadRequest(new { message = $"Invalid location type: '{location}'." });
            }

            var result = await _mediator.Send(new GetStockByLocationQuery(locationType));
            return Ok(result);
        }

        // POST: api/stockitems
        [Authorize(Roles = Roles.AdminOrManager)]
        [HttpPost]
        public async Task<ActionResult<int>> CreateStockItem([FromBody] CreateStockItemCommand command)
        {
            var stockItemId = await _mediator.Send(command);
            return Ok(stockItemId);
        }

        // POST: api/stockitems/move  (partial quantity between locations)
        [Authorize(Roles = Roles.AdminOrManager)]
        [HttpPost("move")]
        public async Task<IActionResult> MoveStock([FromBody] MoveStockCommand command)
        {
            await _mediator.Send(command);
            return Ok(new { success = true, message = "Stock moved successfully." });
        }
    }
}