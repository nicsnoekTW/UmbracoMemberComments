using System.ComponentModel.DataAnnotations;

namespace MemberComments.Models;

public sealed class CreateCommentForm
{
    [Required]
    public Guid ContentKey { get; set; }

    public int? ParentId { get; set; }

    [StringLength(256)]
    public string? Subject { get; set; }

    [Required]
    [StringLength(100_000)]
    public string? Text { get; set; }
}
