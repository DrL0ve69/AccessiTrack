using System;
using System.Collections.Generic;
using System.Text;

namespace AccessiTrack.Domain.Entities;

public class UserProfile
{
    public Guid Id { get; set; }
    // Lien vers l'ID de ApplicationUser dans Identity
    public string IdentityId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Exemple de données métiers spécifiques
    public string PreferredLanguage { get; set; } = "fr-CA";
    public bool HighContrastEnabled { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Extended profile fields
    public string? PhoneNumber { get; set; }
    public string? Avatar { get; set; }
    public string? Bio { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Privacy flags
    public bool EmailPrivate { get; set; } = false;
    public bool PhonePrivate { get; set; } = false;
    public bool BioPrivate { get; set; } = false;

    // Soft delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}

