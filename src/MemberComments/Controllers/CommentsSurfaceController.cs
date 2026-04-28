using MemberComments.Models;
using MemberComments.Services;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
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
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;

    public CommentsSurfaceController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        ICommentService commentService,
        IMemberManager memberManager)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _commentService = commentService;
        _memberManager = memberManager;
        _umbracoContextAccessor = umbracoContextAccessor;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(Prefix = "")] CreateCommentForm model, CancellationToken cancellationToken)
    {
        IPublishedContent? page = ResolvePage(model.ContentKey);
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

        string authorName = string.IsNullOrWhiteSpace(member.Name) ? member.UserName ?? "Member" : member.Name;
        CommentSaveResult result = await _commentService.TryCreateAsync(
            page,
            model.ParentId,
            member.Key,
            authorName,
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
        IPublishedContent? page = ResolvePage(model.ContentKey);
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

        CommentSaveResult result = await _commentService.TryUpdateAsync(
            page,
            model.CommentId,
            member.Key,
            isModerator,
            model.Text ?? string.Empty,
            cancellationToken);

        if (result.Success is false)
        {
            TempData["MemberCommentsError"] = result.ErrorMessage;
        }

        return RedirectToUmbracoPage(page);
    }

    private IPublishedContent? ResolvePage(Guid contentKey) =>
        _umbracoContextAccessor.GetRequiredUmbracoContext().Content?.GetById(contentKey);
}
