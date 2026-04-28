namespace MemberComments.ViewModels;

/// <summary>Context passed into the recursive comment node partial.</summary>
public sealed class CommentNodeVm
{
    public required CommentThreadNode Node { get; init; }

    public required MemberCommentsViewModel Page { get; init; }
}
