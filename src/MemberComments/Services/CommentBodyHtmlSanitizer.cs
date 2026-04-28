using System.Text.RegularExpressions;
using Ganss.Xss;

namespace MemberComments.Services;

public sealed class CommentBodyHtmlSanitizer : ICommentBodyHtmlSanitizer
{
    private static readonly Regex Tags = new("<[^>]+>", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private readonly HtmlSanitizer _sanitizer = new();

    /// <inheritdoc />
    public string SanitizeForStorageAndDisplay(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        string sanitized = _sanitizer.Sanitize(html.Trim());
        // Prevents closing the host textarea if HTML is pasted into an edit field rendered as &lt;textarea&gt;.
        return sanitized.Replace("</textarea", "&lt;/textarea", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>True when there is no user-visible text (e.g. empty paragraphs only).</summary>
    public static bool IsVisuallyEmpty(string html) =>
        string.IsNullOrWhiteSpace(Tags.Replace(html, string.Empty));
}
