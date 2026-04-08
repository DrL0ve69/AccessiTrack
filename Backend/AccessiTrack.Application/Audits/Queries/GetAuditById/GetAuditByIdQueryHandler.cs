using AccessiTrack.Application.Common.Exceptions;
using AccessiTrack.Domain.Interfaces;
using MediatR;

namespace AccessiTrack.Application.Audits.Queries.GetAuditById;

public class GetAuditByIdQueryHandler
    : IRequestHandler<GetAuditByIdQuery, AuditDetailsDto>
{
    private readonly IAuditRepository _repository;

    public GetAuditByIdQueryHandler(IAuditRepository repository)
    {
        _repository = repository;
    }

    public async Task<AuditDetailsDto> Handle(
        GetAuditByIdQuery request,
        CancellationToken cancellationToken)
    {
        var audit = await _repository.GetByIdAsync(
            request.AuditId, cancellationToken);

        if (audit == null)
            throw new NotFoundException($"Audit with ID {request.AuditId} not found.", request.AuditId);

        return new AuditDetailsDto(
            audit.Id,
            audit.ProjectId,
            audit.Status.ToString(),
            audit.StartedAt,
            audit.CompletedAt,
            audit.Notes,
            audit.Violations.Count,
            audit.Violations.Count(v =>
                v.Severity == Domain.Enums.ViolationSeverity.Critical
                && !v.IsResolved),
            audit.Violations.Count(v => v.IsResolved),
            audit.Violations.Select(v => new ViolationDto(
                v.Id,
                v.AuditId,
                v.WcagCriterion,
                v.WcagCriterionName,
                v.HtmlElement,
                v.Description,
                v.Severity.ToString(),
                v.IsResolved,
                v.ResolutionNote,
                v.ReportedAt
            ))
        );
    }
}
