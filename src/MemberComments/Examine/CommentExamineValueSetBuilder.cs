using System.Text.RegularExpressions;
using Examine;
using MemberComments.Data.Entities;
using Umbraco.Cms.Infrastructure.Examine;

namespace MemberComments.Examine;

/// <summary>Builds Examine <see cref="ValueSet"/>s from <see cref="Comment"/> rows.</summary>
public sealed class CommentExamineValueSetBuilder : IValueSetBuilder<Comment>
{
    private static readonly Regex HtmlTags = new("<[^>]+>", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <inheritdoc />
    public IEnumerable<ValueSet> GetValueSets(params Comment[] comments)
    {
        foreach (Comment comment in comments)
        {
            if (comment.DeletedUtc.HasValue)
            {
                continue;
            }

            var values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                [CommentsIndexConstants.FieldCommentId] = comment.Id,
                [CommentsIndexConstants.FieldContentKey] = comment.ContentKey.ToString("D"),
                [CommentsIndexConstants.FieldSubject] = comment.Subject ?? string.Empty,
                [CommentsIndexConstants.FieldAuthorName] = comment.AuthorName ?? string.Empty,
                [CommentsIndexConstants.FieldText] = StripHtml(comment.Text),
            };

            yield return new ValueSet(
                comment.Id.ToString(),
                CommentsIndexConstants.Category,
                CommentsIndexConstants.Category,
                values);
        }
    }

    /// <summary>Removes HTML tags so body text is tokenized like user search terms.</summary>
    internal static string StripHtml(string? html) =>
        string.IsNullOrEmpty(html) ? string.Empty : HtmlTags.Replace(html, " ").Trim();
}
