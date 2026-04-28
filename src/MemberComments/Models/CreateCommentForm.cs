using System.ComponentModel.DataAnnotations;

namespace MemberComments.Models;

public sealed class CreateCommentForm
{
    [Required]
    public Guid ContentKey { get; set; }

    public int? ParentId { get; set; }

    [Required]
    [StringLength(4000, MinimumLength = 1)]
    public string? Text { get; set; }
}
