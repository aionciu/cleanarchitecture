using System.Net;
using System.Text;
using System.Text.Json;
using Application.Exceptions;
using Api.Middleware;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Api.Tests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _loggerMock;
    private readonly Mock<IWebHostEnvironment> _environmentMock;
    private readonly ExceptionHandlingMiddleware _middleware;
    private readonly DefaultHttpContext _httpContext;

    public ExceptionHandlingMiddlewareTests()
    {
        _nextMock = new Mock<RequestDelegate>();
        _loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        _environmentMock = new Mock<IWebHostEnvironment>();
        _httpContext = new DefaultHttpContext();
        _httpContext.Request.Path = "/api/test";
        _httpContext.Response.Body = new MemoryStream();

        _middleware = new ExceptionHandlingMiddleware(
            _nextMock.Object,
            _loggerMock.Object,
            _environmentMock.Object);
    }

    [Fact]
    public async Task InvokeAsync_WhenNoException_ShouldCallNext()
    {
        // Arrange
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _nextMock.Verify(next => next(_httpContext), Times.Once);
        Assert.Equal(200, _httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WhenValidationException_ShouldReturn400WithProblemDetails()
    {
        // Arrange
        var validationException = new ValidationException(new[]
        {
            new ValidationFailure("Amount", "Amount must be greater than 0"),
            new ValidationFailure("Currency", "Currency is required")
        });

        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(validationException);
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Production");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, _httpContext.Response.StatusCode);
        Assert.StartsWith("application/problem+json", _httpContext.Response.ContentType);

        var problemDetails = await DeserializeResponse<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Contains("rfc7231", problemDetails!.Type);
        Assert.Equal("Validation Error", problemDetails.Title);
        Assert.Equal((int)HttpStatusCode.BadRequest, problemDetails.Status);
        Assert.True(problemDetails.Extensions.ContainsKey("errors"));
        Assert.True(problemDetails.Extensions.ContainsKey("correlationId"));
    }

    [Fact]
    public async Task InvokeAsync_WhenNotFoundException_ShouldReturn404WithProblemDetails()
    {
        // Arrange
        var notFoundException = new NotFoundException("Transaction", Guid.NewGuid());
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(notFoundException);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal((int)HttpStatusCode.NotFound, _httpContext.Response.StatusCode);
        Assert.StartsWith("application/problem+json", _httpContext.Response.ContentType);

        var problemDetails = await DeserializeResponse<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Contains("rfc7231", problemDetails!.Type);
        Assert.Equal("Resource Not Found", problemDetails.Title);
        Assert.Equal((int)HttpStatusCode.NotFound, problemDetails.Status);
        Assert.True(problemDetails.Extensions.ContainsKey("correlationId"));
        Assert.True(problemDetails.Extensions.ContainsKey("resourceName"));
        Assert.True(problemDetails.Extensions.ContainsKey("identifier"));
    }

    [Fact]
    public async Task InvokeAsync_WhenDbUpdateConcurrencyException_ShouldReturn409WithProblemDetails()
    {
        // Arrange
        var concurrencyException = new DbUpdateConcurrencyException("Concurrency conflict");
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(concurrencyException);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal((int)HttpStatusCode.Conflict, _httpContext.Response.StatusCode);
        Assert.StartsWith("application/problem+json", _httpContext.Response.ContentType);

        var problemDetails = await DeserializeResponse<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Contains("rfc7231", problemDetails!.Type);
        Assert.Equal("Concurrency Conflict", problemDetails.Title);
        Assert.Equal((int)HttpStatusCode.Conflict, problemDetails.Status);
        Assert.True(problemDetails.Extensions.ContainsKey("correlationId"));
    }

    [Fact]
    public async Task InvokeAsync_WhenArgumentException_ShouldReturn400WithProblemDetails()
    {
        // Arrange
        var argumentException = new ArgumentException("Invalid argument", "amount");
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(argumentException);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, _httpContext.Response.StatusCode);
        Assert.StartsWith("application/problem+json", _httpContext.Response.ContentType);

        var problemDetails = await DeserializeResponse<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal("Bad Request", problemDetails!.Title);
        Assert.Equal((int)HttpStatusCode.BadRequest, problemDetails.Status);
        Assert.True(problemDetails.Extensions.ContainsKey("correlationId"));
        Assert.True(problemDetails.Extensions.ContainsKey("paramName"));
    }

    [Fact]
    public async Task InvokeAsync_WhenArgumentNullException_ShouldReturn400WithProblemDetails()
    {
        // Arrange
        var argumentNullException = new ArgumentNullException("command");
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(argumentNullException);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, _httpContext.Response.StatusCode);
        Assert.StartsWith("application/problem+json", _httpContext.Response.ContentType);

        var problemDetails = await DeserializeResponse<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal("Bad Request", problemDetails!.Title);
        Assert.True(problemDetails.Extensions.ContainsKey("paramName"));
    }

    [Fact]
    public async Task InvokeAsync_WhenDbUpdateException_ShouldReturn500WithProblemDetails()
    {
        // Arrange
        var dbUpdateException = new DbUpdateException("Database error", new Exception("Inner error"));
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(dbUpdateException);
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Production");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal((int)HttpStatusCode.InternalServerError, _httpContext.Response.StatusCode);
        Assert.StartsWith("application/problem+json", _httpContext.Response.ContentType);

        var problemDetails = await DeserializeResponse<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal("Database Error", problemDetails!.Title);
        Assert.Equal((int)HttpStatusCode.InternalServerError, problemDetails.Status);
        Assert.Equal("An error occurred while processing your request.", problemDetails.Detail);
        Assert.True(problemDetails.Extensions.ContainsKey("correlationId"));
        Assert.False(problemDetails.Extensions.ContainsKey("innerException"));
    }

    [Fact]
    public async Task InvokeAsync_WhenDbUpdateExceptionInDevelopment_ShouldIncludeInnerException()
    {
        // Arrange
        var innerException = new Exception("Inner database error");
        var dbUpdateException = new DbUpdateException("Database error", innerException);
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(dbUpdateException);
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Development");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var problemDetails = await DeserializeResponse<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal(innerException.Message, problemDetails!.Detail);
        Assert.True(problemDetails.Extensions.ContainsKey("innerException"));
    }

    [Fact]
    public async Task InvokeAsync_WhenGenericException_ShouldReturn500WithProblemDetails()
    {
        // Arrange
        var genericException = new InvalidOperationException("Something went wrong");
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(genericException);
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Production");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal((int)HttpStatusCode.InternalServerError, _httpContext.Response.StatusCode);
        Assert.StartsWith("application/problem+json", _httpContext.Response.ContentType);

        var problemDetails = await DeserializeResponse<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal("Internal Server Error", problemDetails!.Title);
        Assert.Equal((int)HttpStatusCode.InternalServerError, problemDetails.Status);
        Assert.Equal("An error occurred while processing your request.", problemDetails.Detail);
        Assert.True(problemDetails.Extensions.ContainsKey("correlationId"));
        Assert.False(problemDetails.Extensions.ContainsKey("stackTrace"));
    }

    [Fact]
    public async Task InvokeAsync_WhenGenericExceptionInDevelopment_ShouldIncludeStackTrace()
    {
        // Arrange
        var genericException = new InvalidOperationException("Something went wrong");
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(genericException);
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Development");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var problemDetails = await DeserializeResponse<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal(genericException.Message, problemDetails!.Detail);
        Assert.True(problemDetails.Extensions.ContainsKey("stackTrace"));
        Assert.True(problemDetails.Extensions.ContainsKey("innerException"));
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogException()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(exception);
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Production");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldIncludeCorrelationIdInAllResponses()
    {
        // Arrange
        var exception = new NotFoundException("Test");
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var problemDetails = await DeserializeResponse<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.True(problemDetails!.Extensions.ContainsKey("correlationId"));
        Assert.Equal(_httpContext.TraceIdentifier, problemDetails.Extensions["correlationId"]!.ToString());
    }

    [Fact]
    public async Task InvokeAsync_WhenValidationException_ShouldGroupErrorsByProperty()
    {
        // Arrange
        var validationException = new ValidationException(new[]
        {
            new ValidationFailure("Amount", "Amount must be greater than 0"),
            new ValidationFailure("Amount", "Amount is required"),
            new ValidationFailure("Currency", "Currency is required")
        });

        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(validationException);
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Production");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var problemDetails = await DeserializeResponse<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.True(problemDetails!.Extensions.ContainsKey("errors"));

        // Deserialize errors from JSON
        var errorsJson = JsonSerializer.Serialize(problemDetails.Extensions["errors"]);
        var errors = JsonSerializer.Deserialize<Dictionary<string, string[]>>(errorsJson);
        Assert.NotNull(errors);
        Assert.True(errors!.ContainsKey("Amount"));
        Assert.Equal(2, errors["Amount"].Length);
        Assert.True(errors.ContainsKey("Currency"));
        Assert.Single(errors["Currency"]);
    }

    private async Task<T?> DeserializeResponse<T>()
    {
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        return JsonSerializer.Deserialize<T>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}
