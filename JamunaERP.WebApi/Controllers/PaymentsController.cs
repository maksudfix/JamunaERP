using JamunaERP.Application.Payments.DTOs;
using JamunaERP.Application.Payments.Queries.GetPaymentsBySale;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JamunaERP.WebApi.Controllers
{
    // Read-only: receipts are created by CreateSale and PayInstallment.
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly ISender _mediator;

        public PaymentsController(ISender mediator)
        {
            _mediator = mediator;
        }

        // GET: api/payments/sale/{saleId}
        [HttpGet("sale/{saleId:int}")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPaymentsBySale(int saleId)
        {
            var payments = await _mediator.Send(new GetPaymentsBySaleQuery(saleId));
            return Ok(payments);
        }
    }
}