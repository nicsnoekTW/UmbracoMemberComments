using System.ComponentModel.DataAnnotations;

namespace MemberComments.Models;

public sealed class DeleteCommentForm
{
    [Required]
    public Guid ContentKey { get; set; }

    [Required]
    public int CommentId { get; set; }
}
