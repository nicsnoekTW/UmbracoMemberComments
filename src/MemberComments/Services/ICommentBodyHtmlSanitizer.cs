namespace MemberComments.Services;

/// <summary>Strips unsafe markup from member-submitted comment HTML before storage and when building view models.</summary>
public interface ICommentBodyHtmlSanitizer
{
    string SanitizeForStorageAndDisplay(string? html);
}
