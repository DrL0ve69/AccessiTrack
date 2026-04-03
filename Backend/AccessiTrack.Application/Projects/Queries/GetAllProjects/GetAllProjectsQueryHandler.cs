using System;
using System.Collections.Generic;
using System.Text;
using AccessiTrack.Domain.Interfaces;
using MediatR;

namespace AccessiTrack.Application.Projects.Queries.GetAllProjects;

public class GetAllProjectsQueryHandler
    : IRequestHandler<GetAllProjectsQuery, IEnumerable<ProjectDto>>
{
    private readonly IProjectRepository _repository;

    public GetAllProjectsQueryHandler(IProjectRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ProjectDto>> Handle(
        GetAllProjectsQuery request,
        CancellationToken cancellationToken)
    {
        var projects = await _repository.GetAllAsync(cancellationToken);

        return projects.Select(p => new ProjectDto(
            p.Id,
            p.Name,
            p.TargetUrl,
            p.ClientName,
            p.CreatedAt,
            p.Audits.Count,
            p.IsArchived
        ));
    }
}
