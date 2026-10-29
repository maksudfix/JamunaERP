using JamunaERP.Application.Common.Security;
using JamunaERP.Application.EmployeePayments.Commands.CreateEmployeePayment;
using JamunaERP.Application.EmployeePayments.DTOs;
using JamunaERP.Application.EmployeePayments.Queries.GetAllEmployeePayments;
using JamunaERP.Application.EmployeePayments.Queries.GetEmployeePaymentsByEmployee;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JamunaERP.WebApi.Controllers
{
    [Authorize(Roles = Roles.AdminOrManager)]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeePaymentsController : ControllerBase
    {
        private readonly ISender _mediator;

        public EmployeePaymentsController(ISender mediator)
        {
            _mediator = mediator;
        }

        // GET: api/employeepayments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeePaymentDto>>> GetAllEmployeePayments()
        {
            var payments = await _mediator.Send(new GetAllEmployeePaymentsQuery());
            return Ok(payments);
        }

        // GET: api/employeepayments/employee/{employeeId}
        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<IEnumerable<EmployeePaymentDto>>> GetPaymentsByEmployee(string employeeId)
        {
            var payments = await _mediator.Send(new GetEmployeePaymentsByEmployeeQuery(employeeId));
            return Ok(payments);
        }

        // POST: api/employeepayments
        [HttpPost]
        public async Task<ActionResult<int>> CreateEmployeePayment([FromBody] CreateEmployeePaymentCommand command)
        {
            var paymentId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetAllEmployeePayments), new { id = paymentId }, paymentId);
        }
    }
}