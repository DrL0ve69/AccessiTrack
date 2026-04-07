using MediatR;

namespace AccessiTrack.Application.Audits.Commands.CompleteAudit;

public record CompleteAuditCommand(Guid AuditId) : IRequest<Unit>;
