using Examine;
using Examine.Lucene;
using Examine.Lucene.Providers;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;
using MemberComments.Data;
using MemberComments.Examine;
using MemberComments.Migrations.Plans;
using MemberComments.Services;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Examine;
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

        builder.Services.AddExamineLuceneIndex<CommentsIndex, ConfigurationEnabledDirectoryFactory>(
            CommentsIndexConstants.IndexName,
            CommentsIndexFieldDefinitions.Create(),
            new StandardAnalyzer(LuceneVersion.LUCENE_48),
            new ValueSetValidatorDelegate(vs => new ValueSetValidationResult(ValueSetValidationStatus.Valid, vs)),
            new Dictionary<string, IFieldValueTypeFactory>(StringComparer.OrdinalIgnoreCase));
        builder.Services.ConfigureOptions<ConfigureCommentsIndexOptions>();
        builder.Services.AddSingleton<CommentExamineValueSetBuilder>();
        builder.Services.AddSingleton<ICommentsExamineIndexer, CommentsExamineIndexer>();
        builder.Services.AddSingleton<IIndexPopulator, CommentsIndexPopulator>();
    }
}
