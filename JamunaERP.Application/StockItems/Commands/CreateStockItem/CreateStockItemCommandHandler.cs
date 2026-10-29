using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.StockItems.Commands.CreateStockItem
{
    public class CreateStockItemCommandHandler : IRequestHandler<CreateStockItemCommand, int>
    {
        private readonly IAppDbContext _context;

        public CreateStockItemCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreateStockItemCommand request, CancellationToken cancellationToken)
        {
            var productExists = await _context.Products
                .AnyAsync(p => p.Id == request.ProductId, cancellationToken);
            if (!productExists)
                throw new NotFoundException(nameof(Product), request.ProductId);

            var serial = string.IsNullOrWhiteSpace(request.SerialNumber)
                ? null
                : request.SerialNumber.Trim();

            var stockItem = new StockItem
            {
                ProductId = request.ProductId,
                Location = request.Location,
                Quantity = request.Quantity,
                IsFaulty = request.IsFaulty,
                SerialNumber = serial
            };
            _context.StockItems.Add(stockItem);

            // Audit trail: manual stock is never invisible
            _context.StockAdjustments.Add(new StockAdjustment
            {
                ProductId = request.ProductId,
                Location = request.Location,
                IsFaulty = request.IsFaulty,
                QuantityChange = request.Quantity,
                Reason = serial is null ? "Manual stock entry" : $"Manual stock entry (serial {serial})"
            });

            await _context.SaveChangesAsync(cancellationToken);
            return stockItem.Id;
        }
    }
}