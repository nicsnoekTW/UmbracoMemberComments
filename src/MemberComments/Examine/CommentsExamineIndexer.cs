using Examine;
using MemberComments.Data;
using MemberComments.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Search;
using Umbraco.Cms.Persistence.EFCore.Scoping;

namespace MemberComments.Examine;

/// <inheritdoc />
public sealed class CommentsExamineIndexer : ICommentsExamineIndexer
{
    private readonly IExamineManager _examineManager;
    private readonly IEFCoreScopeProvider<MemberCommentsDbContext> _efCoreScopeProvider;
    private readonly CommentExamineValueSetBuilder _valueSetBuilder;
    private readonly IRuntimeState _runtimeState;
    private readonly IUmbracoIndexingHandler _umbracoIndexingHandler;
    private readonly ILogger<CommentsExamineIndexer> _logger;

    public CommentsExamineIndexer(
        IExamineManager examineManager,
        IEFCoreScopeProvider<MemberCommentsDbContext> efCoreScopeProvider,
        CommentExamineValueSetBuilder valueSetBuilder,
        IRuntimeState runtimeState,
        IUmbracoIndexingHandler umbracoIndexingHandler,
        ILogger<CommentsExamineIndexer> logger)
    {
        _examineManager = examineManager;
        _efCoreScopeProvider = efCoreScopeProvider;
        _valueSetBuilder = valueSetBuilder;
        _runtimeState = runtimeState;
        _umbracoIndexingHandler = umbracoIndexingHandler;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task ReindexForContentAsync(int contentId, CancellationToken cancellationToken = default)
    {
        if (ShouldSkipIndexing())
        {
            return;
        }

        if (_examineManager.TryGetIndex(CommentsIndexConstants.IndexName, out IIndex? index) is false || index is null)
        {
            _logger.LogWarning("Examine index {IndexName} is not registered.", CommentsIndexConstants.IndexName);
            return;
        }

        using IEfCoreScope<MemberCommentsDbContext> scope = _efCoreScopeProvider.CreateScope();
        List<Comment> comments = await scope.ExecuteWithContextAsync(async db =>
            await db.Comments
                .AsNoTracking()
                .Where(c => c.ContentId == contentId)
                .OrderBy(c => c.Id)
                .ToListAsync(cancellationToken));
        scope.Complete();

        ValueSet? valueSet = _valueSetBuilder.GetValueSetForContent(contentId, comments);
        string docId = CommentsIndexConstants.IndexDocumentId(contentId);
        if (valueSet is null)
        {
            index.DeleteFromIndex(docId);
            return;
        }

        index.IndexItems(new[] { valueSet });
    }

    private bool ShouldSkipIndexing()
    {
        if (_runtimeState.Level != RuntimeLevel.Run)
        {
            return true;
        }

        if (_umbracoIndexingHandler.Enabled is false)
        {
            return true;
        }

        return false;
    }
}
