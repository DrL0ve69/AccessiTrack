using System;
using System.Collections.Generic;
using System.Text;
using AccessiTrack.Domain.Exceptions;

namespace AccessiTrack.Domain.Entities;

public class Project
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string TargetUrl { get; private set; } = string.Empty;
    public string ClientName { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public bool IsArchived { get; private set; }

    // Navigation EF Core
    public ICollection<Audit> Audits { get; private set; } = new List<Audit>();

    // Constructeur privé pour EF Core
    private Project() { }

    // Factory method (Clean Architecture : pas de setters publics)
    public static Project Create(string name, string targetUrl, string clientName)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Le nom du projet est obligatoire.");

        return new Project
        {
            Id = Guid.NewGuid(),
            Name = name,
            TargetUrl = targetUrl,
            ClientName = clientName,
            CreatedAt = DateTime.UtcNow,
            IsArchived = false
        };
    }

    public void Archive() => IsArchived = true;
}
