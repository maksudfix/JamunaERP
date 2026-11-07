using JamunaERP.Application.SaleItems.Queries.GetAllSaleItems;
using JamunaERP.Application.SaleItems.Queries.GetSaleItemsBySale;
using JamunaERP.Application.Sales.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JamunaERP.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SaleItemsController : ControllerBase
    {
        private readonly ISender _mediator;

        public SaleItemsController(ISender mediator)
        {
            _mediator = mediator;
        }

        // GET: api/saleitems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SaleItemDto>>> GetAllSaleItems()
        {
            var items = await _mediator.Send(new GetAllSaleItemsQuery());
            return Ok(items);
        }

        // GET: api/saleitems/sale/{saleId}
        [HttpGet("sale/{saleId}")]
        public async Task<ActionResult<IEnumerable<SaleItemDto>>> GetItemsBySale(int saleId)
        {
            var items = await _mediator.Send(new GetSaleItemsBySaleQuery(saleId));
            return Ok(items);
        }
    }
}