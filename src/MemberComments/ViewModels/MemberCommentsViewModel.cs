using MemberComments.Services;

namespace MemberComments.ViewModels;

public sealed class MemberCommentsViewModel
{
    public Guid ContentKey { get; init; }

    public IReadOnlyList<MemberCommentsDisplayRow> Rows { get; init; } = Array.Empty<MemberCommentsDisplayRow>();

    public bool CanView { get; init; }

    public bool CanPost { get; init; }

    public bool IsModerator { get; init; }

    public Guid? CurrentMemberKey { get; init; }

    public string? ErrorMessage { get; init; }
}
