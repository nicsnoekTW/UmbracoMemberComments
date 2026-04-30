using MemberComments.Data;
using MemberComments.Data.Entities;
using MemberComments.Examine;
using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Persistence.EFCore.Scoping;

namespace MemberComments.Services;

public sealed class CommentService : ICommentService
{
    private const int MaxSubjectLength = 256;
    private const int MaxTextLength = 100_000;
    private readonly IEFCoreScopeProvider<MemberCommentsDbContext> _efCoreScopeProvider;
    private readonly ICommentBodyHtmlSanitizer _htmlSanitizer;
    private readonly ICommentsExamineIndexer _commentsExamineIndexer;

    public CommentService(
        IEFCoreScopeProvider<MemberCommentsDbContext> efCoreScopeProvider,
        ICommentBodyHtmlSanitizer htmlSanitizer,
        ICommentsExamineIndexer commentsExamineIndexer)
    {
        _efCoreScopeProvider = efCoreScopeProvider;
        _htmlSanitizer = htmlSanitizer;
        _commentsExamineIndexer = commentsExamineIndexer;
    }

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
        string? subject,
        string text,
        CancellationToken cancellationToken = default)
    {
        string subjectTrimmed = NormalizeSubject(subject);

        if (subjectTrimmed.Length > MaxSubjectLength)
        {
            return CommentSaveResult.Fail($"Subject cannot exceed {MaxSubjectLength} characters.");
        }

        string sanitized = _htmlSanitizer.SanitizeForStorageAndDisplay(text);
        if (CommentBodyHtmlSanitizer.IsVisuallyEmpty(sanitized))
        {
            return CommentSaveResult.Fail("Comment cannot be empty.");
        }

        if (sanitized.Length > MaxTextLength)
        {
            return CommentSaveResult.Fail($"Comment cannot exceed {MaxTextLength} characters.");
        }

        if (string.IsNullOrWhiteSpace(authorName))
        {
            return CommentSaveResult.Fail("Author name is required.");
        }

        Guid contentKey = page.Key;

        using IEfCoreScope<MemberCommentsDbContext> scope = _efCoreScopeProvider.CreateScope();
        int? newCommentId = null;
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
                Subject = subjectTrimmed,
                Text = sanitized,
                CreatedUtc = DateTimeOffset.UtcNow,
                EditedUtc = null,
                DeletedUtc = null,
                ModeratorId = null,
            };

            db.Comments.Add(entity);
            await db.SaveChangesAsync(cancellationToken);
            newCommentId = entity.Id;
            return CommentSaveResult.Ok();
        });

        scope.Complete();
        if (result.Success && newCommentId.HasValue)
        {
            await _commentsExamineIndexer.UpsertAsync(newCommentId.Value, cancellationToken);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<CommentSaveResult> TryUpdateAsync(
        IPublishedContent page,
        int commentId,
        Guid memberKey,
        bool isModerator,
        int? moderatorMemberIntId,
        string? newSubject,
        string newText,
        CancellationToken cancellationToken = default)
    {
        string sanitized = _htmlSanitizer.SanitizeForStorageAndDisplay(newText);
        if (CommentBodyHtmlSanitizer.IsVisuallyEmpty(sanitized))
        {
            return CommentSaveResult.Fail("Comment cannot be empty.");
        }

        if (sanitized.Length > MaxTextLength)
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

            if (entity.ParentId is null)
            {
                string subjectTrimmed = NormalizeSubject(newSubject);
                if (subjectTrimmed.Length > MaxSubjectLength)
                {
                    return CommentSaveResult.Fail($"Subject cannot exceed {MaxSubjectLength} characters.");
                }

                entity.Subject = subjectTrimmed;
            }

            entity.Text = sanitized;
            entity.EditedUtc = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            return CommentSaveResult.Ok();
        });

        scope.Complete();
        if (result.Success)
        {
            await _commentsExamineIndexer.UpsertAsync(commentId, cancellationToken);
        }

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
        if (result.Success)
        {
            await _commentsExamineIndexer.DeleteAsync(commentId, cancellationToken);
        }

        return result;
    }

    private CommentViewModel MapToViewModel(Comment c)
    {
        bool isDeleted = c.DeletedUtc.HasValue;
        string publicText = isDeleted
            ? (c.ModeratorId.HasValue
                ? CommentViewModel.DeletedCommentByModeratorPlaceholder
                : CommentViewModel.DeletedCommentPlaceholder)
            : _htmlSanitizer.SanitizeForStorageAndDisplay(c.Text);

        string publicSubject = isDeleted ? string.Empty : c.Subject.Trim();

        return new CommentViewModel(
            c.Id,
            c.ParentId,
            c.MemberKey,
            c.AuthorName,
            publicSubject,
            publicText,
            c.CreatedUtc,
            c.EditedUtc,
            c.DeletedUtc,
            c.ModeratorId);
    }

    private static string NormalizeSubject(string? subject) => subject?.Trim() ?? string.Empty;
}
