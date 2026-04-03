using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace AccessiTrack.Application.Projects.Queries.GetAllProjects;

public record GetAllProjectsQuery() : IRequest<IEnumerable<ProjectDto>>;
