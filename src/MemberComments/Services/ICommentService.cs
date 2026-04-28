using Umbraco.Cms.Core.Models.PublishedContent;

namespace MemberComments.Services;

public interface ICommentService
{
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
        string newText,
        CancellationToken cancellationToken = default);
}
