using System;
using System.Collections.Generic;
using System.Text;

namespace AccessiTrack.Application.Projects.Queries.GetAllProjects;

public record ProjectDto(
    Guid Id,
    string Name,
    string TargetUrl,
    string ClientName,
    DateTime CreatedAt,
    int TotalAudits,
    bool IsArchived
);
