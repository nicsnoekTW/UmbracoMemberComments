using Umbraco.Cms.Core.Packaging;

namespace MemberComments.Migrations.Plans;

/// <summary>Runs EF Core migrations for <see cref="MemberComments.Data.MemberCommentsDbContext"/> during Umbraco package migration.</summary>
public sealed class MemberCommentsPackageMigrationPlan : PackageMigrationPlan
{
    public MemberCommentsPackageMigrationPlan()
        : base("MemberComments")
    {
    }

    protected override void DefinePlan()
    {
        To<ApplyMemberCommentsEfMigrations>("membercomments-ef-initial");
        To<ApplyMemberCommentsEfMigrations>("membercomments-ef-softdelete-moderator");
        To<ApplyMemberCommentsEfMigrations>("membercomments-ef-subject-richtext");
        To<ApplyMemberCommentsEfMigrations>("membercomments-ef-content-id");
    }
}
