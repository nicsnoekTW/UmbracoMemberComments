using System.Linq;
using Examine;
using MemberComments.Data;
using MemberComments.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Persistence.EFCore.Scoping;

namespace MemberComments.Examine;

/// <summary>Rebuilds <see cref="CommentsIndex"/> from all non-deleted comments (one document per content node).</summary>
public sealed class CommentsIndexPopulator : IIndexPopulator
{
    private readonly IEFCoreScopeProvider<MemberCommentsDbContext> _efCoreScopeProvider;
    private readonly CommentExamineValueSetBuilder _valueSetBuilder;

    public CommentsIndexPopulator(
        IEFCoreScopeProvider<MemberCommentsDbContext> efCoreScopeProvider,
        CommentExamineValueSetBuilder valueSetBuilder)
    {
        _efCoreScopeProvider = efCoreScopeProvider;
        _valueSetBuilder = valueSetBuilder;
    }

    /// <inheritdoc />
    public bool IsRegistered(IIndex index) =>
        index.Name.Equals(CommentsIndexConstants.IndexName, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public void Populate(IIndex[] indexes)
    {
        foreach (IIndex index in indexes.Where(IsRegistered))
        {
            using IEfCoreScope<MemberCommentsDbContext> scope = _efCoreScopeProvider.CreateScope();
            List<Comment> all = scope.ExecuteWithContextAsync(async db =>
                await db.Comments
                    .AsNoTracking()
                    .Where(c => c.DeletedUtc == null)
                    .OrderBy(c => c.ContentId)
                    .ThenBy(c => c.Id)
                    .ToListAsync(CancellationToken.None)).GetAwaiter().GetResult();
            scope.Complete();

            foreach (IGrouping<int, Comment> group in all.GroupBy(c => c.ContentId))
            {
                ValueSet? valueSet = _valueSetBuilder.GetValueSetForContent(group.Key, group.ToList());
                if (valueSet is not null)
                {
                    index.IndexItems(new[] { valueSet });
                }
            }
        }
    }
}
