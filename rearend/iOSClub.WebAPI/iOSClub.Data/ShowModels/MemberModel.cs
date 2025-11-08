using iOSClub.Data.DataModels;

namespace iOSClub.Data.ShowModels;

public class MemberModel : StudentModel
{
    /// <summary>
    /// Founder : 创始人
    /// President : 社长,副社长,秘书长
    /// Minister : 部长/副部长
    /// Department : 部员成员
    /// Member : 普通成员
    /// </summary>
    public string Identity { get; set; } = "Member";

    public static readonly Dictionary<string, string> IdentityDictionary = new()
    {
        { "Founder", "创始人" },
        { "President", "社长/团支书" },
        { "Department", "部员" },
        { "Minister", "部长/副部长" },
        { "Member", "普通成员" }
    };

    public static TChild AutoCopy<TParent, TChild>(TParent parent) where TChild : TParent, new()
    {
        var child = new TChild();
        var parentType = typeof(TParent);
        var properties = parentType.GetProperties();
        foreach (var property in properties)
        {
            //循环遍历属性
            if (property is { CanRead: true, CanWrite: true })
            {
                //进行属性拷贝
                property.SetValue(child, property.GetValue(parent, null), null);
            }
        }

        return child;
    }

    public static MemberModel CopyFrom(StudentModel model)
    {
        return AutoCopy<StudentModel, MemberModel>(model);
    }

    public static MemberModel CopyFrom(StudentModel model, string identity)
    {
        var a = AutoCopy<StudentModel, MemberModel>(model);

        a.Identity = identity;

        return a;
    }

    public override string ToString()
    {
        return $"{UserName},{UserId},{Gender},{Academy},{PoliticalLandscape},{ClassName},{PhoneNum},{Identity}";
    }
}