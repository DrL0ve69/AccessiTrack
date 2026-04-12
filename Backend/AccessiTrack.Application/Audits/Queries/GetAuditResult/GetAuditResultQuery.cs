using AccessiTrack.Application.Audits.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace AccessiTrack.Application.Audits.Queries.GetAuditResult;

public record GetAuditResultQuery(Guid AuditId) : IRequest<AuditResultDto>;
