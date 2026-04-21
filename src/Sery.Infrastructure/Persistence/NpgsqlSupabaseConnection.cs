using System.Text.RegularExpressions;

namespace Sery.Infrastructure.Persistence;

internal static class NpgsqlSupabaseConnection
{
    private static readonly Regex s_transactionPoolerPort =
        new(@"\bPort\s*=\s*6543\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static string ApplyPoolerDefaults(string connectionString)
    {
        if (connectionString.Contains("No Reset On Close", StringComparison.OrdinalIgnoreCase))
        {
            return connectionString;
        }

        bool supabasePooler = connectionString.Contains("pooler.supabase.com", StringComparison.OrdinalIgnoreCase);
        bool transactionPort = s_transactionPoolerPort.IsMatch(connectionString)
            || connectionString.Contains(":6543/", StringComparison.OrdinalIgnoreCase);

        if (supabasePooler && transactionPort)
        {
            return connectionString.Trim().TrimEnd(';') + ";No Reset On Close=true";
        }

        return connectionString;
    }
}
