using FluentValidation;
using JamunaERP.Application.Common;
using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Domain.Entities;
using JamunaERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.StockAdjustments.Commands.AdjustStock;

public record AdjustStockCommand(
    int ProductId,
    StockLocationType Location,
    bool IsFaulty,
    int QuantityChange,      // + found, - lost / damaged
    string Reason
) : IRequest<int>;

public class AdjustStockCommandValidator : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockCommandValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Location).IsInEnum();
        RuleFor(x => x.QuantityChange).NotEqual(0).WithMessage("Quantity change cannot be zero.");
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(300);
    }
}

public class AdjustStockCommandHandler : IRequestHandler<AdjustStockCommand, int>
{
    private readonly IAppDbContext _context;

    public AdjustStockCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(AdjustStockCommand request, CancellationToken ct)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, ct)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        // Serial-less pooled rows only (rows with serial numbers are single units, handled elsewhere)
        var rows = await _context.StockItems
            .Where(s => s.ProductId == request.ProductId
                        && s.Location == request.Location
                        && s.IsFaulty == request.IsFaulty
                        && s.SerialNumber == null)
            .OrderBy(s => s.Id)
            .ToListAsync(ct);

        if (request.QuantityChange > 0)
        {
            var row = rows.FirstOrDefault();
            if (row is null)
            {
                row = new StockItem
                {
                    ProductId = request.ProductId,
                    Location = request.Location,
                    IsFaulty = request.IsFaulty,
                    Quantity = 0
                };
                _context.StockItems.Add(row);
            }
            row.Quantity += request.QuantityChange;
        }
        else
        {
            var needed = -request.QuantityChange;
            var available = rows.Sum(r => r.Quantity);

            if (available < needed)
                throw new InsufficientStockException(product.Id, product.Name, needed, available);

            StockPool.Deduct(rows, needed);
        }

        var adjustment = new StockAdjustment
        {
            ProductId = request.ProductId,
            Location = request.Location,
            IsFaulty = request.IsFaulty,
            QuantityChange = request.QuantityChange,
            Reason = request.Reason.Trim()
        };

        _context.StockAdjustments.Add(adjustment);
        await _context.SaveChangesAsync(ct);
        return adjustment.Id;
    }
}