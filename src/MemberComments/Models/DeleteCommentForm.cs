using System.ComponentModel.DataAnnotations;

namespace MemberComments.Models;

public sealed class DeleteCommentForm
{
    [Range(1, int.MaxValue)]
    public int ContentId { get; set; }

    [Required]
    public int CommentId { get; set; }
}
