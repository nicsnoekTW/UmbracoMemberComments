using MemberComments.Data;
using MemberComments.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Persistence.EFCore.Scoping;

namespace MemberComments.Services;

public sealed class CommentService : ICommentService
{
    private const int MaxTextLength = 4000;
    private readonly IEFCoreScopeProvider<MemberCommentsDbContext> _efCoreScopeProvider;

    public CommentService(IEFCoreScopeProvider<MemberCommentsDbContext> efCoreScopeProvider) =>
        _efCoreScopeProvider = efCoreScopeProvider;

    /// <inheritdoc />
    public async Task<IReadOnlyList<CommentViewModel>> GetCommentsForContentAsync(
        Guid contentKey,
        CancellationToken cancellationToken = default)
    {
        using IEfCoreScope<MemberCommentsDbContext> scope = _efCoreScopeProvider.CreateScope();
        List<CommentViewModel> rows = await scope.ExecuteWithContextAsync(async db =>
        {
            List<Comment> list = await db.Comments
                .AsNoTracking()
                .Where(c => c.ContentKey == contentKey)
                .OrderBy(c => c.Id)
                .ToListAsync(cancellationToken);

            return list
                .Select(MapToViewModel)
                .ToList();
        });

        scope.Complete();
        return rows;
    }

    /// <inheritdoc />
    public async Task<CommentSaveResult> TryCreateAsync(
        IPublishedContent page,
        int? parentId,
        Guid memberKey,
        string authorName,
        string text,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return CommentSaveResult.Fail("Comment cannot be empty.");
        }

        if (text.Length > MaxTextLength)
        {
            return CommentSaveResult.Fail($"Comment cannot exceed {MaxTextLength} characters.");
        }

        if (string.IsNullOrWhiteSpace(authorName))
        {
            return CommentSaveResult.Fail("Author name is required.");
        }

        Guid contentKey = page.Key;

        using IEfCoreScope<MemberCommentsDbContext> scope = _efCoreScopeProvider.CreateScope();
        CommentSaveResult result = await scope.ExecuteWithContextAsync(async db =>
        {
            if (parentId is int pid)
            {
                Comment? parent = await db.Comments.FirstOrDefaultAsync(
                    c => c.Id == pid && c.ContentKey == contentKey,
                    cancellationToken);
                if (parent is null)
                {
                    return CommentSaveResult.Fail("Parent comment was not found on this page.");
                }

                if (parent.DeletedUtc.HasValue)
                {
                    return CommentSaveResult.Fail("Cannot reply to a deleted comment.");
                }
            }

            var entity = new Comment
            {
                ContentKey = contentKey,
                ParentId = parentId,
                MemberKey = memberKey,
                AuthorName = authorName.Trim(),
                Text = text.Trim(),
                CreatedUtc = DateTimeOffset.UtcNow,
                EditedUtc = null,
                DeletedUtc = null,
                ModeratorId = null,
            };

            db.Comments.Add(entity);
            await db.SaveChangesAsync(cancellationToken);
            return CommentSaveResult.Ok();
        });

        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<CommentSaveResult> TryUpdateAsync(
        IPublishedContent page,
        int commentId,
        Guid memberKey,
        bool isModerator,
        int? moderatorMemberIntId,
        string newText,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(newText))
        {
            return CommentSaveResult.Fail("Comment cannot be empty.");
        }

        if (newText.Length > MaxTextLength)
        {
            return CommentSaveResult.Fail($"Comment cannot exceed {MaxTextLength} characters.");
        }

        Guid contentKey = page.Key;

        using IEfCoreScope<MemberCommentsDbContext> scope = _efCoreScopeProvider.CreateScope();
        CommentSaveResult result = await scope.ExecuteWithContextAsync(async db =>
        {
            Comment? entity = await db.Comments.FirstOrDefaultAsync(
                c => c.Id == commentId && c.ContentKey == contentKey,
                cancellationToken);

            if (entity is null)
            {
                return CommentSaveResult.Fail("Comment was not found.");
            }

            if (entity.DeletedUtc.HasValue)
            {
                return CommentSaveResult.Fail("Cannot edit a deleted comment.");
            }

            if (entity.MemberKey != memberKey && isModerator is false)
            {
                return CommentSaveResult.Fail("You do not have permission to edit this comment.");
            }

            if (entity.MemberKey == memberKey)
            {
                entity.ModeratorId = null;
            }
            else if (isModerator)
            {
                if (moderatorMemberIntId is null)
                {
                    return CommentSaveResult.Fail("Could not resolve moderator member id.");
                }

                entity.ModeratorId = moderatorMemberIntId;
            }

            entity.Text = newText.Trim();
            entity.EditedUtc = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            return CommentSaveResult.Ok();
        });

        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<CommentSaveResult> TrySoftDeleteAsync(
        IPublishedContent page,
        int commentId,
        Guid memberKey,
        bool isModerator,
        int? moderatorMemberIntId,
        CancellationToken cancellationToken = default)
    {
        Guid contentKey = page.Key;

        using IEfCoreScope<MemberCommentsDbContext> scope = _efCoreScopeProvider.CreateScope();
        CommentSaveResult result = await scope.ExecuteWithContextAsync(async db =>
        {
            Comment? entity = await db.Comments.FirstOrDefaultAsync(
                c => c.Id == commentId && c.ContentKey == contentKey,
                cancellationToken);

            if (entity is null)
            {
                return CommentSaveResult.Fail("Comment was not found.");
            }

            if (entity.DeletedUtc.HasValue)
            {
                return CommentSaveResult.Ok();
            }

            if (entity.MemberKey != memberKey && isModerator is false)
            {
                return CommentSaveResult.Fail("You do not have permission to delete this comment.");
            }

            entity.DeletedUtc = DateTimeOffset.UtcNow;

            if (entity.MemberKey != memberKey && isModerator)
            {
                if (moderatorMemberIntId is null)
                {
                    return CommentSaveResult.Fail("Could not resolve moderator member id.");
                }

                entity.ModeratorId = moderatorMemberIntId;
            }
            else
            {
                entity.ModeratorId = null;
            }

            await db.SaveChangesAsync(cancellationToken);
            return CommentSaveResult.Ok();
        });

        scope.Complete();
        return result;
    }

    private static CommentViewModel MapToViewModel(Comment c)
    {
        bool isDeleted = c.DeletedUtc.HasValue;
        string publicText = isDeleted
            ? (c.ModeratorId.HasValue
                ? CommentViewModel.DeletedCommentByModeratorPlaceholder
                : CommentViewModel.DeletedCommentPlaceholder)
            : c.Text;

        return new CommentViewModel(
            c.Id,
            c.ParentId,
            c.MemberKey,
            c.AuthorName,
            publicText,
            c.CreatedUtc,
            c.EditedUtc,
            c.DeletedUtc,
            c.ModeratorId);
    }
}
