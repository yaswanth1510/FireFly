namespace FireflyApi.Configuration
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public int AccessTokenExpireMinutes { get; set; } = 60 * 24 * 30; // 30 days
        public string Algorithm { get; set; } = "HS256";
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
    }

    public class DatabaseSettings
    {
        public string Server { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public string GetConnectionString()
        {
            return $"Server={Server};Database={Name};User={User};Password={Password};";
        }
    }

    public class RedisSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
    }

    public class FirstUserSettings
    {
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ApplicationSettings
    {
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectVersion { get; set; } = string.Empty;
        public JwtSettings Jwt { get; set; } = new();
        public DatabaseSettings Database { get; set; } = new();
        public RedisSettings Redis { get; set; } = new();
        public FirstUserSettings FirstUser { get; set; } = new();
    }
}