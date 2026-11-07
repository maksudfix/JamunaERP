using FluentValidation;
using JamunaERP.Application.Common;
using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Domain.Entities;
using JamunaERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.StockItems.Commands.MoveStock;

public record MoveStockCommand(
    int ProductId,
    StockLocationType From,
    StockLocationType To,
    int Quantity,
    bool IsFaulty = false,
    string? SerialNumber = null,
    string? Notes = null
) : IRequest<bool>;

public class MoveStockCommandValidator : AbstractValidator<MoveStockCommand>
{
    public MoveStockCommandValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.From).IsInEnum();
        RuleFor(x => x.To).IsInEnum();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.To).NotEqual(x => x.From)
            .WithMessage("Source and destination locations must be different.");
        RuleFor(x => x.SerialNumber).MaximumLength(100);
        RuleFor(x => x.Notes).MaximumLength(200);

        When(x => !string.IsNullOrWhiteSpace(x.SerialNumber), () =>
        {
            RuleFor(x => x.Quantity).Equal(1)
                .WithMessage("A serialized stock transfer must have a quantity of 1.");
        });
    }
}

public class MoveStockCommandHandler : IRequestHandler<MoveStockCommand, bool>
{
    private readonly IAppDbContext _context;

    public MoveStockCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(MoveStockCommand request, CancellationToken ct)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, ct)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        var serial = string.IsNullOrWhiteSpace(request.SerialNumber)
            ? null
            : request.SerialNumber.Trim();

        if (serial is not null)
        {
            var serialRow = await _context.StockItems
                .FirstOrDefaultAsync(s => s.ProductId == request.ProductId
                                          && s.Location == request.From
                                          && s.IsFaulty == request.IsFaulty
                                          && s.SerialNumber == serial
                                          && s.Quantity > 0, ct);

            if (serialRow is null)
                throw new InsufficientStockException(product.Id, $"{product.Name} (Serial {serial})", 1, 0);

            serialRow.Location = request.To;
        }
        else
        {
            var source = await _context.StockItems
                .Where(s => s.ProductId == request.ProductId
                            && s.Location == request.From
                            && s.IsFaulty == request.IsFaulty
                            && s.SerialNumber == null)
                .ToListAsync(ct);

            var available = source.Sum(s => s.Quantity);
            if (available < request.Quantity)
                throw new InsufficientStockException(product.Id, product.Name, request.Quantity, available);

            var destination = await _context.StockItems
                .FirstOrDefaultAsync(s => s.ProductId == request.ProductId
                                          && s.Location == request.To
                                          && s.IsFaulty == request.IsFaulty
                                          && s.SerialNumber == null, ct);

            StockPool.Deduct(source, request.Quantity);

            if (destination is null)
            {
                _context.StockItems.Add(new StockItem
                {
                    ProductId = request.ProductId,
                    Location = request.To,
                    IsFaulty = request.IsFaulty,
                    Quantity = request.Quantity
                });
            }
            else
            {
                destination.Quantity += request.Quantity;
            }
        }

        await _context.SaveChangesAsync(ct);
        return true;
    }
}