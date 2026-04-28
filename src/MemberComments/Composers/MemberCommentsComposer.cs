using MemberComments.Data;
using MemberComments.Migrations.Plans;
using MemberComments.Services;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace MemberComments.Composers;

public sealed class MemberCommentsComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddUmbracoDbContext<MemberCommentsDbContext>(
            (serviceProvider, optionsBuilder, _, _) =>
                optionsBuilder.UseUmbracoDatabaseProvider(serviceProvider));

        builder.PackageMigrationPlans().Add<MemberCommentsPackageMigrationPlan>();

        builder.Services.AddSingleton<ICommentBodyHtmlSanitizer, CommentBodyHtmlSanitizer>();
        builder.Services.AddScoped<ICommentService, CommentService>();
    }
}
