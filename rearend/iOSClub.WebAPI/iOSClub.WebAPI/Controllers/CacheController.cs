using System.Text;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace iOSClub.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class CacheController(
    IConnectionMultiplexer redis) : ControllerBase
{
    private readonly IDatabase _db = redis.GetDatabase();

    [HttpGet("/clean")]
    public async Task<IActionResult> CleanData()
    {
        try
        {
            // 获取Redis服务器实例
            var server = redis.GetServer(redis.GetEndPoints().First());

            // 查找所有键
            var keys = server.Keys(pattern: "*", pageSize: 1000);

            // 转换为数组
            var keyArray = keys.ToArray();
            var builder = new StringBuilder();
            foreach (var key in keyArray)
            {
                builder.AppendLine(key);
            }

            // 删除所有键并获取删除的数量
            long deletedCount = 0;
            if (keyArray.Length > 0)
            {
                deletedCount = await _db.KeyDeleteAsync(keyArray);
            }

            return Ok($"缓存已清除，共清理了 {deletedCount} 个缓存项 \n\r 清理的缓存项列表：\n\r {builder}");
        }
        catch (Exception ex)
        {
            return BadRequest($"清除缓存时出现问题: {ex.Message}");
        }
    }
}