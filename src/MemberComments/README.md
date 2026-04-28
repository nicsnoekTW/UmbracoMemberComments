# Member Comments

For client build steps and file watching, see [README.txt](README.txt).

## Page comments (members)

The package adds a **member commenting** feature: comments are stored in the Umbraco database (EF Core), scoped to a content page (`ContentKey`), with **nested replies** under the parent comment (`ParentId`). Only members who can access the page (public access rules) can post; **edit** and **soft delete** are allowed for the **original author** or anyone in the **Comment moderators** member group (exact name).

**Soft delete** sets `DeletedUtc`; the UI shows **`[comment deleted]`** when the author removed it, or **`[comment deleted by moderator]`** when a moderator removed someone else’s comment, plus a **Deleted at …** timestamp (original body remains in the database). **Replies** to a deleted comment are blocked.

When a moderator edits someone else’s comment, the UI shows **Edited by moderator at …** (no moderator name). The row stores the Umbraco member integer id in **`ModeratorId`** for the last moderator edit or moderator-led soft-delete on another member’s comment.

### 1. Reference the package

Your Umbraco web project must reference **MemberComments** (project reference during development, or the **Umbraco.Community.MemberComments** NuGet package when consumed from a feed).

The [MemberComments.TestSite](../MemberComments.TestSite/MemberComments.TestSite.csproj) sample uses a `ProjectReference` to this project.

### 2. Moderator member group

In **Settings → Members → Member groups**, create a group named exactly:

`Comment moderators`

Assign members who should be allowed to **edit or soft-delete any** comment on the site.

### 3. Include the comments partial in a template

In any Razor view where `Model` is (or derives from) `IPublishedContent` for the page being viewed—for example `Views/ContentPage.cshtml` in the test site—add:

```cshtml
<partial name="Partials/MemberComments/Comments" model="Model" />
```

This renders the `MemberComments` view component (list, reply, edit forms) and posts to the surface controller shipped with the package.

### 4. Database schema (EF Core + package migration)

On install/upgrade, Umbraco runs the package migration plan **MemberComments**, which applies pending EF Core migrations for `membercomments_Comment`.

To add a new EF migration during development (from the `src` folder, with `dotnet-ef` installed):

```bash
dotnet ef migrations add YourMigrationName --project MemberComments --startup-project MemberComments.TestSite --context MemberCommentsDbContext
```

The startup project must reference `Microsoft.EntityFrameworkCore.Design` (the test site already does for this solution).

### 5. Rebuild and run

After changing the package or templates, rebuild and run the site so the Razor Class Library views and migrations are picked up.

## Legacy test partial

The placeholder partial `Partials/MemberCommentsTestOnly` remains for packaging checks; use `Partials/MemberComments/Comments` for the real commenting UI.
