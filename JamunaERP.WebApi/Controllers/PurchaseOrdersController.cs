using JamunaERP.Application.Common.Security;
using JamunaERP.Application.PurchaseOrders.Commands.CreatePurchaseOrder;
using JamunaERP.Application.PurchaseOrders.Commands.ReceivePurchaseOrder;
using JamunaERP.Application.PurchaseOrders.DTOs;
using JamunaERP.Application.PurchaseOrders.Queries.GetAllPurchaseOrders;
using JamunaERP.Application.PurchaseOrders.Queries.GetPurchaseOrdersByDealer;
using JamunaERP.Application.PurchaseOrders.Queries.GetPurchaseOrdersPaged;
using JamunaERP.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JamunaERP.WebApi.Controllers;

[Authorize(Roles = Roles.AdminOrManager)]
[Route("api/[controller]")]
[ApiController]
public class PurchaseOrdersController(ISender mediator) : ControllerBase
{
    // GET: api/purchaseorders
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PurchaseOrderDto>>> GetAllPurchaseOrders()
    {
        var orders = await mediator.Send(
            new GetAllPurchaseOrdersQuery());

        return Ok(orders);
    }

    // GET: api/purchaseorders/paged?page=1&pageSize=20
    // Optional: search, dealerId, from, to
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged(
        [FromQuery] GetPurchaseOrdersPagedQuery query,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            query,
            cancellationToken);

        return Ok(result);
    }

    // GET: api/purchaseorders/dealer/5
    [HttpGet("dealer/{dealerId:int}")]
    public async Task<ActionResult<IEnumerable<PurchaseOrderDto>>> GetOrdersByDealer(int dealerId)
    {
        var orders = await mediator.Send(
            new GetPurchaseOrdersByDealerQuery(dealerId));

        return Ok(orders);
    }

    // POST: api/purchaseorders
    [HttpPost]
    public async Task<ActionResult<int>> CreatePurchaseOrder(
        [FromBody] CreatePurchaseOrderCommand command)
    {
        var orderId = await mediator.Send(command);

        return CreatedAtAction(
            nameof(GetAllPurchaseOrders),
            new { id = orderId },
            orderId);
    }

    // POST: api/purchaseorders/5/receive?location=GoDown
    [HttpPost("{id:int}/receive")]
    public async Task<IActionResult> Receive(
        int id,
        [FromQuery] StockLocationType location =
            StockLocationType.GoDown)
    {
        await mediator.Send(
            new ReceivePurchaseOrderCommand(id, location));

        return Ok(new
        {
            message = "Purchase order received and stock updated."
        });
    }
}