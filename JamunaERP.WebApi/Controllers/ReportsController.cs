using JamunaERP.Application.Common;
using JamunaERP.Application.Common.Security;
using JamunaERP.Application.Reports.DTOs;
using JamunaERP.Application.Reports.Queries.GetCustomerDues;
using JamunaERP.Application.Reports.Queries.GetDailySalesReport;
using JamunaERP.Application.Reports.Queries.GetDealerPayables;
using JamunaERP.Application.Reports.Queries.GetOverdueInstallments;
using JamunaERP.Application.Reports.Queries.GetProfitReport;
using JamunaERP.Application.Reports.Queries.GetStockSummary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JamunaERP.WebApi.Controllers
{
    [Authorize(Roles = Roles.AdminOrManager)]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly ISender _mediator;

        public ReportsController(ISender mediator)
        {
            _mediator = mediator;
        }

        // Default range = first day of this month until today (shop time)
        private static (DateTime From, DateTime To) Range(DateTime? from, DateTime? to)
        {
            var today = ShopTime.Today;
            return (from ?? new DateTime(today.Year, today.Month, 1), to ?? today);
        }

        // GET: api/reports/daily-sales?from=2026-10-01&to=2026-10-31
        [HttpGet("daily-sales")]
        public async Task<ActionResult<DailySalesReportDto>> GetDailySales([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var (f, t) = Range(from, to);
            return Ok(await _mediator.Send(new GetDailySalesReportQuery(f, t)));
        }

        // GET: api/reports/profit?from=2026-10-01&to=2026-10-31
        [HttpGet("profit")]
        public async Task<ActionResult<ProfitReportDto>> GetProfit([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var (f, t) = Range(from, to);
            return Ok(await _mediator.Send(new GetProfitReportQuery(f, t)));
        }

        // GET: api/reports/customer-dues
        [HttpGet("customer-dues")]
        public async Task<ActionResult<IEnumerable<CustomerDueDto>>> GetCustomerDues()
            => Ok(await _mediator.Send(new GetCustomerDuesQuery()));

        // GET: api/reports/overdue-installments
        [HttpGet("overdue-installments")]
        public async Task<ActionResult<IEnumerable<OverdueInstallmentDto>>> GetOverdueInstallments()
            => Ok(await _mediator.Send(new GetOverdueInstallmentsQuery()));

        // GET: api/reports/dealer-payables?includeSettled=false
        [HttpGet("dealer-payables")]
        public async Task<ActionResult<IEnumerable<DealerPayableDto>>> GetDealerPayables([FromQuery] bool includeSettled = false)
            => Ok(await _mediator.Send(new GetDealerPayablesQuery(includeSettled)));

        // GET: api/reports/stock?lowStockThreshold=5
        [HttpGet("stock")]
        public async Task<ActionResult<StockSummaryDto>> GetStock([FromQuery] int? lowStockThreshold)
            => Ok(await _mediator.Send(new GetStockSummaryQuery(lowStockThreshold)));
    }
}