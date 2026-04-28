using MemberComments.Services;

namespace MemberComments.ViewModels;

/// <summary>Recursive thread node for rendering comment trees.</summary>
public sealed class CommentThreadNode
{
    public CommentThreadNode(CommentViewModel comment, IReadOnlyList<CommentThreadNode> children)
    {
        Comment = comment;
        Children = children;
    }

    public CommentViewModel Comment { get; }

    public IReadOnlyList<CommentThreadNode> Children { get; }
}
