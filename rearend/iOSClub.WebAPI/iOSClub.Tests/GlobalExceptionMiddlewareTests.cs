using System.Net;
using System.Net.Http.Json;
using FluentValidation;
using iOSClub.Data.DataModels;
using iOSClub.WebAPI.Common;
using iOSClub.WebAPI.Common.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace iOSClub.Tests;

public class GlobalExceptionMiddlewareTests
{
    // 测试控制器，用于抛出各种异常
    private class TestExceptionController : ControllerBase
    {
        [HttpGet("/throw-argument-null")]
        public IActionResult ThrowArgumentNullException()
        {
            throw new ArgumentNullException("testParam");
        }

        [HttpGet("/throw-argument")]
        public IActionResult ThrowArgumentException()
        {
            throw new ArgumentException("Invalid argument", "testParam");
        }

        [HttpGet("/throw-key-not-found")]
        public IActionResult ThrowKeyNotFoundException()
        {
            throw new KeyNotFoundException("Key not found");
        }

        [HttpGet("/throw-business-exception")]
        public IActionResult ThrowBusinessException()
        {
            throw new BusinessException(ErrorCode.OperationFailed, "Business operation failed");
        }

        [HttpGet("/throw-auth-exception")]
        public IActionResult ThrowAuthException()
        {
            throw new AuthException(ErrorCode.Unauthorized, "Authentication failed");
        }

        [HttpGet("/throw-validation-exception")]
        public IActionResult ThrowValidationException()
        {
            // 直接抛出自定义验证异常
            throw new iOSClub.WebAPI.Common.Exceptions.ValidationException(
                ErrorCode.ParameterValidationFailed,
                "请求参数验证失败",
                new Dictionary<string, string[]> {
                    { "Name", new[] { "Name is required" } },
                    { "Age", new[] { "Age must be greater than 0" } }
                }
            );
        }

        [HttpGet("/throw-data-access-exception")]
        public IActionResult ThrowDataAccessException()
        {
            throw new DataAccessException(
                ErrorCode.DatabaseOperationFailed,
                "Database operation failed",
                "Insert",
                "Users",
                "Failed to insert user record");
        }

        [HttpGet("/throw-generic-exception")]
        public IActionResult ThrowGenericException()
        {
            throw new Exception("Generic exception occurred");
        }

        [HttpGet("/success")]
        public IActionResult Success()
        {
            return Ok("Success");
        }
    }

    // 创建测试服务器
    private async Task<TestServer> CreateTestServer()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        // 配置服务
        builder.Services.AddControllers()
            .AddApplicationPart(typeof(TestExceptionController).Assembly);

        // 构建应用
        var app = builder.Build();

        // 注册全局异常处理中间件
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapControllers();

        await app.StartAsync();
        return app.GetTestServer();
    }

    // 测试参数异常处理
    [Fact]
    public async Task HandleArgumentNullException_ReturnsBadRequest()
    {
        // Arrange
        var testServer = await CreateTestServer();
        var client = testServer.CreateClient();

        // Act
        var response = await client.GetAsync("/throw-argument-null");
        var responseBody = await response.Content.ReadFromJsonAsync<ApiResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal((int)HttpStatusCode.BadRequest, responseBody?.Code);
        Assert.Equal(ErrorCode.ParameterEmpty, responseBody?.ErrorCode);
        Assert.Contains("testParam", responseBody?.Message ?? string.Empty);
        Assert.NotNull(responseBody?.RequestId);
        Assert.NotNull(responseBody?.Timestamp);
    }

    // 测试参数异常处理
    [Fact]
    public async Task HandleArgumentException_ReturnsBadRequest()
    {
        // Arrange
        var testServer = await CreateTestServer();
        var client = testServer.CreateClient();

        // Act
        var response = await client.GetAsync("/throw-argument");
        var responseBody = await response.Content.ReadFromJsonAsync<ApiResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal((int)HttpStatusCode.BadRequest, responseBody?.Code);
        Assert.Equal(ErrorCode.ParameterFormatError, responseBody?.ErrorCode);
        Assert.Contains("testParam", responseBody?.Message ?? string.Empty);
        Assert.NotNull(responseBody?.RequestId);
        Assert.NotNull(responseBody?.Timestamp);
    }

    // 测试资源不存在异常处理
    [Fact]
    public async Task HandleKeyNotFoundException_ReturnsNotFound()
    {
        // Arrange
        var testServer = await CreateTestServer();
        var client = testServer.CreateClient();

        // Act
        var response = await client.GetAsync("/throw-key-not-found");
        var responseBody = await response.Content.ReadFromJsonAsync<ApiResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal((int)HttpStatusCode.NotFound, responseBody?.Code);
        Assert.Equal(ErrorCode.ResourceNotFound, responseBody?.ErrorCode);
        Assert.NotNull(responseBody?.RequestId);
        Assert.NotNull(responseBody?.Timestamp);
    }

    // 测试业务异常处理
    [Fact]
    public async Task HandleBusinessException_ReturnsBadRequest()
    {
        // Arrange
        var testServer = await CreateTestServer();
        var client = testServer.CreateClient();

        // Act
        var response = await client.GetAsync("/throw-business-exception");
        var responseBody = await response.Content.ReadFromJsonAsync<ApiResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal((int)HttpStatusCode.BadRequest, responseBody?.Code);
        Assert.Equal(ErrorCode.OperationFailed, responseBody?.ErrorCode);
        Assert.NotNull(responseBody?.RequestId);
        Assert.NotNull(responseBody?.Timestamp);
    }

    // 测试认证异常处理
    [Fact]
    public async Task HandleAuthException_ReturnsUnauthorized()
    {
        // Arrange
        var testServer = await CreateTestServer();
        var client = testServer.CreateClient();

        // Act
        var response = await client.GetAsync("/throw-auth-exception");
        var responseBody = await response.Content.ReadFromJsonAsync<ApiResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal((int)HttpStatusCode.Unauthorized, responseBody?.Code);
        Assert.Equal(ErrorCode.Unauthorized, responseBody?.ErrorCode);
        Assert.NotNull(responseBody?.RequestId);
        Assert.NotNull(responseBody?.Timestamp);
    }

    // 测试验证异常处理
    [Fact]
    public async Task HandleValidationException_ReturnsBadRequest()
    {
        // Arrange
        var testServer = await CreateTestServer();
        var client = testServer.CreateClient();

        // Act
        var response = await client.GetAsync("/throw-validation-exception");
        var responseBody = await response.Content.ReadFromJsonAsync<ApiResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal((int)HttpStatusCode.BadRequest, responseBody?.Code);
        Assert.Equal(ErrorCode.ParameterValidationFailed, responseBody?.ErrorCode);
        Assert.Contains("Name", responseBody?.Detail ?? string.Empty);
        Assert.Contains("Age", responseBody?.Detail ?? string.Empty);
        Assert.NotNull(responseBody?.RequestId);
        Assert.NotNull(responseBody?.Timestamp);
    }

    // 测试数据访问异常处理
    [Fact]
    public async Task HandleDataAccessException_ReturnsInternalServerError()
    {
        // Arrange
        var testServer = await CreateTestServer();
        var client = testServer.CreateClient();

        // Act
        var response = await client.GetAsync("/throw-data-access-exception");
        var responseBody = await response.Content.ReadFromJsonAsync<ApiResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal((int)HttpStatusCode.InternalServerError, responseBody?.Code);
        Assert.Equal(ErrorCode.DatabaseOperationFailed, responseBody?.ErrorCode);
        Assert.NotNull(responseBody?.RequestId);
        Assert.NotNull(responseBody?.Timestamp);
    }

    // 测试通用异常处理
    [Fact]
    public async Task HandleGenericException_ReturnsInternalServerError()
    {
        // Arrange
        var testServer = await CreateTestServer();
        var client = testServer.CreateClient();

        // Act
        var response = await client.GetAsync("/throw-generic-exception");
        var responseBody = await response.Content.ReadFromJsonAsync<ApiResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal((int)HttpStatusCode.InternalServerError, responseBody?.Code);
        Assert.Equal(ErrorCode.InternalServerError, responseBody?.ErrorCode);
        Assert.NotNull(responseBody?.RequestId);
        Assert.NotNull(responseBody?.Timestamp);
    }

    // 测试成功响应
    [Fact]
    public async Task HandleSuccessRequest_ReturnsSuccess()
    {
        // Arrange
        var testServer = await CreateTestServer();
        var client = testServer.CreateClient();

        // Act
        var response = await client.GetAsync("/success");
        var responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Success", responseBody);
    }

    // 测试请求ID是否正确添加到响应头
    [Fact]
    public async Task RequestId_IsAddedToResponseHeaders()
    {
        // Arrange
        var testServer = await CreateTestServer();
        var client = testServer.CreateClient();

        // Act
        var response = await client.GetAsync("/throw-generic-exception");

        // Assert
        Assert.True(response.Headers.TryGetValues("X-Request-ID", out var requestIdValues));
        var requestId = requestIdValues?.FirstOrDefault();
        Assert.NotNull(requestId);

        // 验证响应体中也包含相同的请求ID
        var responseBody = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.Equal(requestId, responseBody?.RequestId);
    }

    // 测试生产环境下不返回详细异常信息
    [Fact]
    public async Task ProductionEnvironment_DoesNotReturnDetailedExceptionInfo()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.WebHost.UseEnvironment(Environments.Production);

        // 配置服务
        builder.Services.AddControllers()
            .AddApplicationPart(typeof(TestExceptionController).Assembly);

        // 构建应用
        var app = builder.Build();
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapControllers();

        await app.StartAsync();
        var client = app.GetTestServer().CreateClient();

        // Act
        var response = await client.GetAsync("/throw-generic-exception");
        var responseBody = await response.Content.ReadFromJsonAsync<ApiResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(ErrorCode.InternalServerError, responseBody?.ErrorCode);
        Assert.Null(responseBody?.Detail); // 生产环境下不返回详细异常信息
        Assert.Equal("服务器内部错误", responseBody?.Message); // 生产环境下返回通用错误消息
    }
}