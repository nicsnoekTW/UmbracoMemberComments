namespace MemberComments.Examine;

/// <summary>
/// Names and categories for the custom Examine <see cref="CommentsIndex"/>.
/// One index document per content node; <see cref="IndexDocumentId"/> and stored <see cref="FieldNodeId"/> match Umbraco <c>IPublishedContent.Id</c>.
/// Search across <see cref="FieldSubject"/>, <see cref="FieldText"/>, and <see cref="FieldAuthorName"/> (e.g. grouped OR in Examine).
/// </summary>
public static class CommentsIndexConstants
{
    public const string IndexName = "CommentsIndex";

    /// <summary>ValueSet category — pass to <c>ISearcher.CreateQuery</c> when searching this index.</summary>
    public const string Category = "comment";

    /// <summary>Lucene / Examine document id (same value as <see cref="FieldNodeId"/>).</summary>
    public static string IndexDocumentId(int contentId) => contentId.ToString();

    public const string FieldNodeId = "nodeId";
    public const string FieldSubject = "subject";
    public const string FieldAuthorName = "authorName";
    public const string FieldText = "text";
}
