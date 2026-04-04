using AccessiTrack.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace AccessiTrack.Application.Audits.Queries.GetAuditsByProject;

public class GetAuditsByProjectQueryHandler
    : IRequestHandler<GetAuditsByProjectQuery, IEnumerable<AuditDto>>
{
    private readonly IAuditRepository _repository;

    public GetAuditsByProjectQueryHandler(IAuditRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<AuditDto>> Handle(
        GetAuditsByProjectQuery request,
        CancellationToken cancellationToken)
    {
        var audits = await _repository.GetByProjectIdAsync(
            request.ProjectId, cancellationToken);

        return audits.Select(a => new AuditDto(
            a.Id,
            a.ProjectId,
            a.Status.ToString(),
            a.StartedAt,
            a.CompletedAt,
            a.Violations.Count,
            a.Violations.Count(v =>
                v.Severity == Domain.Enums.ViolationSeverity.Critical
                && !v.IsResolved),
            a.Violations.Count(v => v.IsResolved)
        ));
    }
}
