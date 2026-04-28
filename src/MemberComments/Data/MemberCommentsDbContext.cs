using MemberComments.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MemberComments.Data;

public sealed class MemberCommentsDbContext : DbContext
{
    public MemberCommentsDbContext(DbContextOptions<MemberCommentsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.ToTable("membercomments_Comment");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.AuthorName).HasMaxLength(256).IsRequired();
            entity.Property(e => e.Subject).HasMaxLength(256).IsRequired();
            entity.Property(e => e.Text).HasMaxLength(100_000).IsRequired();

            entity.HasIndex(e => e.ContentKey);
            entity.HasIndex(e => e.ParentId);

            entity
                .HasOne(e => e.Parent)
                .WithMany(e => e.Replies)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
