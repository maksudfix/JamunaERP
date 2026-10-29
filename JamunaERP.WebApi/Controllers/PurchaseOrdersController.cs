using JamunaERP.Application.Common.Security;
using JamunaERP.Application.PurchaseOrders.Commands.CreatePurchaseOrder;
using JamunaERP.Application.PurchaseOrders.Commands.ReceivePurchaseOrder;
using JamunaERP.Application.PurchaseOrders.DTOs;
using JamunaERP.Application.PurchaseOrders.Queries.GetAllPurchaseOrders;
using JamunaERP.Application.PurchaseOrders.Queries.GetPurchaseOrdersByDealer;
using JamunaERP.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JamunaERP.WebApi.Controllers
{
    [Authorize(Roles = Roles.AdminOrManager)]
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseOrdersController : ControllerBase
    {
        private readonly ISender _mediator;

        public PurchaseOrdersController(ISender mediator)
        {
            _mediator = mediator;
        }

        // GET: api/purchaseorders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PurchaseOrderDto>>> GetAllPurchaseOrders()
        {
            var orders = await _mediator.Send(new GetAllPurchaseOrdersQuery());
            return Ok(orders);
        }

        // GET: api/purchaseorders/dealer/{dealerId}
        [HttpGet("dealer/{dealerId}")]
        public async Task<ActionResult<IEnumerable<PurchaseOrderDto>>> GetOrdersByDealer(int dealerId)
        {
            var orders = await _mediator.Send(new GetPurchaseOrdersByDealerQuery(dealerId));
            return Ok(orders);
        }

        // POST: api/purchaseorders
        [HttpPost]
        public async Task<ActionResult<int>> CreatePurchaseOrder([FromBody] CreatePurchaseOrderCommand command)
        {
            var orderId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetAllPurchaseOrders), new { id = orderId }, orderId);
        }

        // POST: api/purchaseorders/{id}/receive?location=GoDown
        [HttpPost("{id:int}/receive")]
        public async Task<IActionResult> Receive(int id, [FromQuery] StockLocationType location = StockLocationType.GoDown)
        {
            await _mediator.Send(new ReceivePurchaseOrderCommand(id, location));
            return Ok(new { message = "Purchase order received and stock updated." });
        }
    }
}