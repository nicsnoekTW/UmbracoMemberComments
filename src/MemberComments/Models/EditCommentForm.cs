using System.ComponentModel.DataAnnotations;

namespace MemberComments.Models;

public sealed class EditCommentForm
{
    [Required]
    public Guid ContentKey { get; set; }

    [Required]
    public int CommentId { get; set; }

    [Required]
    [StringLength(4000, MinimumLength = 1)]
    public string? Text { get; set; }
}
