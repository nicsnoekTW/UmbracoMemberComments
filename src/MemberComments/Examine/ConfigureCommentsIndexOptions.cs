using Examine.Lucene;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Util;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace MemberComments.Examine;

/// <summary>Configures Lucene fields and analyzer for <see cref="CommentsIndex"/>.</summary>
public sealed class ConfigureCommentsIndexOptions : IConfigureNamedOptions<LuceneDirectoryIndexOptions>
{
    private readonly IOptions<IndexCreatorSettings> _indexCreatorSettings;

    public ConfigureCommentsIndexOptions(IOptions<IndexCreatorSettings> indexCreatorSettings) =>
        _indexCreatorSettings = indexCreatorSettings;

    public void Configure(string? name, LuceneDirectoryIndexOptions options)
    {
        if (name?.Equals(CommentsIndexConstants.IndexName, StringComparison.OrdinalIgnoreCase) is false)
        {
            return;
        }

        options.Analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48);

        options.FieldDefinitions = CommentsIndexFieldDefinitions.Create();

        options.UnlockIndex = true;

        if (_indexCreatorSettings.Value.LuceneDirectoryFactory == LuceneDirectoryFactory.SyncedTempFileSystemDirectoryFactory)
        {
            options.IndexDeletionPolicy = new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy());
        }
    }

    public void Configure(LuceneDirectoryIndexOptions options) { }
}
