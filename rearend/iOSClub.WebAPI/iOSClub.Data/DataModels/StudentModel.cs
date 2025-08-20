using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace iOSClub.Data.DataModels;

public class StudentModel
{
    [Column(TypeName = "varchar(50)")] public string UserName { get; set; } = "";

    [Key]
    [Column(TypeName = "varchar(10)")]
    public string UserId { get; set; } = "";

    [Column(TypeName = "varchar(50)")] public string Academy { get; set; } = "";
    [Column(TypeName = "varchar(10)")] public string PoliticalLandscape { get; set; } = "群众";
    [Column(TypeName = "varchar(2)")] public string Gender { get; set; } = "";
    [Column(TypeName = "varchar(20)")] public string ClassName { get; set; } = "";
    [Column(TypeName = "varchar(11)")] public string PhoneNum { get; set; } = "";
    [Column(TypeName = "DATE")] public DateTime JoinTime { get; set; } = DateTime.Today;
    [Column(TypeName = "varchar(256)")] public string PasswordHash { get; set; } = "";
    [Column(TypeName = "varchar(256)")] public string? EMail { get; set; }

    public override string ToString()
    {
        return $"{UserName},{UserId},{Gender},{Academy},{PoliticalLandscape},{ClassName},{PhoneNum}";
    }

    public StudentModel Standardization(string? time = null)
    {
        UserId = UserId.Replace(" ", "");
        if (JoinTime.Year == 0)
        {
            JoinTime = string.IsNullOrEmpty(time)
                ? DateTime.Today
                : DateTime.Parse(time);
        }

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