using AccessiTrack.Application.Common.Exceptions;
using AccessiTrack.Domain.Interfaces;
using MediatR;

namespace AccessiTrack.Application.Audits.Queries.GetAuditById;

public class GetAuditByIdQueryHandler(IAuditRepository repository)
    : IRequestHandler<GetAuditByIdQuery, AuditDetailsDto>
{
    public async Task<AuditDetailsDto> Handle(
        GetAuditByIdQuery request,
        CancellationToken cancellationToken)
    {
        var audit = await repository.GetByIdAsync(
            request.AuditId, cancellationToken);

        if (audit is null)
            throw new NotFoundException($"Audit with ID {request.AuditId} not found.", request.AuditId);

        return new AuditDetailsDto(
            audit.Id,
            audit.ProjectId,
            audit.Status.ToString(),
            DateTime.Now,
            audit.CompletedAt,
            audit.Score.ToString(),
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
