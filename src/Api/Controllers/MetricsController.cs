using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Metrics;

namespace Api.Controllers;

/// <summary>
/// Controller for viewing application metrics and request statistics.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MetricsController : ControllerBase
{
    private readonly Meter _meter;
    private readonly ILogger<MetricsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetricsController"/> class.
    /// </summary>
    public MetricsController(ILogger<MetricsController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _meter = new Meter("CleanArchitecture.Api", "1.0.0");
    }

    /// <summary>
    /// Gets information about available metrics endpoints.
    /// </summary>
    /// <returns>Information about metrics endpoints.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(MetricsInfo), StatusCodes.Status200OK)]
    public ActionResult<MetricsInfo> GetMetricsInfo()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        
        return Ok(new MetricsInfo
        {
            PrometheusEndpoint = $"{baseUrl}/metrics",
            Message = "Access the Prometheus metrics endpoint to view detailed metrics. " +
                     "For visualization, use Grafana or other Prometheus-compatible tools."
        });
    }
}

/// <summary>
/// Information about available metrics endpoints.
/// </summary>
public class MetricsInfo
{
    /// <summary>
    /// The Prometheus metrics endpoint URL.
    /// </summary>
    public string PrometheusEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// Additional information about metrics.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

