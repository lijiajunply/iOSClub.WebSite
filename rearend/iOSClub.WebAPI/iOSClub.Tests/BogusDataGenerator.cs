using Bogus;
using iOSClub.Data;
using iOSClub.Data.DataModels;

namespace iOSClub.Tests;

/// <summary>
/// Bogus数据生成器工具类，用于生成各种数据模型的伪数据
/// </summary>
public static class BogusDataGenerator
{
    // 用于生成唯一ID的计数器
    private static int _counter = 0;
    
    /// <summary>
    /// 重置计数器
    /// </summary>
    public static void ResetCounter()
    {
        _counter = 0;
    }
    
    /// <summary>
    /// 获取下一个唯一ID
    /// </summary>
    private static int NextId()
    {
        return Interlocked.Increment(ref _counter);
    }
    
    /// <summary>
    /// StudentModel的Bogus生成器
    /// </summary>
    public static readonly Faker<StudentModel> StudentFaker = new Faker<StudentModel>()
        .RuleFor(s => s.UserName, f => f.Name.FullName())
        .RuleFor(s => s.UserId, f => $"2023{NextId().ToString().PadLeft(6, '0')}")
        .RuleFor(s => s.Academy, f => f.PickRandom("计算机学院", "软件学院", "电子工程学院", "通信工程学院", "自动化学院"))
        .RuleFor(s => s.PoliticalLandscape, f => f.PickRandom("群众", "共青团员", "中共预备党员", "中共党员"))
        .RuleFor(s => s.Gender, f => f.PickRandom("男", "女"))
        .RuleFor(s => s.ClassName, f => f.PickRandom("计科2001", "软工2002", "电子2003", "通信2004", "自动化2005"))
        .RuleFor(s => s.PhoneNum, f => $"1{f.Random.Number(3, 9)}{f.Random.Number(100000000, 999999999)}") // 确保中国大陆手机号格式
        .RuleFor(s => s.JoinTime, f => f.Date.Past(2))
        .RuleFor(s => s.PasswordHash, f => DataTool.StringToHash("password123"))
        .RuleFor(s => s.EMail, f => f.Internet.Email());
    
    /// <summary>
    /// ArticleModel的Bogus生成器
    /// </summary>
    public static readonly Faker<ArticleModel> ArticleFaker = new Faker<ArticleModel>()
        .RuleFor(a => a.Path, f => $"/articles/{f.Random.Guid()}")
        .RuleFor(a => a.Title, f => f.Lorem.Sentence(5, 3))
        .RuleFor(a => a.Content, f => f.Lorem.Paragraphs(3))
        .RuleFor(a => a.Identity, f => f.PickRandom("Founder", "President", "Minister", "Department", "Member"))
        .RuleFor(a => a.CategoryId, f => f.Random.Guid().ToString())
        .RuleFor(a => a.LastWriteTime, f => f.Date.Past(1))
        .RuleFor(a => a.ArticleOrder, f => f.Random.Int(0, 100));
    
    /// <summary>
    /// CategoryModel的Bogus生成器
    /// </summary>
    public static readonly Faker<CategoryModel> CategoryFaker = new Faker<CategoryModel>()
        .RuleFor(c => c.Id, f => f.Random.Guid().ToString())
        .RuleFor(c => c.Name, f => f.Lorem.Word())
        .RuleFor(c => c.Order, f => f.Random.Int(0, 100));
    
    /// <summary>
    /// ClientApplication的Bogus生成器
    /// </summary>
    public static readonly Faker<ClientApplication> ClientApplicationFaker = new Faker<ClientApplication>()
        .RuleFor(c => c.ClientId, f => f.Random.Guid().ToString())
        .RuleFor(c => c.ClientSecret, f => f.Random.AlphaNumeric(32))
        .RuleFor(c => c.ApplicationName, f => f.Company.CompanyName())
        .RuleFor(c => c.Description, f => f.Company.CatchPhrase())
        .RuleFor(c => c.HomepageUrl, f => f.Internet.Url())
        .RuleFor(c => c.RedirectUris, f => $"http://localhost:{f.Random.Number(3000, 9999)};http://localhost:{f.Random.Number(3000, 9999)}")
        .RuleFor(c => c.LogoUrl, f => f.Internet.UrlWithPath())
        .RuleFor(c => c.IsActive, f => f.Random.Bool())
        .RuleFor(c => c.SupportsPkce, f => f.Random.Bool())
        .RuleFor(c => c.IsNeedEMail, f => f.Random.Bool());
    
    /// <summary>
    /// DepartmentModel的Bogus生成器
    /// </summary>
    public static readonly Faker<DepartmentModel> DepartmentFaker = new Faker<DepartmentModel>()
        .RuleFor(d => d.Key, f => f.Random.Guid().ToString())
        .RuleFor(d => d.Name, f => f.Lorem.Word())
        .RuleFor(d => d.Description, f => f.Lorem.Sentence());
    
    /// <summary>
    /// ProjectModel的Bogus生成器
    /// </summary>
    public static readonly Faker<ProjectModel> ProjectFaker = new Faker<ProjectModel>()
        .RuleFor(p => p.Id, f => f.Random.Guid().ToString())
        .RuleFor(p => p.Title, f => f.Company.CatchPhrase())
        .RuleFor(p => p.Description, f => f.Lorem.Paragraphs(2))
        .RuleFor(p => p.StartTime, f => f.Date.Past(6).ToString("yyyy-MM-dd"))
        .RuleFor(p => p.EndTime, f => f.Date.Future(6).ToString("yyyy-MM-dd"))
        .RuleFor(p => p.Staffs, _ => new List<StaffModel>())
        .RuleFor(p => p.Tasks, _ => new List<TaskModel>());
    
    /// <summary>
    /// ResourceModel的Bogus生成器
    /// </summary>
    public static readonly Faker<ResourceModel> ResourceFaker = new Faker<ResourceModel>()
        .RuleFor(r => r.Id, f => f.Random.Guid().ToString())
        .RuleFor(r => r.Name, f => f.Lorem.Word())
        .RuleFor(r => r.Description, f => f.Lorem.Sentence())
        .RuleFor(r => r.Tag, f => f.Lorem.Word());
    
    /// <summary>
    /// StaffModel的Bogus生成器
    /// </summary>
    public static readonly Faker<StaffModel> StaffFaker = new Faker<StaffModel>()
        .RuleFor(s => s.UserId, f => $"2023{NextId().ToString().PadLeft(6, '0')}")
        .RuleFor(s => s.Name, f => f.Name.FullName())
        .RuleFor(s => s.Identity, f => f.PickRandom("Founder", "President", "Minister", "Department", "Member"))
        .RuleFor(s => s.Projects, _ => new List<ProjectModel>())
        .RuleFor(s => s.Tasks, _ => new List<TaskModel>());
    
    /// <summary>
    /// TaskModel的Bogus生成器
    /// </summary>
    public static readonly Faker<TaskModel> TaskFaker = new Faker<TaskModel>()
        .RuleFor(t => t.Id, f => f.Random.Guid().ToString())
        .RuleFor(t => t.Title, f => f.Lorem.Sentence(3, 2))
        .RuleFor(t => t.Description, f => f.Lorem.Paragraph())
        .RuleFor(t => t.Status, f => f.Random.Bool())
        .RuleFor(t => t.StartTime, f => f.Date.Past(30).ToString("yyyy-MM-dd"))
        .RuleFor(t => t.EndTime, f => f.Date.Future(30).ToString("yyyy-MM-dd"))
        .RuleFor(t => t.Users, _ => new List<StaffModel>());
    
    /// <summary>
    /// TodoModel的Bogus生成器
    /// </summary>
    public static readonly Faker<TodoModel> TodoFaker = new Faker<TodoModel>()
        .RuleFor(t => t.Id, f => f.Random.Guid().ToString())
        .RuleFor(t => t.Title, f => f.Lorem.Sentence(3, 2))
        .RuleFor(t => t.Description, f => f.Lorem.Paragraph())
        .RuleFor(t => t.Status, f => f.Random.Bool())
        .RuleFor(t => t.StartTime, f => f.Date.Past(30).ToString("yyyy-MM-dd"))
        .RuleFor(t => t.EndTime, f => f.Date.Future(30).ToString("yyyy-MM-dd"))
        .RuleFor(t => t.CreatedTime, f => f.Date.Past(30))
        .RuleFor(t => t.StudentId, f => $"2023{NextId().ToString().PadLeft(6, '0')}");
    
    /// <summary>
    /// 生成指定数量的StudentModel
    /// </summary>
    public static List<StudentModel> GenerateStudents(int count)
    {
        return StudentFaker.Generate(count);
    }
    
    /// <summary>
    /// 生成指定数量的ArticleModel
    /// </summary>
    public static List<ArticleModel> GenerateArticles(int count)
    {
        return ArticleFaker.Generate(count);
    }
    
    /// <summary>
    /// 生成指定数量的CategoryModel
    /// </summary>
    public static List<CategoryModel> GenerateCategories(int count)
    {
        return CategoryFaker.Generate(count);
    }
    
    /// <summary>
    /// 生成指定数量的ClientApplication
    /// </summary>
    public static List<ClientApplication> GenerateClientApplications(int count)
    {
        return ClientApplicationFaker.Generate(count);
    }
    
    /// <summary>
    /// 生成指定数量的DepartmentModel
    /// </summary>
    public static List<DepartmentModel> GenerateDepartments(int count)
    {
        return DepartmentFaker.Generate(count);
    }
    
    /// <summary>
    /// 生成指定数量的ProjectModel
    /// </summary>
    public static List<ProjectModel> GenerateProjects(int count)
    {
        return ProjectFaker.Generate(count);
    }
    
    /// <summary>
    /// 生成指定数量的ResourceModel
    /// </summary>
    public static List<ResourceModel> GenerateResources(int count)
    {
        return ResourceFaker.Generate(count);
    }
    
    /// <summary>
    /// 生成指定数量的StaffModel
    /// </summary>
    public static List<StaffModel> GenerateStaffs(int count)
    {
        return StaffFaker.Generate(count);
    }
    
    /// <summary>
    /// 生成指定数量的TaskModel
    /// </summary>
    public static List<TaskModel> GenerateTasks(int count)
    {
        return TaskFaker.Generate(count);
    }
    
    /// <summary>
    /// 生成指定数量的TodoModel
    /// </summary>
    public static List<TodoModel> GenerateTodos(int count)
    {
        return TodoFaker.Generate(count);
    }
}