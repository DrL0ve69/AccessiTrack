using System;
using System.Collections.Generic;
using System.Text;
using AccessiTrack.Domain.Entities;
using AccessiTrack.Domain.Interfaces;
using MediatR;

namespace AccessiTrack.Application.Audits.Commands.StartAudit;

public class StartAuditCommandHandler(
    IAuditRepository auditRepository,
    IProjectRepository projectRepository)
    : IRequestHandler<StartAuditCommand, Guid>
{
    public async Task<Guid> Handle(
        StartAuditCommand request,
        CancellationToken cancellationToken)
    {
        // Vérifie que le projet existe
        var project = await projectRepository.GetByIdAsync(
            request.ProjectId, cancellationToken);

        if (project is null)
            throw new Exception($"Projet {request.ProjectId} introuvable.");

        // Crée l'audit via la Factory Method du Domain
        var audit = Audit.Create(request.ProjectId);

        await auditRepository.AddAsync(audit, cancellationToken);
        await auditRepository.SaveChangesAsync(cancellationToken);

        return audit.Id;
    }
}
