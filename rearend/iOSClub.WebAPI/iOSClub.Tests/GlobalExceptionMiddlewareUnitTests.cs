using iOSClub.WebAPI.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using iOSClub.DataApi.Exceptions;

namespace iOSClub.Tests;

public class GlobalExceptionMiddlewareUnitTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange
        var nextMock = new Mock<RequestDelegate>();
        var loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();

        // Act
        var middleware = new GlobalExceptionMiddleware(nextMock.Object, loggerMock.Object);

        // Assert
        Assert.NotNull(middleware);
    }

    [Fact]
    public void ApiResponse_Success_CreatesCorrectResponse()
    {
        // Act
        var response = ApiResponse.Success("Success message");
        response.Data = "test data";

        // Assert
        Assert.Equal(200, response.Code);
        Assert.Equal(0, response.ErrorCode);
        Assert.Equal("Success message", response.Message);
        Assert.Equal("test data", response.Data);
        Assert.NotNull(response.Timestamp);
    }

    [Fact]
    public void ApiResponse_Fail_CreatesCorrectResponse()
    {
        // Act
        var response = ApiResponse.Fail(ErrorCode.OperationFailed, "Failed message");

        // Assert
        Assert.Equal(400, response.Code);
        Assert.Equal(ErrorCode.OperationFailed, response.ErrorCode);
        Assert.Equal("Failed message", response.Message);
        Assert.NotNull(response.Timestamp);
    }

    [Fact]
    public void CustomException_CreatesCorrectException()
    {
        // Act
        var exception = new CustomException(ErrorCode.OperationFailed, "Custom error message", 400, "Detailed error");

        // Assert
        Assert.Equal(ErrorCode.OperationFailed, exception.ErrorCode);
        Assert.Equal(400, exception.HttpStatusCode);
        Assert.Equal("Custom error message", exception.Message);
        Assert.Equal("Detailed error", exception.Detail);
    }

    [Fact]
    public void BusinessException_CreatesCorrectException()
    {
        // Act
        var exception = new BusinessException(ErrorCode.ResourceAlreadyExists, "Resource already exists");

        // Assert
        Assert.Equal(ErrorCode.ResourceAlreadyExists, exception.ErrorCode);
        Assert.Equal(400, exception.HttpStatusCode);
        Assert.Equal("Resource already exists", exception.Message);
    }

    [Fact]
    public void AuthException_CreatesCorrectException()
    {
        // Act
        var exception = new AuthException(ErrorCode.Unauthorized, "Authentication failed", 401, "Bearer");

        // Assert
        Assert.Equal(ErrorCode.Unauthorized, exception.ErrorCode);
        Assert.Equal(401, exception.HttpStatusCode);
        Assert.Equal("Authentication failed", exception.Message);
        Assert.Equal("Bearer", exception.AuthType);
    }

    [Fact]
    public void ResourceAccessException_CreatesCorrectException()
    {
        // Act
        var exception = new ResourceAccessException(ErrorCode.ResourceNotFound, "Resource not found", "User", "123");

        // Assert
        Assert.Equal(ErrorCode.ResourceNotFound, exception.ErrorCode);
        Assert.Equal(404, exception.HttpStatusCode);
        Assert.Equal("Resource not found", exception.Message);
        Assert.Equal("User", exception.ResourceName);
        Assert.Equal("123", exception.ResourceId);
    }

    [Fact]
    public void ValidationException_CreatesCorrectException()
    {
        // Act
        var validationErrors = new Dictionary<string, string[]>
        {
            { "Name", new[] { "Name is required" } },
            { "Age", new[] { "Age must be greater than 0" } }
        };
        var exception =
            new ValidationException(ErrorCode.ParameterValidationFailed, "Validation failed", validationErrors);

        // Assert
        Assert.Equal(ErrorCode.ParameterValidationFailed, exception.ErrorCode);
        Assert.Equal(400, exception.HttpStatusCode);
        Assert.Equal("Validation failed", exception.Message);
        Assert.Equal(validationErrors, exception.ValidationErrors);
    }

    [Fact]
    public void DataAccessException_CreatesCorrectException()
    {
        // Act
        var exception = new DataAccessException(
            ErrorCode.DatabaseOperationFailed,
            "Database operation failed",
            "Insert",
            "Users",
            "Failed to insert user record");

        // Assert
        Assert.Equal(ErrorCode.DatabaseOperationFailed, exception.ErrorCode);
        Assert.Equal(500, exception.HttpStatusCode);
        Assert.Equal("Database operation failed", exception.Message);
        Assert.Equal("Insert", exception.OperationType);
        Assert.Equal("Users", exception.TableName);
        Assert.Equal("Failed to insert user record", exception.Detail);
    }

    [Fact]
    public void ApiResponse_SerializesToJsonCorrectly()
    {
        // Arrange
        var response = new ApiResponse
        {
            Code = 200,
            ErrorCode = 0,
            Message = "Success",
            Data = new { Name = "Test", Value = 123 },
            RequestId = "test-request-id",
            Timestamp = "2025-01-01T00:00:00.000Z"
        };

        // Act
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        // Assert
        Assert.Contains("test-request-id", json);
        Assert.Contains("2025-01-01T00:00:00.000Z", json);
        Assert.Contains("Success", json);
        Assert.Contains("Test", json);
        Assert.Contains("123", json);
    }

    [Fact]
    public void ErrorCode_HasCorrectValues()
    {
        // Assert
        Assert.Equal(0, ErrorCode.Success);
        Assert.Equal(1000, ErrorCode.ParameterEmpty);
        Assert.Equal(1003, ErrorCode.ParameterValidationFailed);
        Assert.Equal(2000, ErrorCode.ResourceAlreadyExists);
        Assert.Equal(3000, ErrorCode.Unauthorized);
        Assert.Equal(3001, ErrorCode.InsufficientPermission);
        Assert.Equal(4000, ErrorCode.ResourceNotFound);
        Assert.Equal(5000, ErrorCode.InternalServerError);
        Assert.Equal(5001, ErrorCode.DatabaseOperationFailed);
    }

    [Fact]
    public void ErrorCode_Ranges_AreCorrect()
    {
        // Assert
        // 参数错误 (1000-1999)
        Assert.True(ErrorCode.ParameterEmpty >= 1000 && ErrorCode.ParameterEmpty < 2000);
        Assert.True(ErrorCode.ParameterFormatError >= 1000 && ErrorCode.ParameterFormatError < 2000);
        Assert.True(ErrorCode.ParameterValidationFailed >= 1000 && ErrorCode.ParameterValidationFailed < 2000);

        // 业务逻辑错误 (2000-2999)
        Assert.True(ErrorCode.ResourceAlreadyExists >= 2000 && ErrorCode.ResourceAlreadyExists < 3000);
        Assert.True(ErrorCode.OperationFailed >= 2000 && ErrorCode.OperationFailed < 3000);

        // 权限错误 (3000-3999)
        Assert.True(ErrorCode.Unauthorized >= 3000 && ErrorCode.Unauthorized < 4000);
        Assert.True(ErrorCode.InsufficientPermission >= 3000 && ErrorCode.InsufficientPermission < 4000);
        Assert.True(ErrorCode.InvalidToken >= 3000 && ErrorCode.InvalidToken < 4000);

        // 资源错误 (4000-4999)
        Assert.True(ErrorCode.ResourceNotFound >= 4000 && ErrorCode.ResourceNotFound < 5000);

        // 系统错误 (5000-5999)
        Assert.True(ErrorCode.InternalServerError >= 5000 && ErrorCode.InternalServerError < 6000);
        Assert.True(ErrorCode.DatabaseOperationFailed >= 5000 && ErrorCode.DatabaseOperationFailed < 6000);
    }
}