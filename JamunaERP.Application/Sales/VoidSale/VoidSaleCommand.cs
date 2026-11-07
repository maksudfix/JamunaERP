using FluentValidation;
using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.SaleReturns.Commands.CreateSaleReturn;
using JamunaERP.Domain.Entities;
using JamunaERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.Sales.Commands.VoidSale;

public record VoidSaleCommand(int SaleId, string Reason, StockLocationType Location)
    : IRequest<SaleReturnResult>;

public class VoidSaleCommandValidator : AbstractValidator<VoidSaleCommand>
{
    public VoidSaleCommandValidator()
    {
        RuleFor(x => x.SaleId).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(290);
        RuleFor(x => x.Location).IsInEnum();
    }
}

public class VoidSaleCommandHandler : IRequestHandler<VoidSaleCommand, SaleReturnResult>
{
    private readonly IAppDbContext _context;
    private readonly ISender _sender;

    public VoidSaleCommandHandler(IAppDbContext context, ISender sender)
    {
        _context = context;
        _sender = sender;
    }

    public async Task<SaleReturnResult> Handle(VoidSaleCommand request, CancellationToken ct)
    {
        var sale = await _context.Sales
            .AsNoTracking()
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == request.SaleId, ct)
            ?? throw new NotFoundException(nameof(Sale), request.SaleId);

        var itemIds = sale.Items.Select(i => i.Id).ToList();

        var returned = await _context.SaleReturnItems
            .Where(r => itemIds.Contains(r.SaleItemId))
            .GroupBy(r => r.SaleItemId)
            .Select(g => new { ItemId = g.Key, Qty = g.Sum(x => x.Quantity) })
            .ToDictionaryAsync(x => x.ItemId, x => x.Qty, ct);

        // Everything not yet returned goes back
        var lines = sale.Items
            .Select(i => new SaleReturnLineInput(i.Id, i.Quantity - returned.GetValueOrDefault(i.Id)))
            .Where(l => l.Quantity > 0)
            .ToList();

        if (lines.Count == 0)
            throw new BusinessRuleException("This sale has already been fully returned.");

        // A void is a full return. It goes through the normal pipeline (validation included).
        return await _sender.Send(
            new CreateSaleReturnCommand(request.SaleId, "VOID: " + request.Reason.Trim(), request.Location, lines),
            ct);
    }
}