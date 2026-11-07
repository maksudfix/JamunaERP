using FluentValidation;
using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.InstallmentPlans.Commands.UpdateFollowUp;

public record UpdateInstallmentFollowUpCommand(
    int InstallmentPlanId,
    string CollectionNote,
    DateTime? NextPromiseDate = null
) : IRequest<bool>;

public class UpdateInstallmentFollowUpCommandValidator : AbstractValidator<UpdateInstallmentFollowUpCommand>
{
    public UpdateInstallmentFollowUpCommandValidator()
    {
        RuleFor(x => x.InstallmentPlanId).GreaterThan(0);
        RuleFor(x => x.CollectionNote).NotEmpty().MaximumLength(500);
    }
}

public class UpdateInstallmentFollowUpCommandHandler : IRequestHandler<UpdateInstallmentFollowUpCommand, bool>
{
    private readonly IAppDbContext _context;

    public UpdateInstallmentFollowUpCommandHandler(IAppDbContext context) => _context = context;

    public async Task<bool> Handle(UpdateInstallmentFollowUpCommand request, CancellationToken ct)
    {
        var plan = await _context.InstallmentPlans
            .FirstOrDefaultAsync(p => p.Id == request.InstallmentPlanId, ct)
            ?? throw new NotFoundException(nameof(InstallmentPlan), request.InstallmentPlanId);

        plan.CollectionNote = request.CollectionNote.Trim();
        plan.LastFollowUpDate = DateTime.UtcNow;
        plan.NextPromiseDate = request.NextPromiseDate?.Date;

        await _context.SaveChangesAsync(ct);
        return true;
    }
}