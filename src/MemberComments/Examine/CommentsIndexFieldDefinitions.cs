using Examine;

namespace MemberComments.Examine;

/// <summary>Shared Lucene field definitions for <see cref="CommentsIndex"/> (must match <see cref="ConfigureCommentsIndexOptions"/>).</summary>
internal static class CommentsIndexFieldDefinitions
{
    public static FieldDefinitionCollection Create() =>
        new FieldDefinitionCollection(
            new FieldDefinition(CommentsIndexConstants.FieldNodeId, FieldDefinitionTypes.Integer),
            new FieldDefinition(CommentsIndexConstants.FieldSubject, FieldDefinitionTypes.FullText),
            new FieldDefinition(CommentsIndexConstants.FieldAuthorName, FieldDefinitionTypes.FullText),
            new FieldDefinition(CommentsIndexConstants.FieldText, FieldDefinitionTypes.FullText));
}
