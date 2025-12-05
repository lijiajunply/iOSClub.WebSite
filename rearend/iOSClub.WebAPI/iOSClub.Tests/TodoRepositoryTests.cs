using iOSClub.Data;
using iOSClub.Data.DataModels;
using iOSClub.DataApi.Repositories;
using Microsoft.EntityFrameworkCore;

namespace iOSClub.Tests;

public class TodoRepositoryTests
{
    private readonly ClubContext _context;
    private readonly TodoRepository _repository;
    private readonly string _testUserId = "user123";
    private readonly string _testUserId2 = "user456";

    public TodoRepositoryTests()
    {
        // 创建in-memory数据库
        var options = new DbContextOptionsBuilder<ClubContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ClubContext(options);
        _repository = new TodoRepository(_context);

        // 清理数据
        _context.Todos.RemoveRange(_context.Todos);
        _context.Students.RemoveRange(_context.Students);
        _context.SaveChangesAsync().Wait();
    }

    [Fact]
    public async Task GetTodosByUserIdAsync_ReturnsUserTodos()
    {
        // Arrange
        var todo1 = new TodoModel
        {
            Id = "todo-1",
            StudentId = _testUserId,
            Title = "Test Todo 1",
            Description = "Description 1",
            Status = false,
            CreatedTime = DateTime.Now,
            Student = null
        };

        var todo2 = new TodoModel
        {
            Id = "todo-2",
            StudentId = _testUserId,
            Title = "Test Todo 2",
            Description = "Description 2",
            Status = true,
            CreatedTime = DateTime.Now,
            Student = null
        };

        var todo3 = new TodoModel
        {
            Id = "todo-3",
            StudentId = _testUserId2,
            Title = "Other User Todo",
            Description = "Other Description",
            Status = false,
            CreatedTime = DateTime.Now,
            Student = null
        };

        await _context.Todos.AddRangeAsync(todo1, todo2, todo3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetTodosByUserIdAsync(_testUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, todo => Assert.Equal(_testUserId, todo.StudentId));
        Assert.Contains(result, todo => todo.Id == "todo-1");
        Assert.Contains(result, todo => todo.Id == "todo-2");
    }

    [Fact]
    public async Task GetTodoByIdAsync_ValidId_ReturnsTodo()
    {
        // Arrange
        var todo = new TodoModel
        {
            Id = "todo-1",
            StudentId = _testUserId,
            Title = "Test Todo",
            Description = "Description",
            Status = false,
            CreatedTime = DateTime.Now,
            Student = null
        };

        await _context.Todos.AddAsync(todo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetTodoByIdAsync("todo-1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("todo-1", result.Id);
        Assert.Equal("Test Todo", result.Title);
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
        // Arrange
        var todo = new TodoModel
        {
            Id = "todo-1",
            StudentId = _testUserId,
            Title = "Test Todo",
            Description = "Description",
            Status = false,
            CreatedTime = DateTime.Now,
            Student = null
        };

        // Act
        var result = await _repository.AddTodoAsync(todo);

        // Assert
        Assert.True(result);
        Assert.Equal(1, await _context.Todos.CountAsync());
    }

    [Fact]
    public async Task UpdateTodoAsync_ValidTodo_ReturnsTrue()
    {
        // Arrange
        var todo = new TodoModel
        {
            Id = "todo-1",
            StudentId = _testUserId,
            Title = "Test Todo",
            Description = "Description",
            Status = false,
            CreatedTime = DateTime.Now,
            Student = null
        };

        await _context.Todos.AddAsync(todo);
        await _context.SaveChangesAsync();

        var updatedTodo = new TodoModel
        {
            Id = "todo-1",
            StudentId = _testUserId,
            Title = "Updated Todo",
            Description = "Updated Description",
            Status = true,
            CreatedTime = todo.CreatedTime,
            Student = null
        };

        // Act
        var result = await _repository.UpdateTodoAsync(updatedTodo);

        // Assert
        Assert.True(result);
        var dbTodo = await _context.Todos.FirstAsync(t => t.Id == "todo-1");
        Assert.Equal("Updated Todo", dbTodo.Title);
        Assert.Equal("Updated Description", dbTodo.Description);
        Assert.True(dbTodo.Status);
    }

    [Fact]
    public async Task UpdateTodoAsync_InvalidId_ReturnsFalse()
    {
        // Arrange
        var todo = new TodoModel
        {
            Id = "non-existent-id",
            StudentId = _testUserId,
            Title = "Test Todo",
            Description = "Description",
            Status = false,
            CreatedTime = DateTime.Now
        };

        // Act
        var result = await _repository.UpdateTodoAsync(todo);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteTodoAsync_ValidId_ReturnsTrue()
    {
        // Arrange
        var todo = new TodoModel
        {
            Id = "todo-1",
            StudentId = _testUserId,
            Title = "Test Todo",
            Description = "Description",
            Status = false,
            CreatedTime = DateTime.Now
        };

        await _context.Todos.AddAsync(todo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteTodoAsync("todo-1");

        // Assert
        Assert.True(result);
        Assert.Equal(0, await _context.Todos.CountAsync());
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
        // Arrange
        var todo = new TodoModel
        {
            Id = "todo-1",
            StudentId = _testUserId,
            Title = "Test Todo",
            Description = "Description",
            Status = false,
            CreatedTime = DateTime.Now
        };

        await _context.Todos.AddAsync(todo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.TodoExistsAsync("todo-1");

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
        // Arrange
        var todo = new TodoModel
        {
            Id = "todo-1",
            StudentId = _testUserId,
            Title = "Test Todo",
            Description = "Description",
            Status = false,
            CreatedTime = DateTime.Now,
            Student = null
        };

        await _context.Todos.AddAsync(todo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.HasPermissionAsync("todo-1", _testUserId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasPermissionAsync_NotOwner_ReturnsFalse()
    {
        // Arrange
        var todo = new TodoModel
        {
            Id = "todo-1",
            StudentId = _testUserId,
            Title = "Test Todo",
            Description = "Description",
            Status = false,
            CreatedTime = DateTime.Now,
            Student = null
        };

        await _context.Todos.AddAsync(todo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.HasPermissionAsync("todo-1", _testUserId2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetTodoCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var todos = new List<TodoModel>
        {
            new TodoModel
            {
                Id = "todo-1",
                StudentId = _testUserId,
                Title = "Test Todo 1",
                Description = "Description 1",
                Status = false,
                CreatedTime = DateTime.Now,
                Student = null
            },
            new TodoModel
            {
                Id = "todo-2",
                StudentId = _testUserId,
                Title = "Test Todo 2",
                Description = "Description 2",
                Status = true,
                CreatedTime = DateTime.Now,
                Student = null
            },
            new TodoModel
            {
                Id = "todo-3",
                StudentId = _testUserId2,
                Title = "Other User Todo",
                Description = "Other Description",
                Status = false,
                CreatedTime = DateTime.Now,
                Student = null
            }
        };

        await _context.Todos.AddRangeAsync(todos);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetTodoCountAsync(_testUserId);

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public async Task GetCompletedTodoCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var todos = new List<TodoModel>
        {
            new TodoModel
            {
                Id = "todo-1",
                StudentId = _testUserId,
                Title = "Test Todo 1",
                Description = "Description 1",
                Status = false,
                CreatedTime = DateTime.Now,
                Student = null
            },
            new TodoModel
            {
                Id = "todo-2",
                StudentId = _testUserId,
                Title = "Test Todo 2",
                Description = "Description 2",
                Status = true,
                CreatedTime = DateTime.Now,
                Student = null
            },
            new TodoModel
            {
                Id = "todo-3",
                StudentId = _testUserId,
                Title = "Test Todo 3",
                Description = "Description 3",
                Status = true,
                CreatedTime = DateTime.Now,
                Student = null
            }
        };

        await _context.Todos.AddRangeAsync(todos);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetCompletedTodoCountAsync(_testUserId);

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public async Task GetTodosPagedAsync_ReturnsCorrectPage()
    {
        // Arrange
        var todos = new List<TodoModel>();
        for (int i = 1; i <= 15; i++)
        {
            todos.Add(new TodoModel
            {
                Id = $"todo-{i}",
                StudentId = _testUserId,
                Title = $"Test Todo {i}",
                Description = $"Description {i}",
                Status = i % 2 == 0,
                CreatedTime = DateTime.Now.AddMinutes(-i),
                Student = null
            });
        }

        await _context.Todos.AddRangeAsync(todos);
        await _context.SaveChangesAsync();

        // Act
        var page1 = await _repository.GetTodosPagedAsync(_testUserId, 1, 5);
        var page2 = await _repository.GetTodosPagedAsync(_testUserId, 2, 5);
        var page3 = await _repository.GetTodosPagedAsync(_testUserId, 3, 5);

        // Assert
        Assert.Equal(5, page1.Count);
        Assert.Equal(5, page2.Count);
        Assert.Equal(5, page3.Count);

        Assert.Equal("todo-1", page1[0].Id);
        Assert.Equal("todo-6", page2[0].Id);
        Assert.Equal("todo-11", page3[0].Id);
    }
}
