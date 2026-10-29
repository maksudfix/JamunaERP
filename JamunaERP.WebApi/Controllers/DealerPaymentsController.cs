using JamunaERP.Application.Common.Security;
using JamunaERP.Application.DealerPayments.Commands.RecordDealerPayment;
using JamunaERP.Application.DealerPayments.DTOs;
using JamunaERP.Application.DealerPayments.Queries.GetDealerBalance;
using JamunaERP.Application.DealerPayments.Queries.GetPaymentsByDealer;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JamunaERP.WebApi.Controllers
{
    [Authorize(Roles = Roles.AdminOrManager)]
    [Route("api/[controller]")]
    [ApiController]
    public class DealerPaymentsController : ControllerBase
    {
        private readonly ISender _mediator;

        public DealerPaymentsController(ISender mediator)
        {
            _mediator = mediator;
        }

        // GET: api/dealerpayments/dealer/{dealerId}
        [HttpGet("dealer/{dealerId:int}")]
        public async Task<ActionResult<IEnumerable<DealerPaymentDto>>> GetPaymentsByDealer(int dealerId)
        {
            var payments = await _mediator.Send(new GetPaymentsByDealerQuery(dealerId));
            return Ok(payments);
        }

        // GET: api/dealerpayments/dealer/{dealerId}/balance
        [HttpGet("dealer/{dealerId:int}/balance")]
        public async Task<ActionResult<DealerBalanceDto>> GetDealerBalance(int dealerId)
        {
            var balance = await _mediator.Send(new GetDealerBalanceQuery(dealerId));
            return Ok(balance);
        }

        // POST: api/dealerpayments
        [HttpPost]
        public async Task<ActionResult<int>> RecordDealerPayment([FromBody] RecordDealerPaymentCommand command)
        {
            var paymentId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetPaymentsByDealer), new { dealerId = command.DealerId }, paymentId);
        }
    }
}