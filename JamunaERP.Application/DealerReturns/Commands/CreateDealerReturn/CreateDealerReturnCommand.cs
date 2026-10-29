using FluentValidation;
using JamunaERP.Application.Common;
using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Domain.Entities;
using JamunaERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.DealerReturns.Commands.CreateDealerReturn;

public record CreateDealerReturnCommand(
    int DealerId,
    int ProductId,
    StockLocationType Location,   // where the faulty units are right now
    int Quantity,
    decimal? CreditAmount,        // null = Quantity x product BuyingPrice; 0 = replacement, no credit
    string Reason,
    DateTime? ReturnDate
) : IRequest<int>;

public class CreateDealerReturnCommandValidator : AbstractValidator<CreateDealerReturnCommand>
{
    public CreateDealerReturnCommandValidator()
    {
        RuleFor(x => x.DealerId).GreaterThan(0);
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Location).IsInEnum();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.CreditAmount).GreaterThanOrEqualTo(0).When(x => x.CreditAmount.HasValue);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ReturnDate)
            .Must(d => d is null || d.Value <= DateTime.UtcNow.AddDays(1))
            .WithMessage("Return date cannot be in the future.");
    }
}

public class CreateDealerReturnCommandHandler : IRequestHandler<CreateDealerReturnCommand, int>
{
    private readonly IAppDbContext _context;

    public CreateDealerReturnCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateDealerReturnCommand request, CancellationToken ct)
    {
        var dealerExists = await _context.Dealers.AnyAsync(d => d.Id == request.DealerId, ct);
        if (!dealerExists)
            throw new NotFoundException(nameof(Dealer), request.DealerId);

        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, ct)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        // Faulty units only, from the serial-less pool at that location
        var faultyRows = await _context.StockItems
            .Where(s => s.ProductId == request.ProductId
                        && s.Location == request.Location
                        && s.IsFaulty
                        && s.SerialNumber == null)
            .ToListAsync(ct);

        var available = faultyRows.Sum(s => s.Quantity);
        if (available < request.Quantity)
            throw new InsufficientStockException(product.Id, product.Name + " (faulty)", request.Quantity, available);

        StockPool.Deduct(faultyRows, request.Quantity);

        var dealerReturn = new DealerReturn
        {
            DealerId = request.DealerId,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            CreditAmount = request.CreditAmount ?? product.BuyingPrice * request.Quantity,
            Reason = request.Reason.Trim(),
            ReturnDate = request.ReturnDate ?? DateTime.UtcNow
        };

        _context.DealerReturns.Add(dealerReturn);
        await _context.SaveChangesAsync(ct);
        return dealerReturn.Id;
    }
}