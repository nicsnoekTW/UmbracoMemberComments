using Examine.Lucene;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Examine;

namespace MemberComments.Examine;

/// <summary>Lucene Examine index for member comments (see <see cref="CommentsIndexConstants.IndexName"/>).</summary>
public sealed class CommentsIndex : UmbracoExamineIndex
{
    public CommentsIndex(
        ILoggerFactory loggerFactory,
        string name,
        IOptionsMonitor<LuceneDirectoryIndexOptions> indexOptions,
        IHostingEnvironment hostingEnvironment,
        IRuntimeState runtimeState)
        : base(loggerFactory, name, indexOptions, hostingEnvironment, runtimeState)
    {
    }
}
