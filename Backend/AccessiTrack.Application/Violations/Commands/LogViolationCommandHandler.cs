using AccessiTrack.Domain.Entities;
using AccessiTrack.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace AccessiTrack.Application.Violations.Commands;

public class LogViolationCommandHandler
    : IRequestHandler<LogViolationCommand, Guid>
{
    private readonly IViolationRepository _violationRepository;
    private readonly IAuditRepository _auditRepository;

    public LogViolationCommandHandler(
        IViolationRepository violationRepository,
        IAuditRepository auditRepository)
    {
        _violationRepository = violationRepository;
        _auditRepository = auditRepository;
    }

    public async Task<Guid> Handle(
        LogViolationCommand request,
        CancellationToken cancellationToken)
    {
        var audit = await _auditRepository.GetByIdAsync(
            request.AuditId, cancellationToken);

        if (audit is null)
            throw new Exception($"Audit {request.AuditId} introuvable.");

        var violation = Violation.Report(
            request.AuditId,
            request.WcagCriterion,
            request.WcagCriterionName,
            request.HtmlElement,
            request.Description,
            request.Severity);

        await _violationRepository.AddAsync(violation, cancellationToken);
        await _violationRepository.SaveChangesAsync(cancellationToken);

        return violation.Id;
    }
}
