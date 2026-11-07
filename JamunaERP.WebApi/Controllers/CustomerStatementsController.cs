using JamunaERP.Application.Customers.DTOs;
using JamunaERP.Application.Customers.Queries.GetCustomerStatement;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JamunaERP.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerStatementsController : ControllerBase
    {
        private readonly ISender _mediator;

        public CustomerStatementsController(ISender mediator)
        {
            _mediator = mediator;
        }

        // GET: api/customerstatements/5
        [HttpGet("{customerId:int}")]
        public async Task<ActionResult<CustomerStatementDto>> GetStatement(int customerId)
        {
            var statement = await _mediator.Send(new GetCustomerStatementQuery(customerId));
            return Ok(statement);
        }
    }
}