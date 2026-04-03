using System;
using System.Collections.Generic;
using System.Text;
using AccessiTrack.Domain.Entities;
using AccessiTrack.Domain.Interfaces;
using MediatR;

namespace AccessiTrack.Application.Audits.Commands.StartAudit;

public class StartAuditCommandHandler
    : IRequestHandler<StartAuditCommand, Guid>
{
    private readonly IAuditRepository _auditRepository;
    private readonly IProjectRepository _projectRepository;

    public StartAuditCommandHandler(
        IAuditRepository auditRepository,
        IProjectRepository projectRepository)
    {
        _auditRepository = auditRepository;
        _projectRepository = projectRepository;
    }

    public async Task<Guid> Handle(
        StartAuditCommand request,
        CancellationToken cancellationToken)
    {
        // Vérifie que le projet existe
        var project = await _projectRepository.GetByIdAsync(
            request.ProjectId, cancellationToken);

        if (project is null)
            throw new Exception($"Projet {request.ProjectId} introuvable.");

        // Crée l'audit via la Factory Method du Domain
        var audit = Audit.Start(request.ProjectId);

        await _auditRepository.AddAsync(audit, cancellationToken);
        await _auditRepository.SaveChangesAsync(cancellationToken);

        return audit.Id;
    }
}
