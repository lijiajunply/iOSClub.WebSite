using Bogus;
using iOSClub.Data.DataObjects;

namespace iOSClub.Tests;

public class BogusDataGenerationTests
{
    [Fact]
    public void GenerateStudentDOsWithBogus()
    {
        // 配置Bogus生成器
        var faker = new Faker<StudentDO>()
            .RuleFor(s => s.UserName, f => f.Name.FullName())
            .RuleFor(s => s.UserId, f => f.Random.Replace("2023######"))
            .RuleFor(s => s.Academy, f => f.PickRandom("计算机学院", "软件学院", "电子工程学院", "通信工程学院", "自动化学院"))
            .RuleFor(s => s.PoliticalLandscape, f => f.PickRandom("群众", "共青团员", "中共预备党员", "中共党员"))
            .RuleFor(s => s.Gender, f => f.PickRandom("男", "女"))
            .RuleFor(s => s.ClassName, f => f.PickRandom("计科2001", "软工2002", "电子2003", "通信2004", "自动化2005"))
            .RuleFor(s => s.PhoneNum, f => f.Phone.PhoneNumber("1##########"))
            .RuleFor(s => s.JoinTime, f => f.Date.Past(2))
            .RuleFor(s => s.PasswordHash, f => f.Internet.Password())
            .RuleFor(s => s.EMail, f => f.Internet.Email());

        // 生成10个学生模型
        var students = faker.Generate(10);

        // 验证生成的数据
        Assert.NotNull(students);
        Assert.Equal(10, students.Count);
        
        foreach (var student in students)
        {
            Assert.False(string.IsNullOrEmpty(student.UserName));
            Assert.False(string.IsNullOrEmpty(student.UserId));
            Assert.Equal(10, student.UserId.Length);
            Assert.False(string.IsNullOrEmpty(student.Academy));
            Assert.False(string.IsNullOrEmpty(student.Gender));
            Assert.False(string.IsNullOrEmpty(student.PhoneNum));
            Assert.Equal(11, student.PhoneNum.Length);
            Assert.True(student.JoinTime < DateTime.Now);
        }
    }

    [Fact]
    public void GenerateArticleDOsWithBogus()
    {
        // 配置Bogus生成器用于ArticleDO
        var faker = new Faker<ArticleDO>()
            .RuleFor(a => a.Path, f => $"/articles/{f.Random.Guid()}")
            .RuleFor(a => a.Title, f => f.Lorem.Sentence(5, 3))
            .RuleFor(a => a.Content, f => f.Lorem.Paragraphs(3))
            .RuleFor(a => a.Identity, f => f.PickRandom("Founder", "President", "Minister", "Department"))
            .RuleFor(a => a.CategoryId, f => f.Random.Guid().ToString())
            .RuleFor(a => a.LastWriteTime, f => f.Date.Past(1))
            .RuleFor(a => a.ArticleOrder, f => f.Random.Int(0, 100));

        // 生成5篇文章
        var articles = faker.Generate(5);

        // 验证生成的数据
        Assert.NotNull(articles);
        Assert.Equal(5, articles.Count);
        
        foreach (var article in articles)
        {
            Assert.False(string.IsNullOrEmpty(article.Path));
            Assert.False(string.IsNullOrEmpty(article.Title));
            Assert.False(string.IsNullOrEmpty(article.Content));
            Assert.True(article.LastWriteTime < DateTime.Now);
        }
    }

}
