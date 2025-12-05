using System.Text.RegularExpressions;

namespace iOSClub.Data;

public static class ValidationTool
{
    // 数据类型检查
    public static bool IsValidInteger(string value)
    {
        return int.TryParse(value, out _);
    }

    public static bool IsValidDateTime(string value)
    {
        return DateTime.TryParse(value, out _);
    }

    // 长度限制检查
    public static bool IsValidLength(string value, int minLength, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
        {
            return minLength == 0;
        }
        return value.Length >= minLength && value.Length <= maxLength;
    }

    // 格式验证
    public static bool IsValidPhoneNumber(string phoneNumber)
    {
        // 简单的中国大陆手机号验证
        var regex = new Regex("^1[3-9]\\d{9}$");
        return regex.IsMatch(phoneNumber);
    }

    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return true; // 邮箱可以为空
        }
        var regex = new Regex("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$");
        return regex.IsMatch(email);
    }

    // 特殊字符过滤
    public static string SanitizeInput(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }
        // 移除危险的HTML标签和脚本
        return Regex.Replace(input, "<script[^>]*?>.*?</script>", "", RegexOptions.IgnoreCase);
    }

    // 密码强度验证
    public static bool IsValidPassword(string password, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (string.IsNullOrEmpty(password))
        {
            errorMessage = "密码不能为空";
            return false;
        }

        // 至少8个字符
        if (password.Length < 8)
        {
            errorMessage = "密码长度至少为8个字符";
            return false;
        }

        // 至少包含一个大写字母
        if (!Regex.IsMatch(password, "[A-Z]"))
        {
            errorMessage = "密码必须包含至少一个大写字母";
            return false;
        }

        // 至少包含一个数字
        if (!Regex.IsMatch(password, "[0-9]"))
        {
            errorMessage = "密码必须包含至少一个数字";
            return false;
        }

        return true;
    }
}