using MediatR;

namespace AccessiTrack.Application.Audits.Queries.GetAuditById;

public record GetAuditByIdQuery(Guid AuditId)
    : IRequest<AuditDetailsDto>;
