using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace JamunaERP.Application.Dealers.Commands.CreateDealer
{
    public class CreateDealerCommandHandler : IRequestHandler<CreateDealerCommand, int>
    {
        private readonly IAppDbContext _context;

        public CreateDealerCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreateDealerCommand request, CancellationToken cancellationToken)
        {
            var dealer = new Dealer
            {
                Name = request.Name,
                ContactPerson = request.ContactPerson,
                Phone = request.Phone,
                Address = request.Address
            };

            _context.Dealers.Add(dealer);
            await _context.SaveChangesAsync(cancellationToken);

            return dealer.Id;
        }
    }
}