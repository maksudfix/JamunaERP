using JamunaERP.Domain.Enums;
using MediatR;

namespace JamunaERP.Application.Sales.Commands.CreateSale
{
    public record CreateSaleItemInput(
        int ProductId,
        int Quantity,
        int? StockItemId = null,
        string? SerialNumber = null);

    public record GuarantorInput(
        string Name,
        string Phone,
        string Address,
        string NidNumber);

    public record CreateSaleCommand(
        int CustomerId,
        StockLocationType Location,
        decimal PaidAmount,                      // Down payment on installment sales, or full amount on cash sales
        bool IsInstallment,
        int InstallmentCount,                    // Configurable: 3, 6, 9, or 12 months
        DateTime? FirstDueDate,
        List<CreateSaleItemInput> Items,
        string PaymentMethod = "Cash",
        decimal DiscountAmount = 0,
        decimal ServiceCharge = 0,               // Optional installment service charge
        GuarantorInput? Guarantor = null,        // Required on installment sales
        string? CustomerNid = null,              // Optional override/update if customer record lacks NID
        string? CustomerAlternatePhone = null,   // Optional override/update if customer record lacks AlternatePhone
        string? MobileBankingProvider = null,    // bKash, Nagad, Rocket
        string? TransactionId = null             // Mobile banking or card TrxID
    ) : IRequest<int>;
}