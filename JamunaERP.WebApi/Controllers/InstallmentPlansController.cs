using JamunaERP.Application.InstallmentPlans.Commands.PayInstallment;
using JamunaERP.Application.InstallmentPlans.DTOs;
using JamunaERP.Application.InstallmentPlans.Queries.GetInstallmentsBySale;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JamunaERP.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class InstallmentPlansController : ControllerBase
    {
        private readonly ISender _mediator;

        public InstallmentPlansController(ISender mediator)
        {
            _mediator = mediator;
        }

        // GET: api/installmentplans/sale/{saleId}
        [HttpGet("sale/{saleId}")]
        public async Task<ActionResult<IEnumerable<InstallmentPlanDto>>> GetPlansBySale(int saleId)
        {
            var plans = await _mediator.Send(new GetInstallmentsBySaleQuery(saleId));
            return Ok(plans);
        }

        // POST: api/installmentplans/pay
        [HttpPost("pay")]
        public async Task<IActionResult> PayInstallment([FromBody] PayInstallmentCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(new { success = result, message = "Installment payment recorded successfully." });
        }
    }
}