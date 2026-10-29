using JamunaERP.Application.Common.Security;
using JamunaERP.Application.PurchaseOrderItems.Commands.CreatePurchaseOrderItem;
using JamunaERP.Application.PurchaseOrderItems.Queries.GetAllPurchaseOrderItems;
using JamunaERP.Application.PurchaseOrderItems.Queries.GetPurchaseOrderItemsByOrder;
using JamunaERP.Application.PurchaseOrders.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JamunaERP.WebApi.Controllers
{
    [Authorize(Roles = Roles.AdminOrManager)]
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseOrderItemsController : ControllerBase
    {
        private readonly ISender _mediator;

        public PurchaseOrderItemsController(ISender mediator)
        {
            _mediator = mediator;
        }

        // GET: api/purchaseorderitems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PurchaseOrderItemDto>>> GetAllPurchaseOrderItems()
        {
            var items = await _mediator.Send(new GetAllPurchaseOrderItemsQuery());
            return Ok(items);
        }

        // GET: api/purchaseorderitems/purchaseorder/{purchaseOrderId}
        [HttpGet("purchaseorder/{purchaseOrderId}")]
        public async Task<ActionResult<IEnumerable<PurchaseOrderItemDto>>> GetItemsByPurchaseOrder(int purchaseOrderId)
        {
            var items = await _mediator.Send(new GetPurchaseOrderItemsByOrderQuery(purchaseOrderId));
            return Ok(items);
        }

        // POST: api/purchaseorderitems
        [HttpPost]
        public async Task<ActionResult<int>> CreatePurchaseOrderItem([FromBody] CreatePurchaseOrderItemCommand command)
        {
            var itemId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetAllPurchaseOrderItems), new { id = itemId }, itemId);
        }
    }
}