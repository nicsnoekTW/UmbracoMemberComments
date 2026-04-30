using MemberComments.Models;
using MemberComments.Services;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Extensions;

namespace MemberComments.Controllers;

[PluginController(Constants.SurfacePluginControllerName)]
public sealed class CommentsSurfaceController : SurfaceController
{
    private readonly ICommentService _commentService;
    private readonly IMemberManager _memberManager;
    private readonly IMemberService _memberService;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly IPublishedUrlProvider _publishedUrlProvider;

    public CommentsSurfaceController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        ICommentService commentService,
        IMemberManager memberManager,
        IMemberService memberService)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _commentService = commentService;
        _memberManager = memberManager;
        _memberService = memberService;
        _umbracoContextAccessor = umbracoContextAccessor;
        _publishedUrlProvider = publishedUrlProvider;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(Prefix = "")] CreateCommentForm model, CancellationToken cancellationToken)
    {
        IPublishedContent? page = ResolvePage(model.ContentId);
        if (page is null)
        {
            return NotFound();
        }

        if (await _memberManager.MemberHasAccessAsync(page.Path) is false)
        {
            return Forbid();
        }

        MemberIdentityUser? member = await _memberManager.GetCurrentMemberAsync();
        if (member is null)
        {
            return Forbid();
        }

        if (ModelState.IsValid is false)
        {
            TempData["MemberCommentsError"] = "Please check the comment form and try again.";
            return RedirectToUmbracoPage(page);
        }

        string authorName = string.IsNullOrWhiteSpace(member.Name) ? member.UserName ?? "Member" : member.Name;
        CommentSaveResult result = await _commentService.TryCreateAsync(
            page,
            model.ParentId,
            member.Key,
            authorName,
            model.Subject,
            model.Text ?? string.Empty,
            cancellationToken);

        if (result.Success is false)
        {
            TempData["MemberCommentsError"] = result.ErrorMessage;
        }

        return RedirectToUmbracoPage(page);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([Bind(Prefix = "")] EditCommentForm model, CancellationToken cancellationToken)
    {
        IPublishedContent? page = ResolvePage(model.ContentId);
        if (page is null)
        {
            return NotFound();
        }

        if (await _memberManager.MemberHasAccessAsync(page.Path) is false)
        {
            return Forbid();
        }

        MemberIdentityUser? member = await _memberManager.GetCurrentMemberAsync();
        if (member is null)
        {
            return Forbid();
        }

        if (ModelState.IsValid is false)
        {
            TempData["MemberCommentsError"] = "Please enter a valid comment.";
            return RedirectToUmbracoPage(page);
        }

        bool isModerator = await _memberManager.IsMemberAuthorizedAsync(
            allowGroups: new[] { Constants.CommentModeratorsGroupName });

        int? moderatorMemberIntId = await ResolveModeratorMemberIntIdAsync(member, isModerator, cancellationToken);

        CommentSaveResult result = await _commentService.TryUpdateAsync(
            page,
            model.CommentId,
            member.Key,
            isModerator,
            moderatorMemberIntId,
            model.Subject,
            model.Text ?? string.Empty,
            cancellationToken);

        if (result.Success is false)
        {
            TempData["MemberCommentsError"] = result.ErrorMessage;
        }

        return RedirectToUmbracoPage(page);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete([Bind(Prefix = "")] DeleteCommentForm model, CancellationToken cancellationToken)
    {
        IPublishedContent? page = ResolvePage(model.ContentId);
        if (page is null)
        {
            return NotFound();
        }

        if (await _memberManager.MemberHasAccessAsync(page.Path) is false)
        {
            return Forbid();
        }

        MemberIdentityUser? member = await _memberManager.GetCurrentMemberAsync();
        if (member is null)
        {
            return Forbid();
        }

        bool isModerator = await _memberManager.IsMemberAuthorizedAsync(
            allowGroups: new[] { Constants.CommentModeratorsGroupName });

        int? moderatorMemberIntId = await ResolveModeratorMemberIntIdAsync(member, isModerator, cancellationToken);

        CommentSaveResult result = await _commentService.TrySoftDeleteAsync(
            page,
            model.CommentId,
            member.Key,
            isModerator,
            moderatorMemberIntId,
            cancellationToken);

        if (result.Success is false)
        {
            TempData["MemberCommentsError"] = result.ErrorMessage;
        }

        return RedirectToUmbracoPage(page);
    }

    private async Task<int?> ResolveModeratorMemberIntIdAsync(MemberIdentityUser member, bool isModerator, CancellationToken _)
    {
        if (isModerator is false)
        {
            return null;
        }

        IEnumerable<IMember> members = await _memberService.GetByKeysAsync(new[] { member.Key });
        return members.FirstOrDefault()?.Id;
    }

    private IPublishedContent? ResolvePage(int contentId) =>
        _umbracoContextAccessor.GetRequiredUmbracoContext().Content?.GetById(contentId);
}
