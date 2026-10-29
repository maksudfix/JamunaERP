using JamunaERP.Application.WarrantyServices.Commands.CreateWarrantyService;
using JamunaERP.Application.WarrantyServices.Commands.UpdateWarrantyService;
using JamunaERP.Application.WarrantyServices.DTOs;
using JamunaERP.Application.WarrantyServices.Queries.GetAllWarrantyServices;
using JamunaERP.Application.WarrantyServices.Queries.GetWarrantyServicesByCustomer;
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