using System.Linq;
using Examine;
using MemberComments.Data;
using MemberComments.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Persistence.EFCore.Scoping;

namespace MemberComments.Examine;

/// <summary>Rebuilds <see cref="CommentsIndex"/> from all non-deleted comments.</summary>
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
            int lastId = 0;
            const int batchSize = 1000;

            while (true)
            {
                using IEfCoreScope<MemberCommentsDbContext> scope = _efCoreScopeProvider.CreateScope();
                List<Comment> batch = scope.ExecuteWithContextAsync(async db =>
                    await db.Comments
                        .AsNoTracking()
                        .Where(c => c.DeletedUtc == null && c.Id > lastId)
                        .OrderBy(c => c.Id)
                        .Take(batchSize)
                        .ToListAsync(CancellationToken.None)).GetAwaiter().GetResult();
                scope.Complete();

                if (batch.Count == 0)
                {
                    break;
                }

                index.IndexItems(_valueSetBuilder.GetValueSets(batch.ToArray()));
                lastId = batch[^1].Id;
            }
        }
    }
}
