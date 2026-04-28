# Member Comments

For client build steps and file watching, see [README.txt](README.txt).

## Using the partial from this package in your templates

The package ships a Razor partial view you can embed in any front-office template in your Umbraco site.

### 1. Reference the package

Your Umbraco web project must reference **MemberComments** (project reference during development, or the **Umbraco.Community.MemberComments** NuGet package when consumed from a feed).

The [MemberComments.TestSite](../MemberComments.TestSite/MemberComments.TestSite.csproj) sample uses a `ProjectReference` to this project.

### 2. Include the partial in a view

In any Razor view under `Views/` (for example `Views/Contact.cshtml` in the test site), add the partial tag helper where you want the markup to appear:

```cshtml
<partial name="Partials/MemberCommentsTestOnly" />
```

You can place it inside a layout section, a `<section>`, a column, or any other markup; only the partial’s output is fixed.

### 3. Rebuild and run

After changing the plugin or adding the line above, rebuild the solution and run the site so the Razor Class Library’s compiled views are loaded.
