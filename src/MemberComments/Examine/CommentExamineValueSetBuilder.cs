using System.Text;
using System.Text.RegularExpressions;
using Examine;
using MemberComments.Data.Entities;

namespace MemberComments.Examine;

/// <summary>Builds one Examine <see cref="ValueSet"/> per content node (aggregated non-deleted comments).</summary>
public sealed class CommentExamineValueSetBuilder
{
    private static readonly Regex HtmlTags = new("<[^>]+>", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>Returns a value set with id <see cref="CommentsIndexConstants.IndexDocumentId"/> for the page, or <c>null</c> when there is nothing to index.</summary>
    public ValueSet? GetValueSetForContent(int contentId, IReadOnlyList<Comment> commentsOnPage)
    {
        List<Comment> active = commentsOnPage.Where(c => c.DeletedUtc is null).OrderBy(c => c.Id).ToList();
        if (active.Count == 0)
        {
            return null;
        }

        var subject = new StringBuilder();
        var author = new StringBuilder();
        var text = new StringBuilder();
        foreach (Comment c in active)
        {
            if (subject.Length > 0)
            {
                subject.Append(' ');
            }

            subject.Append(c.Subject ?? string.Empty);

            if (author.Length > 0)
            {
                author.Append(' ');
            }

            author.Append(c.AuthorName ?? string.Empty);

            if (text.Length > 0)
            {
                text.Append(' ');
            }

            text.Append(StripHtml(c.Text));
        }

        var values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            [CommentsIndexConstants.FieldNodeId] = contentId,
            [CommentsIndexConstants.FieldSubject] = subject.ToString(),
            [CommentsIndexConstants.FieldAuthorName] = author.ToString(),
            [CommentsIndexConstants.FieldText] = text.ToString(),
        };

        string id = CommentsIndexConstants.IndexDocumentId(contentId);
        return new ValueSet(id, CommentsIndexConstants.Category, CommentsIndexConstants.Category, values);
    }

    /// <summary>Removes HTML tags so body text is tokenized like user search terms.</summary>
    internal static string StripHtml(string? html) =>
        string.IsNullOrEmpty(html) ? string.Empty : HtmlTags.Replace(html, " ").Trim();
}
