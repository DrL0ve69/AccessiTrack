using AccessiTrack.Domain.Entities;
using AccessiTrack.Domain.Enums;
using AccessiTrack.Domain.Interfaces;
using AccessiTrack.Domain.Exceptions;
using MediatR;
using AccessiTrack.Application.Common.Interfaces;
using AccessiTrack.Application.Common.Exceptions;

namespace AccessiTrack.Application.Audits.Commands.StartAutomaticAudit;

/// <summary>
/// Handler for StartAutomaticAuditCommand.
/// Creates an audit, scans the project's URL for accessibility violations,
/// and logs all detected violations automatically.
/// </summary>
public class StartAutomaticAuditCommandHandler
    : IRequestHandler<StartAutomaticAuditCommand, StartAutomaticAuditResult>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly IViolationRepository _violationRepository;
    private readonly IAccessibilityScanner _scanner;

    public StartAutomaticAuditCommandHandler(
        IProjectRepository projectRepository,
        IAuditRepository auditRepository,
        IViolationRepository violationRepository,
        IAccessibilityScanner scanner)
    {
        _projectRepository = projectRepository;
        _auditRepository = auditRepository;
        _violationRepository = violationRepository;
        _scanner = scanner;
    }

    public async Task<StartAutomaticAuditResult> Handle(
        StartAutomaticAuditCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Verify project exists and get its URL
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);

        if (project is null)
            throw new NotFoundException($"Project with ID {request.ProjectId} not found.", request.ProjectId);

        if (string.IsNullOrWhiteSpace(project.TargetUrl))
            throw new DomainException("Project has no TargetUrl configured for automatic scanning.");

        // 2. Create and save the audit
        var audit = Audit.Start(request.ProjectId);
        await _auditRepository.AddAsync(audit, cancellationToken);
        await _auditRepository.SaveChangesAsync(cancellationToken);

        // 3. Scan the URL for violations
        var violationCommands = await _scanner.ScanUrlAsync(project.TargetUrl, cancellationToken);

        // 4. Log all violations
        var violationsFound = 0;
        var criticalCount = 0;
        var majorCount = 0;
        var minorCount = 0;

        foreach (var violationCmd in violationCommands)
        {
            // Create violation entity directly
            var violation = Violation.Report(
                audit.Id,
                violationCmd.WcagCriterion,
                violationCmd.WcagCriterionName,
                violationCmd.HtmlElement,
                violationCmd.Description,
                violationCmd.Severity
            );

            await _violationRepository.AddAsync(violation, cancellationToken);

            violationsFound++;

            switch (violation.Severity)
            {
                case ViolationSeverity.Critical:
                    criticalCount++;
                    break;
                case ViolationSeverity.Major:
                    majorCount++;
                    break;
                case ViolationSeverity.Minor:
                    minorCount++;
                    break;
            }
        }

        await _violationRepository.SaveChangesAsync(cancellationToken);

        // 5. Update audit status if no violations found
        if (violationsFound == 0)
        {
            // Mark as completed if no violations
            audit.Complete();
            await _auditRepository.UpdateAsync(audit, cancellationToken);
            await _auditRepository.SaveChangesAsync(cancellationToken);
        }

        return new StartAutomaticAuditResult(
            audit.Id,
            violationsFound,
            criticalCount,
            majorCount,
            minorCount
        );
    }
}
