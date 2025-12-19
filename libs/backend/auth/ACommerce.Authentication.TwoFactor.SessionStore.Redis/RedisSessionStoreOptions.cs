namespace ACommerce.Authentication.TwoFactor.SessionStore.Redis;

public class RedisSessionStoreOptions
{
    public const string SectionName = "Redis:TwoFactorSessionStore";

    public string ConnectionString { get; set; } = "localhost:6379";

    public string KeyPrefix { get; set; } = "2fa:session:";

    public int DefaultExpirationMinutes { get; set; } = 10;
}
