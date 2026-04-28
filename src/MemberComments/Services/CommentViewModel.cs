namespace MemberComments.Services;

/// <summary>Public view of a comment row. <paramref name="Text"/> is a placeholder when <paramref name="DeletedUtc"/> is set (original body is only persisted in the database).</summary>
public sealed record CommentViewModel(
    int Id,
    int? ParentId,
    Guid MemberKey,
    string AuthorName,
    string Text,
    DateTimeOffset CreatedUtc,
    DateTimeOffset? EditedUtc,
    DateTimeOffset? DeletedUtc,
    int? ModeratorId)
{
    public const string DeletedCommentPlaceholder = "[this comment has been deleted]";

    public const string DeletedCommentByModeratorPlaceholder = "[this comment has been deleted by a moderator]";

    public bool IsDeleted => DeletedUtc.HasValue;

    /// <summary>True when a moderator soft-deleted another member&apos;s comment (<see cref="ModeratorId"/> set).</summary>
    public bool IsDeletedByModerator => IsDeleted && ModeratorId.HasValue;

    /// <summary>True when the last edit was by a moderator on another member&apos;s comment (<see cref="ModeratorId"/> set).</summary>
    public bool IsModeratorAttributedEdit =>
        IsDeleted is false && EditedUtc.HasValue && ModeratorId.HasValue;
}
