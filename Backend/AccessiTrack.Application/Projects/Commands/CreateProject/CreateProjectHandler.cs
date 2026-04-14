using AccessiTrack.Domain.Entities;
using AccessiTrack.Domain.Interfaces;
using MediatR;
using AccessiTrack.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using AccessiTrack.Application.Projects.Commands.CreateProject;

namespace AccessiTrack.Application.Features.Projects.Commands.CreateProject;

public class CreateProjectHandler(
    IProjectRepository projectRepository,
    ICurrentUserService currentUser,
    IAuditQueue auditQueue)
    : IRequestHandler<CreateProjectCommand, CreateProjectResponse>
{
    public async Task<CreateProjectResponse> Handle(
        CreateProjectCommand request, CancellationToken ct)
    {
        var project = Project.Create(request.Name, request.TargetUrl, request.ClientName);

        var audit = Audit.Create(project.Id);
        project.Audits.Add(audit);

        await projectRepository.AddAsync(project, ct);

        // Fire-and-forget via channel — returns 201 immediately
        auditQueue.Enqueue(new AuditJob(audit.Id, project.TargetUrl));

        return new CreateProjectResponse(project.Id, audit.Id);
    }
}
