using MemberComments;
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
        IReadOnlyList<CommentThreadNode> roots = BuildThreadTree(comments);

        string? error = ViewContext.TempData?["MemberCommentsError"] as string;

        return View(new MemberCommentsViewModel
        {
            ContentKey = contentKey,
            RootThreads = roots,
            CanView = true,
            CanPost = canPost,
            IsModerator = isModerator,
            CurrentMemberKey = memberKey,
            ErrorMessage = error,
        });
    }

    private static IReadOnlyList<CommentThreadNode> BuildThreadTree(IReadOnlyList<CommentViewModel> flat)
    {
        if (flat.Count == 0)
        {
            return Array.Empty<CommentThreadNode>();
        }

        Dictionary<int, List<CommentViewModel>> childrenByParent = flat
            .Where(c => c.ParentId.HasValue)
            .GroupBy(c => c.ParentId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(c => c.CreatedUtc).ToList());

        return flat
            .Where(c => c.ParentId is null)
            .OrderBy(c => c.CreatedUtc)
            .Select(root => new CommentThreadNode(root, BuildChildren(root.Id, childrenByParent)))
            .ToList();
    }

    private static IReadOnlyList<CommentThreadNode> BuildChildren(
        int parentId,
        Dictionary<int, List<CommentViewModel>> childrenByParent)
    {
        if (childrenByParent.TryGetValue(parentId, out List<CommentViewModel>? kids) is false)
        {
            return Array.Empty<CommentThreadNode>();
        }

        return kids
            .Select(k => new CommentThreadNode(k, BuildChildren(k.Id, childrenByParent)))
            .ToList();
    }
}
