using Umbraco.Cms.Core.Models.PublishedContent;

namespace MemberComments.Services;

public interface ICommentService
{
    /// <summary>Returns comments for public rendering. Soft-deleted rows use placeholder <see cref="CommentViewModel.Text"/>; original bodies are only in the database.</summary>
    Task<IReadOnlyList<CommentViewModel>> GetCommentsForContentAsync(
        Guid contentKey,
        CancellationToken cancellationToken = default);

    Task<CommentSaveResult> TryCreateAsync(
        IPublishedContent page,
        int? parentId,
        Guid memberKey,
        string authorName,
        string text,
        CancellationToken cancellationToken = default);

    Task<CommentSaveResult> TryUpdateAsync(
        IPublishedContent page,
        int commentId,
        Guid memberKey,
        bool isModerator,
        int? moderatorMemberIntId,
        string newText,
        CancellationToken cancellationToken = default);

    Task<CommentSaveResult> TrySoftDeleteAsync(
        IPublishedContent page,
        int commentId,
        Guid memberKey,
        bool isModerator,
        int? moderatorMemberIntId,
        CancellationToken cancellationToken = default);
}
