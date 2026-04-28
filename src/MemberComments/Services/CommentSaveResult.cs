namespace MemberComments.Services;

public readonly record struct CommentSaveResult(bool Success, string? ErrorMessage)
{
    public static CommentSaveResult Ok() => new(true, null);

    public static CommentSaveResult Fail(string message) => new(false, message);
}
