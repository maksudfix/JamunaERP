using JamunaERP.Domain.Enums;
using MediatR;

namespace JamunaERP.Application.Sales.Commands.CreateSale
{
    public record CreateSaleItemInput(int ProductId, int Quantity);

    public record CreateSaleCommand(
        int CustomerId,
        StockLocationType Location,      // which showroom/godown the stock is sold from
        decimal PaidAmount,              // down payment, or full amount for a cash sale
        bool IsInstallment,
        int InstallmentCount,            // 1..6, ignored for cash sales
        DateTime? FirstDueDate,
        List<CreateSaleItemInput> Items,
        string PaymentMethod = "Cash",
        decimal DiscountAmount = 0
    ) : IRequest<int>;
}