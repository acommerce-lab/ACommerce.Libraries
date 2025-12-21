namespace ACommerce.Marketplace.GCP.Configuration;

/// <summary>
/// Database provider types supported by the application
/// </summary>
public enum DatabaseProvider
{
    SQLite,
    PostgreSQL,
    SqlServer
}

/// <summary>
/// Database configuration resolved from environment variables.
/// Supports Cloud SQL (PostgreSQL/SQL Server) and local SQLite.
///
/// Environment Variables (in order of precedence):
/// - DATABASE_URL: Full connection string (Heroku/Railway style)
/// - DATABASE_CONNECTION_STRING: Direct connection string
/// - GOOGLE_SQL_CONNECTION_STRING: Legacy GCP connection string
/// - Individual components: DB_HOST, DB_PORT, DB_NAME, DB_USER, DB_PASSWORD
///
/// Provider Selection:
/// - DATABASE_PROVIDER: "postgresql", "sqlserver", or "sqlite"
/// - Auto-detected from connection string if not specified
/// </summary>
public class DatabaseConfiguration
{
    public DatabaseProvider Provider { get; set; }
    public string ConnectionString { get; set; } = string.Empty;
    public string? UnixSocketPath { get; set; }
    public bool IsCloudSql { get; set; }

    /// <summary>
    /// Creates database configuration from environment variables
    /// </summary>
    public static DatabaseConfiguration FromEnvironment()
    {
        var config = new DatabaseConfiguration();

        // 1. Try to get provider from environment
        var providerEnv = Environment.GetEnvironmentVariable("DATABASE_PROVIDER")?.ToLowerInvariant();

        // 2. Try to get connection string from various sources
        var connectionString =
            Environment.GetEnvironmentVariable("DATABASE_URL") ??
            Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING") ??
            Environment.GetEnvironmentVariable("GOOGLE_SQL_CONNECTION_STRING");

        // 3. If no connection string, try to build from components
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = BuildConnectionStringFromComponents(providerEnv, out var detectedProvider);
            if (string.IsNullOrEmpty(providerEnv))
            {
                providerEnv = detectedProvider;
            }
        }

        // 4. Detect provider from connection string if not specified
        if (string.IsNullOrEmpty(providerEnv))
        {
            providerEnv = DetectProviderFromConnectionString(connectionString);
        }

        // 5. Set provider
        config.Provider = providerEnv switch
        {
            "postgresql" or "postgres" or "npgsql" => DatabaseProvider.PostgreSQL,
            "sqlserver" or "mssql" => DatabaseProvider.SqlServer,
            _ => DatabaseProvider.SQLite
        };

        // 6. Handle Cloud SQL Unix socket connection (GCP)
        var instanceConnectionName = Environment.GetEnvironmentVariable("CLOUD_SQL_CONNECTION_NAME");
        if (!string.IsNullOrEmpty(instanceConnectionName) && config.Provider == DatabaseProvider.PostgreSQL)
        {
            config.IsCloudSql = true;
            config.UnixSocketPath = $"/cloudsql/{instanceConnectionName}";

            // Build Cloud SQL connection string with Unix socket
            var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "acommerce";
            var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
            var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";

            config.ConnectionString = $"Host={config.UnixSocketPath};Database={dbName};Username={dbUser};Password={dbPassword}";
        }
        else if (string.IsNullOrEmpty(connectionString))
        {
            // Default to SQLite for local development
            config.ConnectionString = "Data Source=acommerce.db";
            config.Provider = DatabaseProvider.SQLite;
        }
        else
        {
            // Convert DATABASE_URL format if needed (Heroku/Railway style)
            config.ConnectionString = ConvertDatabaseUrl(connectionString, config.Provider);
        }

        return config;
    }

    /// <summary>
    /// Builds connection string from individual environment variables
    /// </summary>
    private static string? BuildConnectionStringFromComponents(string? provider, out string? detectedProvider)
    {
        detectedProvider = provider;

        var host = Environment.GetEnvironmentVariable("DB_HOST");
        var port = Environment.GetEnvironmentVariable("DB_PORT");
        var name = Environment.GetEnvironmentVariable("DB_NAME");
        var user = Environment.GetEnvironmentVariable("DB_USER");
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD");

        if (string.IsNullOrEmpty(host))
            return null;

        // Default values
        name ??= "acommerce";
        user ??= "postgres";

        switch (provider?.ToLowerInvariant())
        {
            case "postgresql" or "postgres":
                detectedProvider = "postgresql";
                port ??= "5432";
                return $"Host={host};Port={port};Database={name};Username={user};Password={password}";

            case "sqlserver" or "mssql":
                detectedProvider = "sqlserver";
                port ??= "1433";
                return $"Server={host},{port};Database={name};User Id={user};Password={password};TrustServerCertificate=True";

            default:
                // Try to detect from port
                if (port == "5432" || string.IsNullOrEmpty(port))
                {
                    detectedProvider = "postgresql";
                    port ??= "5432";
                    return $"Host={host};Port={port};Database={name};Username={user};Password={password}";
                }
                else if (port == "1433")
                {
                    detectedProvider = "sqlserver";
                    return $"Server={host},{port};Database={name};User Id={user};Password={password};TrustServerCertificate=True";
                }
                return null;
        }
    }

    /// <summary>
    /// Detects database provider from connection string format
    /// </summary>
    private static string DetectProviderFromConnectionString(string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return "sqlite";

        var lower = connectionString.ToLowerInvariant();

        // Check for PostgreSQL indicators
        if (lower.StartsWith("postgres://") ||
            lower.StartsWith("postgresql://") ||
            lower.Contains("host=") && lower.Contains("username=") ||
            lower.Contains("npgsql"))
        {
            return "postgresql";
        }

        // Check for SQL Server indicators
        if (lower.Contains("server=") && lower.Contains("user id=") ||
            lower.Contains("data source=") && !lower.EndsWith(".db") ||
            lower.Contains("sqlserver") ||
            lower.Contains("mssql"))
        {
            return "sqlserver";
        }

        // Check for SQLite
        if (lower.Contains("data source=") && lower.EndsWith(".db") ||
            lower.Contains("sqlite"))
        {
            return "sqlite";
        }

        return "sqlite";
    }

    /// <summary>
    /// Converts DATABASE_URL format (postgres://user:pass@host:port/db) to standard connection string
    /// </summary>
    private static string ConvertDatabaseUrl(string url, DatabaseProvider provider)
    {
        // Check if it's already a standard connection string
        if (!url.Contains("://"))
            return url;

        try
        {
            var uri = new Uri(url);
            var userInfo = uri.UserInfo.Split(':');
            var user = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : "";
            var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
            var host = uri.Host;
            var port = uri.Port > 0 ? uri.Port : (provider == DatabaseProvider.PostgreSQL ? 5432 : 1433);
            var database = uri.AbsolutePath.TrimStart('/');

            return provider switch
            {
                DatabaseProvider.PostgreSQL =>
                    $"Host={host};Port={port};Database={database};Username={user};Password={password};SSL Mode=Require;Trust Server Certificate=true",
                DatabaseProvider.SqlServer =>
                    $"Server={host},{port};Database={database};User Id={user};Password={password};TrustServerCertificate=True",
                _ => url
            };
        }
        catch
        {
            // Return as-is if parsing fails
            return url;
        }
    }
}
