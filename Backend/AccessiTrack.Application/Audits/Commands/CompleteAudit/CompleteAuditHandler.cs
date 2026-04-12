using AccessiTrack.Domain.Entities;
using AccessiTrack.Domain.Interfaces;
using MediatR;

namespace AccessiTrack.Application.Audits.Commands.CompleteAudit;

public class CompleteAuditCommandHandler : IRequestHandler<CompleteAuditCommand, Unit>
{
    private readonly IAuditRepository _auditRepository;

    public CompleteAuditCommandHandler(IAuditRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    public async Task<Unit> Handle(CompleteAuditCommand request, CancellationToken cancellationToken)
    {
        var audit = await _auditRepository.GetByIdAsync(request.AuditId, cancellationToken);

        if (audit is null)
            throw new Exception($"Audit {request.AuditId} introuvable.");

        audit.Complete(audit.Score ?? 0, audit.ViolationCount, audit.Violations.Count);

        await _auditRepository.UpdateAsync(audit, cancellationToken);
        await _auditRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
