namespace RobotCodingPlatform.Web.Extensions;

public sealed class OpenSearchOptions
{
    public const string SectionName = "OpenSearch";

    public bool Enabled { get; set; }

    public string[] NodeUris { get; set; } = [];

    public string? Username { get; set; }

    public string? Password { get; set; }

    /// <summary>Разрешить самоподписанные сертификаты (только для локальной разработки).</summary>
    public bool AllowInvalidCertificate { get; set; }

    public string IndexFormat { get; set; } = "rcp-web-logs-{0:yyyy.MM.dd}";

    public string StatisticsIndex { get; set; } = "rcp-statistics";

    public bool AutoRegisterTemplate { get; set; } = true;

    public bool DetectOpenSearchVersion { get; set; } = true;

    public int NumberOfShards { get; set; } = 1;

    public int NumberOfReplicas { get; set; }

    public int BatchPostingLimit { get; set; } = 50;

    public int PeriodSeconds { get; set; } = 2;

    public int QueueSizeLimit { get; set; } = 100_000;

    public bool InlineFields { get; set; } = true;

    public int ConnectionTimeoutSeconds { get; set; } = 5;

    public int RequestTimeoutSeconds { get; set; } = 10;
}
