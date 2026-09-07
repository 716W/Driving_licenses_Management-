using System;
using System.IO;

namespace DVLD_DataAccess
{
    /// <summary>
    /// Provides the SQL Server connection string for the DVLD application.
    ///
    /// Resolution order:
    ///   1. Environment variable  DVLD_CONNECTION_STRING
    ///   2. Local file            dvld.env  (key=value, key must be DVLD_CONNECTION_STRING)
    ///      The file is excluded from source control via .gitignore.
    ///      Copy dvld.env.example to dvld.env and fill in your real credentials.
    ///   3. If neither source is present, the application throws at startup so the
    ///      missing configuration is immediately visible — no silent connection failures.
    ///
    /// IMPORTANT: Never commit real credentials. dvld.env is in .gitignore.
    /// </summary>
    static class clsDataAccessSettings
    {
        private static string? _connectionString;

        public static string ConnectionString
        {
            get
            {
                if (_connectionString != null)
                    return _connectionString;

                _connectionString = ResolveConnectionString();
                return _connectionString;
            }
        }

        private static string ResolveConnectionString()
        {
            // 1. Try environment variable
            string? fromEnv = Environment.GetEnvironmentVariable("DVLD_CONNECTION_STRING");
            if (!string.IsNullOrWhiteSpace(fromEnv))
                return fromEnv.Trim();

            // 2. Try dvld.env file next to the executable
            string envFilePath = Path.Combine(AppContext.BaseDirectory, "dvld.env");

            if (!File.Exists(envFilePath))
            {
                // Also try the current working directory (useful during development / F5)
                envFilePath = Path.Combine(Directory.GetCurrentDirectory(), "dvld.env");
            }

            if (File.Exists(envFilePath))
            {
                foreach (string rawLine in File.ReadAllLines(envFilePath))
                {
                    string line = rawLine.Trim();
                    if (line.StartsWith("#") || !line.Contains('='))
                        continue;

                    int idx = line.IndexOf('=');
                    string key   = line[..idx].Trim();
                    string value = line[(idx + 1)..].Trim();

                    if (string.Equals(key, "DVLD_CONNECTION_STRING", StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrWhiteSpace(value))
                    {
                        return value;
                    }
                }
            }

            // 3. Neither source is available — fail fast and clearly
            throw new InvalidOperationException(
                "DVLD database connection string is not configured. " +
                "Set the 'DVLD_CONNECTION_STRING' environment variable, " +
                "or create a 'dvld.env' file next to the executable with the line:\n" +
                "  DVLD_CONNECTION_STRING=Server=<host>;Database=DVLD;User Id=<user>;Password=<pass>;TrustServerCertificate=True;\n" +
                "See dvld.env.example for a template. Never commit real credentials.");
        }

        /// <summary>
        /// Returns the active connection string. Preserved for any callers that used
        /// the old GetConnectionString() method name.
        /// </summary>
        public static string GetConnectionString() => ConnectionString;
    }
}
