using AccessiTrack.Application.Audits.DTOs;
using AccessiTrack.Application.Common.Exceptions;
using AccessiTrack.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace AccessiTrack.Application.Audits.Queries.GetAuditResult;

public class GetAuditResultHandler(IAuditRepository auditRepository)
    : IRequestHandler<GetAuditResultQuery, AuditResultDto>
{
    public async Task<AuditResultDto> Handle(
        GetAuditResultQuery request, CancellationToken ct)
    {
        var audit = await auditRepository.GetByIdWithDetailsAsync(request.AuditId, ct)
            ?? throw new NotFoundException("Audit", request.AuditId);

        return new AuditResultDto(
            audit.Id,
            audit.ProjectId,
            audit.Project.Name,
            audit.Project.TargetUrl,
            audit.Status,
            audit.Score,
            audit.ViolationCount,
            audit.PassCount,
            audit.CreatedAt,
            audit.CompletedAt,
            audit.ErrorMessage,
            audit.Violations
                .Select(v => new ViolationDto(
                    v.Id, v.WcagCriterionName, v.Description, v.Severity,
                    v.WcagCriterion, v.Audit.Project.TargetUrl, v.HtmlElement,
                    v.ResolutionNote, v.ResolutionNote))
                .ToList()
                .AsReadOnly());
    }
}
