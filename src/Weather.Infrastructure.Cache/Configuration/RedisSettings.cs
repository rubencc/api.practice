namespace Weather.Infrastructure.Cache.Configuration;

public class RedisSettings
{
    public const string SectionName = "Redis";
    
    public string ConnectionString { get; set; }
    public int DefaultExpirationMinutes { get; set; } = 60;
    public bool Enabled { get; set; } = true;
    public int ConnectTimeout { get; set; } = 5000;
    public int SyncTimeout { get; set; } = 5000;
    public bool AbortOnConnectFail { get; set; } = false;
}