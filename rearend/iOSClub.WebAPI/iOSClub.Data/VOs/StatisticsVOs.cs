namespace iOSClub.Data.VOs;

/// <summary>
/// 用于存储年度统计数据的记录类型
/// </summary>
[Serializable]
public record YearCountVO(string Year, int Value);

/// <summary>
/// 用于存储学院统计数据的记录类型
/// </summary>
[Serializable]
public record AcademyCountVO(string Type, int Value);

/// <summary>
/// 用于存储年级统计数据的记录类型
/// </summary>
[Serializable]
public record GradeCountVO(string Grade, int Value);

/// <summary>
/// 用于存储政治面貌统计数据的记录类型
/// </summary>
[Serializable]
public record LandscapeCountVO(string Type, int Value);

/// <summary>
/// 用于存储性别统计数据的记录类型
/// </summary>
[Serializable]
public record GenderCountVO(string Type, int Value);
