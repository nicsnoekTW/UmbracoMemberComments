namespace MemberComments.Services;

public sealed record CommentViewModel(
    int Id,
    int? ParentId,
    Guid MemberKey,
    string AuthorName,
    string Text,
    DateTimeOffset CreatedUtc,
    DateTimeOffset? EditedUtc);
