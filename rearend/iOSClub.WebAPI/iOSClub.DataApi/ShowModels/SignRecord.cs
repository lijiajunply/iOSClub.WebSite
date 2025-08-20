

namespace iOSClub.DataApi.ShowModels;

public static class SignRecord
{
    #region Table

    public static readonly string[] Academies =
    [
        "信息与控制工程学院",
        "理学院",
        "机电工程学院",
        "管理学院",
        "土木工程学院",
        "环境与市政工程学院",
        "建筑设备科学与工程学院",
        "材料科学与工程学院",
        "冶金工程学院",
        "资源工程学院",
        "城市发展与现代交通学院",
        "文学院",
        "艺术学院",
        "建筑学院",
        "马克思主义学院",
        "公共管理学院",
        "化学与化工学院",
        "体育学院",
        "安德学院",
        "未来技术学院",
        "国际教育学院"
    ];

    public static readonly string[] PoliticalLandscapes =
    [
        "群众",
        "共青团员",
        "中共党员",
        "中共预备党员"
    ];

    public static readonly string[] Genders = ["男", "女"];

    #endregion

    #region Project

    public static Dictionary<string, string> DepartmentDictionary => new()
    {
        ["All"] = "所有",
        ["Technology"] = "科技部",
        ["NewMedia"] = "新媒体部",
        ["Practical"] = "交流实践部",
        [""] = "其他",
        ["Other"] = "其他"
    };

    #endregion


}