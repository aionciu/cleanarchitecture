namespace Api.Configuration;

/// <summary>
/// Configuration for Kestrel web server.
/// </summary>
public static class KestrelConfiguration
{
    private const int MaxRequestBodySizeBytes = 1048576; // 1MB

    /// <summary>
    /// Configures Kestrel server limits.
    /// </summary>
    public static IWebHostBuilder ConfigureKestrelLimits(this IWebHostBuilder builder)
    {
        builder.ConfigureKestrel(options =>
        {
            options.Limits.MaxRequestBodySize = MaxRequestBodySizeBytes;
        });

        return builder;
    }
}

