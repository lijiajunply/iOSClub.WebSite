using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace iOSClub.WebAPI.Controllers;

/// <summary>
/// 日志控制器，提供日志查询和过滤功能
/// 可以获取最近的日志条目或按级别过滤日志
/// </summary>
[ApiController]
[Route("[controller]")]
public class LogsController(ILogger<LogsController> logger)
    : ControllerBase
{
    /// <summary>
    /// SQLite数据库连接字符串，用于访问日志数据库
    /// </summary>
    private const string ConnectionString = "Data Source=logs/log.db";

    // 从配置获取连接字符串或使用默认值

    /// <summary>
    /// 获取最近的日志条目
    /// </summary>
    /// <param name="count">要返回的日志条目数量，默认为10条</param>
    /// <returns>JSON格式的日志条目列表</returns>
    [HttpGet]
    public async Task<IActionResult> GetRecentLogs(int count = 10)
    {
        try
        {
            var logs = new List<LogEntry>();

            await using (var connection = new SqliteConnection(ConnectionString))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = """
                                      SELECT Timestamp, Level, Exception, Properties, RenderedMessage
                                      FROM Logs
                                      ORDER BY Timestamp DESC 
                                      LIMIT @Count
                                      """;

                command.Parameters.AddWithValue("@Count", count);

                await using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        logs.Add(new LogEntry
                        {
                            Timestamp = reader.GetDateTime(0),
                            Level = reader.GetString(1),
                            Exception = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Properties = reader.IsDBNull(3)
                                ? null
                                : JsonSerializer.Deserialize<Dictionary<string, object>>(reader.GetString(3)),
                            Message = reader.IsDBNull(4) ? null : reader.GetString(4),
                        });
                    }
                }
            }

            return Ok(logs);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取日志时发生错误");
            return StatusCode(500, new { Error = "获取日志时发生错误", Details = ex.Message });
        }
    }

    /// <summary>
    /// 按级别过滤日志
    /// </summary>
    /// <param name="level">日志级别 (Information, Warning, Error等)</param>
    /// <param name="count">要返回的日志条目数量，默认为10条</param>
    /// <returns>指定级别的日志条目列表</returns>
    [HttpGet("filter")]
    public async Task<IActionResult> GetLogsByLevel(string level, int count = 10)
    {
        try
        {
            var logs = new List<LogEntry>();

            await using (var connection = new SqliteConnection(ConnectionString))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = """
                                      SELECT Timestamp, Level, Exception, Properties, RenderedMessage FROM Logs
                                      WHERE Level = @Level
                                      ORDER BY Timestamp DESC
                                      LIMIT @Count
                                      """;

                command.Parameters.AddWithValue("@Level", level);
                command.Parameters.AddWithValue("@Count", count);

                await using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        logs.Add(new LogEntry
                        {
                            Timestamp = reader.GetDateTime(0),
                            Level = reader.GetString(1),
                            Exception = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Properties = reader.IsDBNull(3)
                                ? null
                                : JsonSerializer.Deserialize<Dictionary<string, object>>(reader.GetString(3)),
                            Message = reader.IsDBNull(4) ? null : reader.GetString(4),
                        });
                    }
                }
            }

            return Ok(logs);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "按级别过滤日志时发生错误");
            return StatusCode(500, new { Error = "按级别过滤日志时发生错误", Details = ex.Message });
        }
    }
}

/// <summary>
/// 日志条目模型，用于表示单条日志记录
/// </summary>
[Serializable]
public class LogEntry
{
    /// <summary>
    /// 日志时间戳
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 日志级别（如 Information, Warning, Error等）
    /// </summary>
    public string Level { get; set; } = "";

    /// <summary>
    /// 异常信息（如果有的话）
    /// </summary>
    public string? Exception { get; set; }

    /// <summary>
    /// 日志属性字典
    /// </summary>
    public Dictionary<string, object>? Properties { get; set; }

    /// <summary>
    /// 日志消息内容
    /// </summary>
    public string? Message { get; set; }
}