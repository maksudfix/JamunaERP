using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.EmployeePayments.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.EmployeePayments.Queries.GetEmployeePaymentsByEmployee;

public record GetEmployeePaymentsByEmployeeQuery(string EmployeeId) : IRequest<List<EmployeePaymentDto>>;

public class GetEmployeePaymentsByEmployeeQueryHandler : IRequestHandler<GetEmployeePaymentsByEmployeeQuery, List<EmployeePaymentDto>>
{
    private readonly IAppDbContext _context;

    public GetEmployeePaymentsByEmployeeQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmployeePaymentDto>> Handle(GetEmployeePaymentsByEmployeeQuery request, CancellationToken cancellationToken)
    {
        return await _context.EmployeePayments
            .AsNoTracking()
            .Where(p => p.EmployeeId == request.EmployeeId)
            .OrderByDescending(p => p.PaymentDate)
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