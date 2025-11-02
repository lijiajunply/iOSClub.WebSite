using iOSClub.Data;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.DataApi.Services;

public interface IDataCentreService
{
    public Task<List<YearCount>> GetYearDataAsync();
    public Task<List<AcademyCount>> GetCollegeDataAsync();
    public Task<List<GradeCount>> GetGradeDataAsync();
    public Task<List<LandscapeCount>> GetLandscapeDataAsync();
    public Task<List<GenderCount>> GetGenderDataAsync();
}

// 用于存储年度统计数据的记录类型
[Serializable]
public record YearCount(string Year, int Value);

// 用于存储年级统计数据的记录类型
[Serializable]
public record GradeCount(string Grade, int Value);

// 用于存储政治面貌统计数据的记录类型
[Serializable]
public record LandscapeCount(string Type, int Sales);

// 用于存储性别统计数据的记录类型
[Serializable]
public record GenderCount(string Type, int Value);

public class DataCentreService(IDbContextFactory<iOSContext> contextFactory) : IDataCentreService
{
    // 获取按年份统计数据
    public async Task<List<YearCount>> GetYearDataAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var yearData = new List<YearCount>();

        var total = await context.Students.CountAsync();
        var (year, month, _) = DateTime.Today;

        // 添加历史学年数据
        yearData.AddRange([
            new YearCount("2019学年", 33),
            new YearCount("2020学年", 1),
            new YearCount("2021学年", 274),
            new YearCount("2022学年", 329)
        ]);

        if (total <= 430) return yearData;
        var students = await context.Students.ToListAsync();
        for (var i = year - 2024; i >= 0; i--)
        {
            var date = new DateTime(year - i, 9, 1);
            var a = year - i - 2005;

            // 使用客户端评估来处理字符串到整数的转换
            var v = students.Count(s => s.JoinTime < date && int.Parse(s.UserId[..2]) > a);
            yearData.Add(new YearCount($"{year - i - 1}学年", v));
        }

        if (month < 9) return yearData;

        // 使用客户端评估来处理字符串到整数的转换
        var allStudents = await context.Students.ToListAsync();
        var value = allStudents.Count(s => int.Parse(s.UserId.Substring(0, 2)) > (year - 2004));
        yearData.Add(new YearCount($"{year}学年", value));

        return yearData;
    }

    // 获取按学院统计数据
    public async Task<List<AcademyCount>> GetCollegeDataAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        // 使用LINQ查询替代原始SQL以确保跨数据库兼容性
        return await context.Students
            .GroupBy(s => s.Academy)
            .Select(g => new AcademyCount { Type = g.Key, Value = g.Count() })
            .OrderByDescending(ac => ac.Value)
            .ToListAsync();
    }

    // 获取按年级统计数据
    public async Task<List<GradeCount>> GetGradeDataAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        // 使用客户端评估处理年级数据
        var students = await context.Students.ToListAsync();
        var groupedStudents = students.GroupBy(s => s.UserId.Substring(0, 2));

        var gradeData = groupedStudents.Select(group => new GradeCount(group.Key + "级", group.Count())).ToList();

        gradeData.Sort((x, y) => string.Compare(x.Grade, y.Grade, StringComparison.Ordinal));

        return gradeData;
    }

    // 获取按政治面貌统计数据
    public async Task<List<LandscapeCount>> GetLandscapeDataAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        // 使用客户端评估处理政治面貌数据
        var students = await context.Students.ToListAsync();
        var groupedStudents = students.GroupBy(s => s.PoliticalLandscape);

        return groupedStudents.Select(group => new LandscapeCount(group.Key, group.Count())).ToList();
    }

    // 获取按性别统计数据
    public async Task<List<GenderCount>> GetGenderDataAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var genderData = new List<GenderCount>();

        var man = await context.Students.CountAsync(x => x.Gender == "男");
        var woman = await context.Students.CountAsync(x => x.Gender == "女");

        genderData.AddRange(new List<GenderCount>
        {
            new("男", man),
            new("女", woman)
        });

        return genderData;
    }
}

// 用于存储统计数据的模型类
[Serializable]
public class AcademyCount
{
    public string Type { get; set; } = "";
    public int Value { get; set; }
}