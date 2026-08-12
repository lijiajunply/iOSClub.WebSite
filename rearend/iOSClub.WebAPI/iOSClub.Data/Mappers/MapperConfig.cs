using System.Reflection;
using iOSClub.Data.DataObjects;
using iOSClub.Data.VOs;
using Mapster;

namespace iOSClub.Data.Mappers;

public static class MapperConfig
{
    public static void Configure()
    {
        var config = TypeAdapterConfig.GlobalSettings;

        // DO → VO 映射（VO不含PasswordHash/ClientSecret，Mapster自动跳过不存在的目标属性）
        config.NewConfig<StudentDO, StudentVO>();
        config.NewConfig<StudentDO, MemberVO>();

        config.NewConfig<StaffDO, StaffVO>()
            .Map(dest => dest.DepartmentName,
                src => src.Department != null ? src.Department.Name : null);

        config.NewConfig<ArticleDO, ArticleVO>()
            .Map(dest => dest.CategoryName,
                src => src.Category != null ? src.Category.Name : null);

        config.NewConfig<ArticleDO, ArticleListItemVO>()
            .Map(dest => dest.CategoryName,
                src => src.Category != null ? src.Category.Name : null);

        config.NewConfig<ProjectDO, ProjectVO>()
            .Map(dest => dest.DepartmentName,
                src => src.Department != null ? src.Department.Name : null)
            .Map(dest => dest.Staffs,
                src => src.Staffs.Select(s => new ProjectStaffSummary { UserId = s.UserId, Name = s.Name }).ToList())
            .Map(dest => dest.Tasks,
                src => src.Tasks.Select(t => new ProjectTaskSummary { Id = t.Id, Title = t.Title, Status = t.Status }).ToList());

        config.NewConfig<TaskDO, TaskVO>();
        config.NewConfig<TodoDO, TodoVO>();
        config.NewConfig<ResourceDO, ResourceVO>();

        config.NewConfig<CategoryDO, CategoryVO>()
            .Map(dest => dest.Articles,
                src => src.Articles.Select(a => a.Adapt<ArticleListItemVO>()).ToList());

        config.NewConfig<DepartmentDO, DepartmentVO>()
            .Map(dest => dest.StaffCount, src => src.Staffs.Count)
            .Map(dest => dest.ProjectCount, src => src.Projects.Count);

        config.NewConfig<ClientApplicationDO, ClientAppVO>();

        config.NewConfig<ClientApplicationDO, ClientAppResultVO>()
            .Map(dest => dest.RedirectUris,
                src => src.RedirectUris.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList());

        // DTO → DO 映射
        config.NewConfig<DTOs.StudentCreateDTO, StudentDO>()
            .Map(dest => dest.PasswordHash, src => Data.DataTool.StringToHash(src.Password));

        config.NewConfig<DTOs.StudentUpdateDTO, StudentDO>()
            .Ignore(dest => dest.UserId)
            .Ignore(dest => dest.JoinTime);

        config.NewConfig<DTOs.StaffCreateDTO, StaffDO>()
            .Ignore(dest => dest.Department)
            .Ignore(dest => dest.Projects)
            .Ignore(dest => dest.Tasks);

        config.NewConfig<DTOs.ArticleCreateDTO, ArticleDO>()
            .Ignore(dest => dest.CategoryId)
            .Ignore(dest => dest.Category)
            .Ignore(dest => dest.LastWriteTime);

        config.NewConfig<DTOs.ProjectCreateUpdateDTO, ProjectDO>();
        config.NewConfig<DTOs.TaskCreateUpdateDTO, TaskDO>();
        config.NewConfig<DTOs.TodoCreateUpdateDTO, TodoDO>();
        config.NewConfig<DTOs.ResourceCreateUpdateDTO, ResourceDO>();
        config.NewConfig<DTOs.CategoryCreateUpdateDTO, CategoryDO>();
        config.NewConfig<DTOs.DepartmentCreateUpdateDTO, DepartmentDO>();
        config.NewConfig<DTOs.ClientAppCreateDTO, ClientApplicationDO>();
        config.NewConfig<DTOs.ClientAppUpdateDTO, ClientApplicationDO>();
    }
}
