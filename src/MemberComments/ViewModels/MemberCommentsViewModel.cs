namespace MemberComments.ViewModels;

public sealed class MemberCommentsViewModel
{
    public int ContentId { get; init; }

    public IReadOnlyList<CommentThreadNode> RootThreads { get; init; } = Array.Empty<CommentThreadNode>();

    public bool CanView { get; init; }

    public bool CanPost { get; init; }

    public bool IsModerator { get; init; }

    public Guid? CurrentMemberKey { get; init; }

    public string? ErrorMessage { get; init; }
}
