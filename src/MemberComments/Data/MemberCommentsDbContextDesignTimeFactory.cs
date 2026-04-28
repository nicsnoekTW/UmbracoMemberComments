using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MemberComments.Data;

/// <summary>Design-time factory for <c>dotnet ef migrations</c> (startup project: MemberComments.TestSite).</summary>
public sealed class MemberCommentsDbContextDesignTimeFactory : IDesignTimeDbContextFactory<MemberCommentsDbContext>
{
    public MemberCommentsDbContext CreateDbContext(string[] args)
    {
        string baseDir = Directory.GetCurrentDirectory();
        string? testSite = LocateTestSiteDirectory(baseDir);
        if (testSite is null)
        {
            throw new InvalidOperationException(
                "Could not find MemberComments.TestSite for design-time migrations. Run dotnet ef from the solution src folder.");
        }

        IConfigurationRoot config = new ConfigurationBuilder()
            .SetBasePath(testSite)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        string? connectionString = config.GetConnectionString("umbracoDbDSN");
        string? providerName = config["ConnectionStrings:umbracoDbDSN_ProviderName"];

        var optionsBuilder = new DbContextOptionsBuilder<MemberCommentsDbContext>();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string umbracoDbDSN is missing.");
        }

        // Match Umbraco's |DataDirectory| resolution for local SQLite paths
        if (connectionString.Contains("|DataDirectory|", StringComparison.OrdinalIgnoreCase))
        {
            string dataDir = Path.Combine(testSite, "App_Data");
            Directory.CreateDirectory(dataDir);
            connectionString = connectionString.Replace("|DataDirectory|", dataDir, StringComparison.OrdinalIgnoreCase);
        }

        if (providerName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true)
        {
            optionsBuilder.UseSqlite(connectionString);
        }
        else
        {
            optionsBuilder.UseSqlServer(connectionString);
        }

        return new MemberCommentsDbContext(optionsBuilder.Options);
    }

    private static string? LocateTestSiteDirectory(string start)
    {
        for (var dir = new DirectoryInfo(start); dir is not null; dir = dir.Parent)
        {
            string candidate = Path.Combine(dir.FullName, "MemberComments.TestSite");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }
}
