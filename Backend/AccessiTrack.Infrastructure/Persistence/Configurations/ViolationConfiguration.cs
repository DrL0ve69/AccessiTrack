using System;
using System.Collections.Generic;
using System.Text;
using AccessiTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccessiTrack.Infrastructure.Persistence.Configurations;

public class ViolationConfiguration : IEntityTypeConfiguration<Violation>
{
    public void Configure(EntityTypeBuilder<Violation> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.WcagCriterion)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(v => v.WcagCriterionName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(v => v.HtmlElement)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(v => v.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(v => v.Severity)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(v => v.ResolutionNote)
            .HasMaxLength(2000);
    }
}
