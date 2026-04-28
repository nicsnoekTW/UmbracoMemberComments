using MemberComments.Data;
using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Infrastructure.Migrations;

namespace MemberComments.Migrations.Plans;

/// <summary>Unscoped so <see cref="DbContext.Database.MigrateAsync"/> is not wrapped in the NPoco migration transaction.</summary>
public sealed class ApplyMemberCommentsEfMigrations : UnscopedAsyncMigrationBase
{
    private readonly IDbContextFactory<MemberCommentsDbContext> _dbContextFactory;

    public ApplyMemberCommentsEfMigrations(
        IMigrationContext context,
        IDbContextFactory<MemberCommentsDbContext> dbContextFactory)
        : base(context)
    {
        _dbContextFactory = dbContextFactory;
    }

    protected override async Task MigrateAsync()
    {
        await using MemberCommentsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        await dbContext.Database.MigrateAsync();
        Context.Complete();
    }
}
