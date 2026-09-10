using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace iOSClub.WebAPI.Common;

/// <summary>
/// 强制 HTTP 状态码与响应体里的 <see cref="IApiResponse.Code"/> 保持一致。
/// <para>
/// 背景：控制器普遍写成 <c>return Ok(ApiResponse.Fail(...))</c>，而 <c>Ok()</c> 会把 HTTP 状态
/// 固定成 200，于是业务失败时出现"HTTP 200 但 body 里 code=404/500"的分裂状态——
/// 代理、监控、浏览器的网络面板都读不出真实结果。
/// </para>
/// <para>
/// 与其逐个改写近两百处 <c>Ok(...)</c>，这里在 MVC 结果写入前统一纠正：
/// 凡是 <c>ErrorCode != 0</c> 的信封，一律把 HTTP 状态码设成它自带的 <c>Code</c>。
/// 成功响应（<c>ErrorCode == 0</c>）不受影响，201 Created 等状态码也原样保留。
/// </para>
/// <para>
/// 因此约定是：<b>body 恒为 <c>ApiResponse&lt;T&gt;</c>，HTTP 状态码恒等于 <c>code</c>，
/// <c>errorCode == 0</c> 表示成功</b>。新写的接口可以继续用 <c>Ok(...)</c> 包失败响应，
/// 也可以直接 <c>return StatusCode(body.Code, body)</c>，两者等价。
/// </para>
/// </summary>
public class ApiResponseResultFilter : IAlwaysRunResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is ObjectResult { Value: IApiResponse body } result && body.ErrorCode != 0)
        {
            result.StatusCode = body.Code;
        }
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
        // 无需处理
    }
}
