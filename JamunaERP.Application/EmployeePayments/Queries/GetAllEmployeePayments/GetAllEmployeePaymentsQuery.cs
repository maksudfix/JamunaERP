using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.EmployeePayments.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.EmployeePayments.Queries.GetAllEmployeePayments;

public record GetAllEmployeePaymentsQuery : IRequest<List<EmployeePaymentDto>>;

public class GetAllEmployeePaymentsQueryHandler : IRequestHandler<GetAllEmployeePaymentsQuery, List<EmployeePaymentDto>>
{
    private readonly IAppDbContext _context;

    public GetAllEmployeePaymentsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmployeePaymentDto>> Handle(GetAllEmployeePaymentsQuery request, CancellationToken cancellationToken)
    {
        return await _context.EmployeePayments
            .AsNoTracking()
            .OrderByDescending(p => p.PaymentYear)
            .ThenByDescending(p => p.PaymentMonth)
            .Select(p => new EmployeePaymentDto
            {
                Id = p.Id,
                EmployeeId = p.EmployeeId,
                ManagerId = p.ManagerId,
                BaseSalary = p.BaseSalary,
                Bonus = p.Bonus,
                Deductions = p.Deductions,
                NetPaidAmount = p.NetPaidAmount,
                PaymentMonth = p.PaymentMonth,
                PaymentYear = p.PaymentYear,
                PaymentDate = p.PaymentDate,
                Status = p.Status,
                Remarks = p.Remarks
            })
            .ToListAsync(cancellationToken);
    }
}