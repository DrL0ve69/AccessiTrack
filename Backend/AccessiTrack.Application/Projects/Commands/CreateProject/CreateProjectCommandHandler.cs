using System;
using System.Collections.Generic;
using System.Text;

using AccessiTrack.Domain.Entities;
using AccessiTrack.Domain.Interfaces;
using MediatR;

namespace AccessiTrack.Application.Projects.Commands.CreateProject;

public class CreateProjectCommandHandler
    : IRequestHandler<CreateProjectCommand, Guid>
{
    private readonly IProjectRepository _repository;

    public CreateProjectCommandHandler(IProjectRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(
        CreateProjectCommand request,
        CancellationToken cancellationToken)
    {
        var project = Project.Create(
            request.Name,
            request.TargetUrl,
            request.ClientName);

        await _repository.AddAsync(project, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return project.Id;
    }
}
