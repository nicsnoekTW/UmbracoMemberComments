namespace MemberComments.Examine;

/// <summary>Keeps <see cref="CommentsIndexConstants.IndexName"/> in sync with the comments database.</summary>
public interface ICommentsExamineIndexer
{
    /// <summary>Loads the comment by id and indexes it, or removes it if missing or soft-deleted.</summary>
    Task UpsertAsync(int commentId, CancellationToken cancellationToken = default);

    /// <summary>Removes the comment document from the index.</summary>
    Task DeleteAsync(int commentId, CancellationToken cancellationToken = default);
}
