namespace MemberComments.Data.Entities;

/// <summary>Member comment on a content page; optional <see cref="ParentId"/> for replies.</summary>
public sealed class Comment
{
    public int Id { get; set; }

    /// <summary>When set, this comment is a reply to another comment on the same page.</summary>
    public int? ParentId { get; set; }

    public Comment? Parent { get; set; }

    public ICollection<Comment> Replies { get; set; } = new List<Comment>();

    public Guid ContentKey { get; set; }

    public Guid MemberKey { get; set; }

    /// <summary>Display name captured when the comment was created.</summary>
    public string AuthorName { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public DateTimeOffset CreatedUtc { get; set; }

    /// <summary>When the comment was last edited; null if never edited.</summary>
    public DateTimeOffset? EditedUtc { get; set; }
}
