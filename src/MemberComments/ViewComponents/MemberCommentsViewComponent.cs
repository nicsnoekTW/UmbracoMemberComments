using MemberComments.Services;
using MemberComments.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace MemberComments.ViewComponents;

[ViewComponent(Name = "MemberComments")]
public sealed class MemberCommentsViewComponent : ViewComponent
{
    private readonly ICommentService _commentService;
    private readonly IMemberManager _memberManager;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;

    public MemberCommentsViewComponent(
        ICommentService commentService,
        IMemberManager memberManager,
        IUmbracoContextAccessor umbracoContextAccessor)
    {
        _commentService = commentService;
        _memberManager = memberManager;
        _umbracoContextAccessor = umbracoContextAccessor;
    }

    public async Task<IViewComponentResult> InvokeAsync(Guid contentKey)
    {
        IPublishedContent? page = _umbracoContextAccessor.GetRequiredUmbracoContext().Content?.GetById(contentKey);
        if (page is null)
        {
            return View(new MemberCommentsViewModel { ContentKey = contentKey, CanView = false });
        }

        bool canView = await _memberManager.MemberHasAccessAsync(page.Path);
        if (canView is false)
        {
            return View(new MemberCommentsViewModel { ContentKey = contentKey, CanView = false });
        }

        MemberIdentityUser? member = await _memberManager.GetCurrentMemberAsync();
        Guid? memberKey = member?.Key;
        bool canPost = member is not null && canView;
        bool isModerator = member is not null &&
            await _memberManager.IsMemberAuthorizedAsync(allowGroups: new[] { Constants.CommentModeratorsGroupName });

        IReadOnlyList<CommentViewModel> comments = await _commentService.GetCommentsForContentAsync(contentKey);
        var byId = comments.ToDictionary(c => c.Id);
        var rows = comments
            .OrderBy(c => c.CreatedUtc)
            .Select(c => new MemberCommentsDisplayRow(c, Depth(c, byId)))
            .ToList();

        string? error = ViewContext.TempData?["MemberCommentsError"] as string;

        return View(new MemberCommentsViewModel
        {
            ContentKey = contentKey,
            Rows = rows,
            CanView = true,
            CanPost = canPost,
            IsModerator = isModerator,
            CurrentMemberKey = memberKey,
            ErrorMessage = error,
        });
    }

    private static int Depth(CommentViewModel comment, IReadOnlyDictionary<int, CommentViewModel> byId)
    {
        int depth = 0;
        int? parentId = comment.ParentId;
        while (parentId is int pid && byId.TryGetValue(pid, out CommentViewModel? parent))
        {
            depth++;
            parentId = parent.ParentId;
            if (depth > 64)
            {
                break;
            }
        }

        return depth;
    }
}
