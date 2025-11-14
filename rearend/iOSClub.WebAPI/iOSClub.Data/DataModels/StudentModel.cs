using System.ComponentModel.DataAnnotations;
using System.Text;

namespace iOSClub.Data.DataModels;

public class StudentModel
{
    [MaxLength(50)] public string UserName { get; set; } = "";

    [Key] [MaxLength(10)] public string UserId { get; set; } = "";

    [MaxLength(50)] public string Academy { get; set; } = "";
    [MaxLength(10)] public string PoliticalLandscape { get; set; } = "群众";
    [MaxLength(2)] public string Gender { get; set; } = "";
    [MaxLength(20)] public string ClassName { get; set; } = "";
    [MaxLength(14)] public string PhoneNum { get; set; } = "";
    public DateTime JoinTime { get; set; } = DateTime.UtcNow;
    [MaxLength(256)] public string PasswordHash { get; set; } = "";
    [MaxLength(256)] public string? EMail { get; set; }

    public override string ToString()
    {
        return $"{UserName},{UserId},{Gender},{Academy},{PoliticalLandscape},{ClassName},{PhoneNum}";
    }

    public StudentModel Standardization()
    {
        UserId = UserId.Replace(" ", "");
        JoinTime = DateTime.SpecifyKind(JoinTime.Year == 0 ? DateTime.UtcNow : JoinTime, DateTimeKind.Utc);
        return this;
    }

    public static string GetCsv(IEnumerable<StudentModel> models)
    {
        var builder = new StringBuilder("姓名,学号,性别,学院,政治面貌,专业班级,电话号码,电子邮箱,密码(Hash)");
        foreach (var model in models)
            builder.Append("\n" + model);

        return builder.ToString();
    }

    public bool IsStandardization()
    {
        return !string.IsNullOrEmpty(UserId) &&
               !string.IsNullOrEmpty(UserName) &&
               !string.IsNullOrEmpty(Academy) &&
               !string.IsNullOrEmpty(PoliticalLandscape) &&
               !string.IsNullOrEmpty(Gender) &&
               !string.IsNullOrEmpty(ClassName) &&
               !string.IsNullOrEmpty(PhoneNum) &&
               UserId.Length == 10;
    }

    public void Update(StudentModel model)
    {
        if (!string.IsNullOrEmpty(model.UserName)) UserName = model.UserName;
        if (!string.IsNullOrEmpty(model.Academy)) Academy = model.Academy;
        if (!string.IsNullOrEmpty(model.PoliticalLandscape)) PoliticalLandscape = model.PoliticalLandscape;
        if (!string.IsNullOrEmpty(model.Gender)) Gender = model.Gender;
        if (!string.IsNullOrEmpty(model.ClassName)) ClassName = model.ClassName;
        if (!string.IsNullOrEmpty(model.PhoneNum)) PhoneNum = model.PhoneNum;
        if (!string.IsNullOrEmpty(model.EMail)) EMail = model.EMail;
    }
}