using System;
using System.Collections.Generic;
using System.Text;
using AccessiTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccessiTrack.Infrastructure.Persistence.Configurations;

public class AuditConfiguration : IEntityTypeConfiguration<Audit>
{
    public void Configure(EntityTypeBuilder<Audit> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(a => a.StartedAt)
            .IsRequired();

        builder.Property(a => a.Notes)
            .HasMaxLength(1000);

        builder.HasMany(a => a.Violations)
            .WithOne(v => v.Audit)
            .HasForeignKey(v => v.AuditId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
