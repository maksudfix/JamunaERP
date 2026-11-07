using JamunaERP.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JamunaERP.Application.WarrantyServices.Commands.UpdateWarrantyService
{
    public class UpdateWarrantyServiceCommandHandler : IRequestHandler<UpdateWarrantyServiceCommand, bool>
    {
        private readonly IAppDbContext _context;

        public UpdateWarrantyServiceCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(UpdateWarrantyServiceCommand request, CancellationToken cancellationToken)
        {
            var service = await _context.WarrantyServices
                .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

            if (service == null)
                throw new KeyNotFoundException($"Warranty service record with ID {request.Id} was not found.");

            service.Status = request.Status;
            service.SentBackToDealerForRepair = request.SentBackToDealerForRepair;
            service.RequiredParts = request.RequiredParts;

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}