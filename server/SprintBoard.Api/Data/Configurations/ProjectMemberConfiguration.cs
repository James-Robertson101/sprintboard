using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SprintBoard.Api.Models;

namespace SprintBoard.Api.Data.Configurations;

public class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
{
    public void Configure(EntityTypeBuilder<ProjectMember> builder)
    {
        builder.HasIndex(pm => new { pm.ProjectId, pm.UserId })
            .IsUnique()
            .HasFilter("\"RemovedTime\" IS NULL"); // Postgres uses double quotes, not brackets

        builder.HasIndex(pm => pm.ProjectId);

        builder.HasOne(pm => pm.InvitedByUser)
            .WithMany()
            .HasForeignKey(pm => pm.InvitedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pm => pm.User)
            .WithMany(u => u.ProjectMembers)
            .HasForeignKey(pm => pm.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pm => pm.Project)
            .WithMany(p => p.ProjectMembers)
            .HasForeignKey(pm => pm.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Drop this for now — see note below
        // builder.Property(pm => pm.RowVersion).IsRowVersion();
    }
}