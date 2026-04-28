using MemberComments.Services;

namespace MemberComments.ViewModels;

public sealed record MemberCommentsDisplayRow(CommentViewModel Comment, int Depth);
