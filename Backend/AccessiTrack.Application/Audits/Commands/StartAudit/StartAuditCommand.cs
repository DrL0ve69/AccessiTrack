using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace AccessiTrack.Application.Audits.Commands.StartAudit;

public record StartAuditCommand(Guid ProjectId) : IRequest<Guid>;
