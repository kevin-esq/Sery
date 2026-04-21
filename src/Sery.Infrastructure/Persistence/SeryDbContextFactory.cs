using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Sery.Infrastructure.Persistence;

public sealed class SeryDbContextFactory : IDesignTimeDbContextFactory<SeryDbContext>
{
    public SeryDbContext CreateDbContext(string[] args)
    {
        TryLoadEnvFile();

        IConfigurationRoot configuration = BuildConfiguration();
        string? connectionString = ResolveDesignTimeConnectionString(configuration);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Design-time connection missing. Set CONNECTIONSTRINGS__MIGRATIONCONNECTION (Direct db.<ref>.supabase.co:5432 or Session pooler :5432), " +
                "or ConnectionStrings:MigrationConnection in appsettings. Transaction pooler :6543 is for app runtime only, not for dotnet ef database update. " +
                "Aliases: CONNECTIONSTRINGS__DESIGNTIME; fallback: CONNECTIONSTRINGS__DEFAULTCONNECTION.");
        }

        connectionString = NpgsqlSupabaseConnection.ApplyPoolerDefaults(connectionString);

        var optionsBuilder = new DbContextOptionsBuilder<SeryDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new SeryDbContext(optionsBuilder.Options);
    }

    private static string? ResolveDesignTimeConnectionString(IConfiguration configuration)
    {
        string? fromEnv =
            Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__MIGRATIONCONNECTION")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__MigrationConnection")
            ?? Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__DESIGNTIME");

        if (!string.IsNullOrWhiteSpace(fromEnv))
        {
            return fromEnv;
        }

        string? fromFile = configuration.GetConnectionString("MigrationConnection");
        if (!string.IsNullOrWhiteSpace(fromFile))
        {
            return fromFile;
        }

        return Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__DEFAULTCONNECTION")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? configuration.GetConnectionString("DefaultConnection");
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        string? basePath = FindApiProjectDirectory();
        IConfigurationBuilder builder = new ConfigurationBuilder();
        if (basePath is not null)
        {
            builder.SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false);
        }

        return builder.AddEnvironmentVariables().Build();
    }

    private static string? FindApiProjectDirectory()
    {
        string? dir = Directory.GetCurrentDirectory();
        for (int i = 0; i < 14 && dir is not null; i++)
        {
            if (File.Exists(Path.Combine(dir, "Sery.API.csproj")))
            {
                return dir;
            }

            string nested = Path.Combine(dir, "src", "Sery.API", "Sery.API.csproj");
            if (File.Exists(nested))
            {
                return Path.GetDirectoryName(nested)!;
            }

            dir = Directory.GetParent(dir)?.FullName;
        }

        return null;
    }

    private static void TryLoadEnvFile()
    {
        string? dir = Directory.GetCurrentDirectory();
        for (int i = 0; i < 10 && dir is not null; i++)
        {
            string path = Path.Combine(dir, ".env");
            if (File.Exists(path))
            {
                Env.Load(path);
                return;
            }

            dir = Directory.GetParent(dir)?.FullName;
        }
    }
}
