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
                .Select(c => new CommentViewModel(
                    c.Id,
                    c.ParentId,
                    c.MemberKey,
                    c.AuthorName,
                    c.Text,
                    c.CreatedUtc,
                    c.EditedUtc))
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

            if (entity.MemberKey != memberKey && isModerator is false)
            {
                return CommentSaveResult.Fail("You do not have permission to edit this comment.");
            }

            entity.Text = newText.Trim();
            entity.EditedUtc = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            return CommentSaveResult.Ok();
        });

        scope.Complete();
        return result;
    }
}
