using FluentValidation;
using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.Products.Commands.ManageProduct;

// ---------- UPDATE ----------
public record UpdateProductCommand(
    int Id,
    string Name,
    string ModelNumber,
    string Category,
    decimal BuyingPrice,
    decimal SellingPrice,
    int WarrantyMonths,
    string Brand = "Jamuna",
    string? Barcode = null,
    int ReorderLevel = 5,
    int? DefaultDealerId = null,
    bool AllowInstallment = true) : IRequest<bool>;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Brand).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ModelNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Barcode).MaximumLength(100);
        RuleFor(x => x.BuyingPrice).GreaterThanOrEqualTo(0).PrecisionScale(18, 2, ignoreTrailingZeros: true);
        RuleFor(x => x.SellingPrice).GreaterThan(0).PrecisionScale(18, 2, ignoreTrailingZeros: true);
        RuleFor(x => x.WarrantyMonths).InclusiveBetween(0, 120);
        RuleFor(x => x.ReorderLevel).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DefaultDealerId).GreaterThan(0).When(x => x.DefaultDealerId.HasValue);
    }
}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, bool>
{
    private readonly IAppDbContext _context;
    public UpdateProductCommandHandler(IAppDbContext context) => _context = context;

    public async Task<bool> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        if (request.DefaultDealerId.HasValue)
        {
            var dealerExists = await _context.Dealers
                .AnyAsync(d => d.Id == request.DefaultDealerId.Value, ct);
            if (!dealerExists)
                throw new NotFoundException(nameof(Dealer), request.DefaultDealerId.Value);
        }

        product.Name = request.Name.Trim();
        product.Brand = string.IsNullOrWhiteSpace(request.Brand) ? "Jamuna" : request.Brand.Trim();
        product.ModelNo = request.ModelNumber.Trim();
        product.Category = request.Category.Trim();
        product.Barcode = string.IsNullOrWhiteSpace(request.Barcode) ? null : request.Barcode.Trim();
        product.BuyingPrice = request.BuyingPrice;
        product.SellingPrice = request.SellingPrice;
        product.WarrantyMonths = request.WarrantyMonths;
        product.ReorderLevel = request.ReorderLevel;
        product.DefaultDealerId = request.DefaultDealerId;
        product.AllowInstallment = request.AllowInstallment;

        await _context.SaveChangesAsync(ct);
        return true;
    }
}

// ---------- DEACTIVATE / REACTIVATE ----------
public record SetProductActiveCommand(int Id, bool IsActive) : IRequest<bool>;

public class SetProductActiveCommandHandler : IRequestHandler<SetProductActiveCommand, bool>
{
    private readonly IAppDbContext _context;
    public SetProductActiveCommandHandler(IAppDbContext context) => _context = context;

    public async Task<bool> Handle(SetProductActiveCommand request, CancellationToken ct)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        if (!request.IsActive)
        {
            var goodStock = await _context.StockItems
                .Where(s => s.ProductId == request.Id && !s.IsFaulty)
                .SumAsync(s => (int?)s.Quantity, ct) ?? 0;

            if (goodStock > 0)
                throw new BusinessRuleException(
                    $"Cannot deactivate this product: {goodStock} unit(s) are still in stock. Sell or transfer them first.");
        }

        product.IsActive = request.IsActive;
        await _context.SaveChangesAsync(ct);
        return true;
    }
}