using System;
using System.Collections.Generic;
using System.Text;

using MediatR;

namespace AccessiTrack.Application.Projects.Commands.CreateProject;

public record CreateProjectCommand(
    string Name,
    string TargetUrl,
    string ClientName
) : IRequest<CreateProjectResponse>;

public record CreateProjectResponse(Guid ProjectId, Guid AuditId);
