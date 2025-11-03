# SSO集成指南

本文档详细说明了如何在iOS Club系统中集成OAuth2登录和第三方应用SSO功能。

## 1. OAuth2登录方法

### 1.1 登录流程

1. 用户访问 `/OAuth/login` 端点发起登录请求
2. 系统将用户重定向到OAuth提供商进行身份验证
3. OAuth提供商验证用户身份后，将用户重定向回系统的回调端点 `/OAuth/callback`
4. 系统验证认证结果，提取用户信息并生成JWT令牌
5. 系统返回JWT令牌给客户端

### 1.2 核心代码实现

```csharp
[HttpGet("login")]
public IActionResult Login(string provider = "OAuth2", string? returnUrl = "/")
{
    var redirectUrl = Url.Action(nameof(Callback), "OAuth", new { returnUrl });
    var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
    return Challenge(properties, provider);
}

[HttpGet("callback")]
public async Task<IActionResult> Callback(string? returnUrl = "/")
{
    var authenticateResult = await HttpContext.AuthenticateAsync("OAuth2");

    if (!authenticateResult.Succeeded)
        return BadRequest("OAuth2 认证失败");

    var claims = authenticateResult.Principal?.Claims;
    if (claims == null)
        return BadRequest("无法获取用户信息");

    // 获取用户标识信息
    var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    var userName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
    var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

    if (string.IsNullOrEmpty(userId))
        return BadRequest("无法获取用户ID");

    // 使用 loginService 进行用户登录并生成 Token
    // 首先尝试员工登录
    var token = await _loginService.StaffLogin(userId, userName ?? userId);
    
    // 如果员工登录失败，尝试普通用户登录
    if (string.IsNullOrEmpty(token))
    {
        token = await _loginService.Login(userId, "defaultPassword");
    }
    
    // 如果登录仍然失败，返回错误
    if (string.IsNullOrEmpty(token))
        return BadRequest("用户登录失败");

    // 返回 token 给客户端
    return Ok(new { Token = token, ReturnUrl = returnUrl });
}
```

### 1.3 令牌交换接口

系统还提供了令牌交换接口，允许通过用户信息直接获取JWT令牌：

```csharp
[HttpPost("exchange")]
public async Task<IActionResult> ExchangeToken([FromBody] OAuthExchangeRequest request)
{
    if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.UserName))
        return BadRequest("用户ID和用户名不能为空");
        
    // 首先尝试员工登录
    var token = await _loginService.StaffLogin(request.UserId, request.UserName);
    
    // 如果员工登录失败，尝试普通用户登录
    if (string.IsNullOrEmpty(token))
    {
        token = await _loginService.Login(request.UserId, "defaultPassword");
    }
    
    // 如果登录仍然失败，返回错误
    if (string.IsNullOrEmpty(token))
        return BadRequest("用户登录失败");
        
    return Ok(new { Token = token });
}
```

## 2. 第三方应用SSO集成

### 2.1 SSO架构

系统支持OAuth2标准的授权码流程，允许第三方应用通过SSO集成访问系统资源。

### 2.2 核心组件

1. **ClientApplication实体** - 存储第三方应用信息
2. **IClientApplicationRepository** - 管理客户端应用数据
3. **SSOController** - 处理SSO流程
4. **ClientAppController** - 管理客户端应用

### 2.3 SSO流程

1. **应用注册**：管理员通过ClientAppController注册第三方应用
2. **授权请求**：第三方应用将用户重定向到`/SSO/authorize`
3. **用户认证**：系统将用户重定向到OAuth提供商
4. **回调处理**：认证成功后，系统发放授权码
5. **令牌交换**：第三方应用使用授权码换取访问令牌
6. **资源访问**：第三方应用使用令牌访问受保护资源

### 2.4 授权端点

```
GET /SSO/authorize
```

参数：
- `client_id` - 第三方应用的客户端ID
- `redirect_uri` - 回调地址
- `state` - 用于防止CSRF攻击的随机字符串
- `response_type` - 响应类型（默认为code）

### 2.5 令牌端点

```
POST /SSO/token
```

参数：
- `grant_type` - 授权类型（必须为authorization_code）
- `code` - 授权码
- `client_id` - 客户端ID
- `client_secret` - 客户端密钥
- `redirect_uri` - 回调地址

### 2.6 用户信息端点

```
GET /SSO/userinfo
```

需要在请求头中包含：
```
Authorization: Bearer ACCESS_TOKEN
```

返回用户信息：
```json
{
  "sub": "用户ID",
  "name": "用户名",
  "email": "邮箱",
  "role": "用户角色"
}
```

### 2.7 安全特性

1. **客户端验证**：每个第三方应用都有唯一的ClientId和ClientSecret
2. **回调URL白名单**：防止重定向攻击
3. **角色权限控制**：只有特定角色可以管理客户端应用
4. **令牌管理**：支持令牌的生成、验证和撤销

## 3. 使用示例

### 3.1 第三方应用集成步骤

1. 联系系统管理员注册应用，获取ClientId和ClientSecret
2. 将用户重定向到授权端点：
   ```
   GET /SSO/authorize?client_id=CLIENT_ID&redirect_uri=REDIRECT_URI&state=STATE&response_type=code
   ```
3. 在回调URL中接收授权码
4. 使用授权码换取访问令牌：
   ```
   POST /SSO/token
   grant_type=authorization_code&code=AUTH_CODE&client_id=CLIENT_ID&client_secret=CLIENT_SECRET&redirect_uri=REDIRECT_URI
   ```
5. 使用访问令牌访问用户信息：
   ```
   GET /SSO/userinfo
   Authorization: Bearer ACCESS_TOKEN
   ```

### 3.2 管理员操作

管理员可以使用以下端点管理客户端应用：

1. 获取所有客户端应用：`GET /ClientApp`
2. 获取特定客户端应用：`GET /ClientApp/{clientId}`
3. 创建客户端应用：`POST /ClientApp`
4. 更新客户端应用：`PUT /ClientApp/{clientId}`
5. 删除客户端应用：`DELETE /ClientApp/{clientId}`
6. 重新生成客户端密钥：`POST /ClientApp/{clientId}/regenerate-secret`

## 4. 注意事项

1. 在生产环境中，应使用更安全的密码管理机制替代默认密码
2. 应定期更新客户端密钥以提高安全性
3. 回调URL必须在白名单中，防止重定向攻击
4. 授权码和访问令牌应有适当的过期时间