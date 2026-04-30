namespace MemberComments.Examine;

/// <summary>Keeps <see cref="CommentsIndexConstants.IndexName"/> in sync with the comments database (one document per content node).</summary>
public interface ICommentsExamineIndexer
{
    /// <summary>Rebuilds the index document for the given content node from all non-deleted comments; removes the document if none remain.</summary>
    Task ReindexForContentAsync(int contentId, CancellationToken cancellationToken = default);
}
