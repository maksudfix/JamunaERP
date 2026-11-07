using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Domain.Entities;
using MediatR;

namespace JamunaERP.Application.Customers.Commands.CreateCustomer
{
    public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, int>
    {
        private readonly IAppDbContext _context;

        public CreateCustomerCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = new Customer
            {
                Name = request.Name.Trim(),
                Phone = request.Phone.Trim(),
                Address = request.Address?.Trim() ?? string.Empty,
                NidNumber = string.IsNullOrWhiteSpace(request.NidNumber) ? null : request.NidNumber.Trim(),
                AlternatePhone = string.IsNullOrWhiteSpace(request.AlternatePhone) ? null : request.AlternatePhone.Trim()
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync(cancellationToken);

            return customer.Id;
        }
    }
}