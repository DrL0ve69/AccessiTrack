using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace AccessiTrack.Application.Audits.Queries.GetAuditsByProject;

public record GetAuditsByProjectQuery(Guid ProjectId)
    : IRequest<IEnumerable<AuditDto>>;
