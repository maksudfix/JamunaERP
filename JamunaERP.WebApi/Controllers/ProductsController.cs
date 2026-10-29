using JamunaERP.Application.Common.Models;
using JamunaERP.Application.Common.Security;
using JamunaERP.Application.Products.Commands.CreateProduct;
using JamunaERP.Application.Products.Commands.ManageProduct;
using JamunaERP.Application.Products.DTOs;
using JamunaERP.Application.Products.Queries.GetProducts;
using JamunaERP.Application.Products.Queries.GetProductsPaged;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JamunaERP.WebApi.Controllers
{
    // Everyone logged in can LIST products (employees need them for sales).
    // Only Admin / Manager can ADD / EDIT / DEACTIVATE products and see the buying price.
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ISender _mediator;

        public ProductsController(ISender mediator)
        {
            _mediator = mediator;
        }

        // GET: api/products?includeInactive=true
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts([FromQuery] bool includeInactive = false)
        {
            var canSeeCost = User.IsInRole(Roles.Admin) || User.IsInRole(Roles.Manager);

            // Employees never see deactivated products
            var products = await _mediator.Send(
                new GetProductsQuery(canSeeCost, includeInactive && canSeeCost));

            return Ok(products);
        }

        // GET: api/products/paged?page=1&pageSize=20&search=fan&category=Fan
        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<ProductDto>>> GetProductsPaged(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null, [FromQuery] string? category = null,
            [FromQuery] bool includeInactive = false)
        {
            var canSeeCost = User.IsInRole(Roles.Admin) || User.IsInRole(Roles.Manager);
            var result = await _mediator.Send(new GetProductsPagedQuery(
                page, pageSize, search, category, canSeeCost, includeInactive && canSeeCost));
            return Ok(result);
        }

        // POST: api/products
        [Authorize(Roles = Roles.AdminOrManager)]
        [HttpPost]
        public async Task<ActionResult<int>> CreateProduct([FromBody] CreateProductCommand command)
        {
            var productId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetAllProducts), new { id = productId }, productId);
        }

        // PUT: api/products/5
        [Authorize(Roles = Roles.AdminOrManager)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductCommand command)
        {
            await _mediator.Send(command with { Id = id });
            return NoContent();
        }

        // DELETE: api/products/5  (soft delete = deactivate)
        [Authorize(Roles = Roles.AdminOrManager)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeactivateProduct(int id)
        {
            await _mediator.Send(new SetProductActiveCommand(id, false));
            return NoContent();
        }

        // POST: api/products/5/reactivate
        [Authorize(Roles = Roles.AdminOrManager)]
        [HttpPost("{id:int}/reactivate")]
        public async Task<IActionResult> ReactivateProduct(int id)
        {
            await _mediator.Send(new SetProductActiveCommand(id, true));
            return NoContent();
        }
    }
}