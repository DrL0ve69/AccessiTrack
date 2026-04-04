using AccessiTrack.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace AccessiTrack.Application.Violations.Commands;

public record LogViolationCommand(
    Guid AuditId,
    string WcagCriterion,
    string WcagCriterionName,
    string HtmlElement,
    string Description,
    ViolationSeverity Severity
) : IRequest<Guid>;
