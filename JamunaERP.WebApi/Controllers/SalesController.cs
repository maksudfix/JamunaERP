using JamunaERP.Application.Common.Models;
using JamunaERP.Application.Common.Security;
using JamunaERP.Application.SaleReturns.Commands.CreateSaleReturn;
using JamunaERP.Application.Sales.Commands.CreateSale;
using JamunaERP.Application.Sales.Commands.VoidSale;
using JamunaERP.Application.Sales.DTOs;
using JamunaERP.Application.Sales.Queries.GetAllSales;
using JamunaERP.Application.Sales.Queries.GetInvoice;
using JamunaERP.Application.Sales.Queries.GetSalesByCustomer;
using JamunaERP.Application.Sales.Queries.GetSalesPaged;
using JamunaERP.Domain.Enums;
using JamunaERP.WebApi.Printing;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JamunaERP.WebApi.Controllers
{
    public record VoidSaleRequest(string Reason, StockLocationType Location);

    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        private readonly ISender _mediator;

        public SalesController(ISender mediator)
        {
            _mediator = mediator;
        }

        // GET: api/sales
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SaleDto>>> GetAllSales()
        {
            var result = await _mediator.Send(new GetAllSalesQuery());
            return Ok(result);
        }

        // GET: api/sales/customer/{customerId}
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<IEnumerable<SaleDto>>> GetSalesByCustomer(int customerId)
        {
            var result = await _mediator.Send(new GetSalesByCustomerQuery(customerId));
            return Ok(result);
        }

        // GET: api/sales/paged?page=1&pageSize=20&search=INV-2026&from=2026-10-01&to=2026-10-31&onlyDue=true
        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<SaleDto>>> GetSalesPaged(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null, [FromQuery] int? customerId = null,
            [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null,
            [FromQuery] bool onlyDue = false)
        {
            var result = await _mediator.Send(new GetSalesPagedQuery(page, pageSize, search, customerId, from, to, onlyDue));
            return Ok(result);
        }

        // GET: api/sales/5/invoice  (JSON, for your frontend to render)
        [HttpGet("{id:int}/invoice")]
        public async Task<ActionResult<InvoiceDto>> GetInvoice(int id)
        {
            return Ok(await _mediator.Send(new GetInvoiceQuery(id)));
        }

        // GET: api/sales/5/invoice/print  (printable HTML page)
        [HttpGet("{id:int}/invoice/print")]
        public async Task<ContentResult> PrintInvoice(int id, [FromServices] IConfiguration config)
        {
            var invoice = await _mediator.Send(new GetInvoiceQuery(id));
            var html = InvoiceHtmlBuilder.Build(
                invoice,
                config["Shop:Name"] ?? "JamunaERP",
                config["Shop:Address"],
                config["Shop:Phone"]);
            return Content(html, "text/html; charset=utf-8");
        }

        // POST: api/sales
        [HttpPost]
        public async Task<ActionResult<int>> CreateSale([FromBody] CreateSaleCommand command)
        {
            // Only managers/admins can give a discount
            if (command.DiscountAmount > 0 && !User.IsInRole(Roles.Admin) && !User.IsInRole(Roles.Manager))
                return StatusCode(403, new { message = "Only a manager or admin can give a discount." });

            var saleId = await _mediator.Send(command);
            return Ok(saleId);
        }

        // POST: api/sales/5/void
        [Authorize(Roles = Roles.AdminOrManager)]
        [HttpPost("{id:int}/void")]
        public async Task<ActionResult<SaleReturnResult>> VoidSale(int id, [FromBody] VoidSaleRequest request)
        {
            var result = await _mediator.Send(new VoidSaleCommand(id, request.Reason, request.Location));
            return Ok(result);
        }
    }
}