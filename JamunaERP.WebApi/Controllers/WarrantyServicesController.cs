using JamunaERP.Application.Common.Models;
using JamunaERP.Application.WarrantyServices.Commands.CreateWarrantyService;
using JamunaERP.Application.WarrantyServices.Commands.UpdateWarrantyService;
using JamunaERP.Application.WarrantyServices.DTOs;
using JamunaERP.Application.WarrantyServices.Queries.GetAllWarrantyServices;
using JamunaERP.Application.WarrantyServices.Queries.GetWarrantyServicesByCustomer;
using JamunaERP.Application.WarrantyServices.Queries.GetWarrantyServicesPaged;
using JamunaERP.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JamunaERP.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class WarrantyServicesController : ControllerBase
    {
        private readonly ISender _mediator;

        public WarrantyServicesController(ISender mediator)
        {
            _mediator = mediator;
        }

        // GET: api/warrantyservices
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WarrantyServiceDto>>> GetAllWarrantyServices()
        {
            var services = await _mediator.Send(new GetAllWarrantyServicesQuery());
            return Ok(services);
        }

        // GET: api/warrantyservices/paged?page=1&pageSize=20&search=SN-001&status=Claimed
        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<WarrantyServiceDto>>> GetWarrantyServicesPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] WarrantyStatus? status = null)
        {
            var result = await _mediator.Send(new GetWarrantyServicesPagedQuery(page, pageSize, search, status));
            return Ok(result);
        }

        // GET: api/warrantyservices/customer/{customerId}
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<IEnumerable<WarrantyServiceDto>>> GetWarrantyServicesByCustomer(int customerId)
        {
            var services = await _mediator.Send(new GetWarrantyServicesByCustomerQuery(customerId));
            return Ok(services);
        }

        // POST: api/warrantyservices
        [HttpPost]
        public async Task<ActionResult<int>> CreateWarrantyService([FromBody] CreateWarrantyServiceCommand command)
        {
            var serviceId = await _mediator.Send(command);
            return Ok(serviceId);
        }

        // PUT: api/warrantyservices
        [HttpPut]
        public async Task<IActionResult> UpdateWarrantyService([FromBody] UpdateWarrantyServiceCommand command)
        {
            await _mediator.Send(command);
            return Ok(new { success = true, message = "Warranty service record updated successfully." });
        }
    }
}