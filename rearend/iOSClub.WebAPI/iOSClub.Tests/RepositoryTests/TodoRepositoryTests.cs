using iOSClub.Data;
using iOSClub.Data.DataModels;
using iOSClub.DataApi.Repositories;
using Microsoft.EntityFrameworkCore;
using iOSClub.Tests;

namespace iOSClub.Tests.RepositoryTests;

public class TodoRepositoryTests
{
    private readonly TodoRepository _repository;
    private readonly IDbContextFactory<ClubContext> _contextFactory;

    public TodoRepositoryTests()
    {
        // 创建in-memory数据库
        var options = new DbContextOptionsBuilder<ClubContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ClubContext(options);
        _contextFactory = new TestDbContextFactory(options);
        _repository = new TodoRepository(_contextFactory);

        // 同步清理数据
        context.Todos.RemoveRange(context.Todos);
        context.Students.RemoveRange(context.Students);
        context.SaveChanges();
    }

    [Fact]
    public async Task GetTodosByUserIdAsync_ReturnsUserTodos()
    {
        // Arrange
        var student1UserId = Guid.NewGuid().ToString().Substring(0, 10);
        var student2UserId = Guid.NewGuid().ToString().Substring(0, 10);
        
        await using (var context = await _contextFactory.CreateDbContextAsync())
        {
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
            
            // 直接创建Student实例，确保每个实例都有唯一的UserId
            var student1 = new StudentModel
            {
                UserId = student1UserId,
                UserName = "Student 1",
                PasswordHash = DataTool.StringToHash("password1"),
                PhoneNum = "13800138001",
                Academy = "Computer Science",
                Gender = "男",
                PoliticalLandscape = "共青团员",
                JoinTime = DateTime.Now.AddDays(-30)
            };
            var student2 = new StudentModel
            {
                UserId = student2UserId,
                UserName = "Student 2",
                PasswordHash = DataTool.StringToHash("password2"),
                PhoneNum = "13800138002",
                Academy = "Information Technology",
                Gender = "女",
                PoliticalLandscape = "群众",
                JoinTime = DateTime.Now.AddDays(-20)
            };

            await context.Students.AddRangeAsync(student1, student2);
            await context.SaveChangesAsync();

            // 直接创建Todo实例，确保每个实例都有唯一的Id，并将Student属性显式设为null
            var todo1 = new TodoModel
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = student1UserId,
                Student = null,
                Title = "Test Todo 1",
                Description = "Description 1",
                Status = false,
                StartTime = DateTime.Now.AddDays(-5).ToString("yyyy-MM-dd"),
                EndTime = DateTime.Now.AddDays(5).ToString("yyyy-MM-dd"),
                CreatedTime = DateTime.Now
            };
            var todo2 = new TodoModel
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = student1UserId,
                Student = null,
                Title = "Test Todo 2",
                Description = "Description 2",
                Status = true,
                StartTime = DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd"),
                EndTime = DateTime.Now.AddDays(3).ToString("yyyy-MM-dd"),
                CreatedTime = DateTime.Now
            };
            var todo3 = new TodoModel
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = student2UserId,
                Student = null,
                Title = "Other User Todo",
                Description = "Other Description",
                Status = false,
                StartTime = DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd"),
                EndTime = DateTime.Now.AddDays(2).ToString("yyyy-MM-dd"),
                CreatedTime = DateTime.Now
            };

            await context.Todos.AddRangeAsync(todo1, todo2, todo3);
            await context.SaveChangesAsync();
        }

        // Act
        var result = await _repository.GetTodosByUserIdAsync(student1UserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, todo => Assert.Equal(student1UserId, todo.StudentId));
    }

    [Fact]
    public async Task GetTodoByIdAsync_ValidId_ReturnsTodo()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        // Arrange
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var todo = BogusDataGenerator.TodoFaker.Generate();

        await context.Todos.AddAsync(todo);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetTodoByIdAsync(todo.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(todo.Id, result.Id);
        Assert.Equal(todo.Title, result.Title);
    }

    [Fact]
    public async Task GetTodoByIdAsync_InvalidId_ReturnsNull()
    {
        // Act
        var result = await _repository.GetTodoByIdAsync("non-existent-id");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddTodoAsync_ValidTodo_ReturnsTrue()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        // Arrange
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var todo = BogusDataGenerator.TodoFaker.Generate();

        // Act
        var result = await _repository.AddTodoAsync(todo);

        // Assert
        Assert.True(result);
        Assert.Equal(1, await context.Todos.CountAsync());
    }

    [Fact]
    public async Task UpdateTodoAsync_ValidTodo_ReturnsTrue()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        // Arrange
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var todo = BogusDataGenerator.TodoFaker.Generate();

        await context.Todos.AddAsync(todo);
        await context.SaveChangesAsync();

        var updatedTodo = BogusDataGenerator.TodoFaker
            .RuleFor(t => t.Id, todo.Id)
            .RuleFor(t => t.StudentId, todo.StudentId)
            .RuleFor(t => t.CreatedTime, todo.CreatedTime)
            .Generate();

        // Act
        var result = await _repository.UpdateTodoAsync(updatedTodo);

        // Assert
        Assert.True(result);
        
        // 使用新的 DbContext 来查询更新后的 todo 项，因为每个 DbContext 都有自己的缓存
        var newContext = await _contextFactory.CreateDbContextAsync();
        var dbTodo = await newContext.Todos.FirstAsync(t => t.Id == todo.Id);
        Assert.Equal(updatedTodo.Title, dbTodo.Title);
        Assert.Equal(updatedTodo.Description, dbTodo.Description);
        Assert.Equal(updatedTodo.Status, dbTodo.Status);
    }

    [Fact]
    public async Task UpdateTodoAsync_InvalidId_ReturnsFalse()
    {
        // Arrange
        var todo = BogusDataGenerator.TodoFaker.Generate();
        todo.Id = "non-existent-id";

        // Act
        var result = await _repository.UpdateTodoAsync(todo);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteTodoAsync_ValidId_ReturnsTrue()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        // Arrange
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var todo = BogusDataGenerator.TodoFaker.Generate();

        await context.Todos.AddAsync(todo);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteTodoAsync(todo.Id);

        // Assert
        Assert.True(result);
        Assert.Equal(0, await context.Todos.CountAsync());
    }

    [Fact]
    public async Task DeleteTodoAsync_InvalidId_ReturnsFalse()
    {
        // Act
        var result = await _repository.DeleteTodoAsync("non-existent-id");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task TodoExistsAsync_ExistingId_ReturnsTrue()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        // Arrange
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var todo = BogusDataGenerator.TodoFaker.Generate();

        await context.Todos.AddAsync(todo);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.TodoExistsAsync(todo.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task TodoExistsAsync_NonExistingId_ReturnsFalse()
    {
        // Act
        var result = await _repository.TodoExistsAsync("non-existent-id");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HasPermissionAsync_Owner_ReturnsTrue()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        // Arrange
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // 先创建并保存Student实例
        var student = BogusDataGenerator.StudentFaker.Generate();
        await context.Students.AddAsync(student);
        await context.SaveChangesAsync();

        // 创建Todo实例，关联到已存在的Student
        var todo = BogusDataGenerator.TodoFaker
            .RuleFor(t => t.StudentId, student.UserId)
            .RuleFor(t => t.Student, student)
            .Generate();

        await context.Todos.AddAsync(todo);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.HasPermissionAsync(todo.Id, student.UserId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasPermissionAsync_NotOwner_ReturnsFalse()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        // Arrange
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var student1 = BogusDataGenerator.StudentFaker.Generate();
        var student2 = BogusDataGenerator.StudentFaker.Generate();
        
        var todo = BogusDataGenerator.TodoFaker
            .RuleFor(t => t.StudentId, student1.UserId)
            .Generate();

        await context.Todos.AddAsync(todo);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.HasPermissionAsync(todo.Id, student2.UserId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetTodoCountAsync_ReturnsCorrectCount()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        // Arrange
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // 先创建并保存Student实例
        var student1 = BogusDataGenerator.StudentFaker.Generate();
        var student2 = BogusDataGenerator.StudentFaker.Generate();
        await context.Students.AddRangeAsync(student1, student2);
        await context.SaveChangesAsync();

        // 创建Todo实例，关联到已存在的Student
        var todos = new List<TodoModel>
        {
            BogusDataGenerator.TodoFaker
                .RuleFor(t => t.StudentId, student1.UserId)
                .RuleFor(t => t.Student, student1)
                .Generate(),
            BogusDataGenerator.TodoFaker
                .RuleFor(t => t.StudentId, student1.UserId)
                .RuleFor(t => t.Student, student1)
                .Generate(),
            BogusDataGenerator.TodoFaker
                .RuleFor(t => t.StudentId, student2.UserId)
                .RuleFor(t => t.Student, student2)
                .Generate()
        };

        await context.Todos.AddRangeAsync(todos);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetTodoCountAsync(student1.UserId);

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public async Task GetCompletedTodoCountAsync_ReturnsCorrectCount()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        // Arrange
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // 先创建并保存Student实例
        var student = BogusDataGenerator.StudentFaker.Generate();
        await context.Students.AddAsync(student);
        await context.SaveChangesAsync();

        // 创建Todo实例，关联到已存在的Student
        var todos = new List<TodoModel>
        {
            BogusDataGenerator.TodoFaker
                .RuleFor(t => t.StudentId, student.UserId)
                .RuleFor(t => t.Student, student)
                .RuleFor(t => t.Status, false)
                .Generate(),
            BogusDataGenerator.TodoFaker
                .RuleFor(t => t.StudentId, student.UserId)
                .RuleFor(t => t.Student, student)
                .RuleFor(t => t.Status, true)
                .Generate(),
            BogusDataGenerator.TodoFaker
                .RuleFor(t => t.StudentId, student.UserId)
                .RuleFor(t => t.Student, student)
                .RuleFor(t => t.Status, true)
                .Generate()
        };

        await context.Todos.AddRangeAsync(todos);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetCompletedTodoCountAsync(student.UserId);

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public async Task GetTodosPagedAsync_ReturnsCorrectPage()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        // Arrange
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // 先创建并保存Student实例
        var student = BogusDataGenerator.StudentFaker.Generate();
        await context.Students.AddAsync(student);
        await context.SaveChangesAsync();

        // 创建Todo实例，关联到已存在的Student
        var todos = new List<TodoModel>();
        for (var i = 1; i <= 15; i++)
        {
            var todo = BogusDataGenerator.TodoFaker
                .RuleFor(t => t.StudentId, student.UserId)
                .RuleFor(t => t.Student, student)
                .RuleFor(t => t.Id, $"todo-{i}")
                .RuleFor(t => t.CreatedTime, DateTime.Now.AddMinutes(-i))
                .Generate();
            todos.Add(todo);
        }

        await context.Todos.AddRangeAsync(todos);
        await context.SaveChangesAsync();

        // Act
        var page1 = await _repository.GetTodosPagedAsync(student.UserId, 1, 5);
        var page2 = await _repository.GetTodosPagedAsync(student.UserId, 2, 5);
        var page3 = await _repository.GetTodosPagedAsync(student.UserId, 3, 5);

        // Assert
        Assert.Equal(5, page1.Count);
        Assert.Equal(5, page2.Count);
        Assert.Equal(5, page3.Count);

        Assert.Equal("todo-1", page1[0].Id);
        Assert.Equal("todo-6", page2[0].Id);
        Assert.Equal("todo-11", page3[0].Id);
    }
}