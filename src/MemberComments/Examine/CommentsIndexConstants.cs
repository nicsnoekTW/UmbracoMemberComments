namespace MemberComments.Examine;

/// <summary>
/// Names and categories for the custom Examine <see cref="CommentsIndex"/>.
/// Search across <see cref="FieldSubject"/>, <see cref="FieldText"/>, and <see cref="FieldAuthorName"/> (e.g. grouped OR in Examine);
/// read <see cref="FieldContentKey"/> from each hit to resolve the Umbraco page.
/// </summary>
public static class CommentsIndexConstants
{
    public const string IndexName = "CommentsIndex";

    /// <summary>ValueSet category — pass to <c>ISearcher.CreateQuery</c> when searching this index.</summary>
    public const string Category = "comment";

    public const string FieldContentKey = "contentKey";
    public const string FieldCommentId = "commentId";
    public const string FieldSubject = "subject";
    public const string FieldAuthorName = "authorName";
    public const string FieldText = "text";
}
