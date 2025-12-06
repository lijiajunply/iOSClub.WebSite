using iOSClub.Data.DataModels;
using iOSClub.Data.ShowModels;
using iOSClub.DataApi.CQRS;
using iOSClub.DataApi.CQRS.Commands;
using iOSClub.DataApi.CQRS.Handlers;
using iOSClub.DataApi.CQRS.Queries;
using iOSClub.DataApi.Repositories;
using iOSClub.DataApi.Services;
using iOSClub.WebAPI.Common.Security;

namespace iOSClub.WebAPI.Common.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// 注册CQRS相关服务
        /// </summary>
        public void RegisterCqrsServices()
        {
            // 注册CQRS查询处理器和命令处理器
            services.AddScoped<StudentQueryHandler>();
            services.AddScoped<StudentCommandHandler>();
            services.AddScoped<ArticleQueryHandler>();
            services.AddScoped<ArticleCommandHandler>();
            services.AddScoped<CategoryQueryHandler>();
            services.AddScoped<CategoryCommandHandler>();
            services.AddScoped<ProjectQueryHandler>();
            services.AddScoped<ProjectCommandHandler>();

            // 注册CQRS处理程序的接口映射
            services.AddScoped<IQueryHandler<GetStudentsQuery, List<StudentModel>>, StudentQueryHandler>();
            services.AddScoped<IQueryHandler<GetStudentByIdQuery, StudentModel?>, StudentQueryHandler>();
            services.AddScoped<IQueryHandler<GetStudentsPagedQuery, (List<MemberModel>, int)>, StudentQueryHandler>();
            services.AddScoped<IQueryHandler<GetArticlesQuery, IEnumerable<ArticleModel>>, ArticleQueryHandler>();
            services.AddScoped<IQueryHandler<GetArticleByIdQuery, ArticleModel?>, ArticleQueryHandler>();
            services
                .AddScoped<IQueryHandler<GetArticlesByCategoryQuery, IEnumerable<ArticleModel>>, ArticleQueryHandler>();
            services.AddScoped<IQueryHandler<GetCategoriesQuery, IEnumerable<CategoryModel>>, CategoryQueryHandler>();
            services.AddScoped<IQueryHandler<GetCategoryByIdQuery, CategoryModel?>, CategoryQueryHandler>();
            services.AddScoped<IQueryHandler<GetCategoryByNameQuery, CategoryModel?>, CategoryQueryHandler>();
            services.AddScoped<IQueryHandler<GetProjectsQuery, IEnumerable<ProjectModel>>, ProjectQueryHandler>();
            services.AddScoped<IQueryHandler<GetProjectByIdQuery, ProjectModel?>, ProjectQueryHandler>();

            services.AddScoped<ICommandHandler<CreateStudentCommand, StudentModel?>, StudentCommandHandler>();
            services.AddScoped<ICommandHandler<UpdateStudentCommand, bool>, StudentCommandHandler>();
            services.AddScoped<ICommandHandler<DeleteStudentCommand, bool>, StudentCommandHandler>();
            services.AddScoped<ICommandHandler<UpdateManyStudentsCommand, bool>, StudentCommandHandler>();
            services.AddScoped<ICommandHandler<StudentLoginCommand, bool>, StudentCommandHandler>();
            services.AddScoped<ICommandHandler<CreateArticleCommand, bool>, ArticleCommandHandler>();
            services.AddScoped<ICommandHandler<UpdateArticleCommand, bool>, ArticleCommandHandler>();
            services.AddScoped<ICommandHandler<DeleteArticleCommand, bool>, ArticleCommandHandler>();
            services.AddScoped<ICommandHandler<UpdateManyArticlesCommand, bool>, ArticleCommandHandler>();
            services.AddScoped<ICommandHandler<CreateCategoryCommand, bool>, CategoryCommandHandler>();
            services.AddScoped<ICommandHandler<UpdateCategoryCommand, bool>, CategoryCommandHandler>();
            services.AddScoped<ICommandHandler<DeleteCategoryCommand, bool>, CategoryCommandHandler>();
            services.AddScoped<ICommandHandler<UpdateManyCategoriesCommand, bool>, CategoryCommandHandler>();
            services.AddScoped<ICommandHandler<CreateProjectCommand, bool>, ProjectCommandHandler>();
            services.AddScoped<ICommandHandler<UpdateProjectCommand, bool>, ProjectCommandHandler>();
            services.AddScoped<ICommandHandler<DeleteProjectCommand, bool>, ProjectCommandHandler>();
        }

        /// <summary>
        /// 注册仓库和服务
        /// </summary>
        public void RegisterRepositoriesAndServices()
        {
            services.AddScoped<IArticleRepository, ArticleRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IDepartmentRepository, DepartmentRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IResourceRepository, ResourceRepository>();
            services.AddScoped<IStaffRepository, StaffRepository>();
            services.AddScoped<IStudentRepository, StudentRepository>();
            services.AddScoped<ITodoRepository, TodoRepository>();

            services.AddScoped<IDataCentreService, DataCentreService>();
            services.AddScoped<IClientApplicationRepository, ClientApplicationRepository>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ILoginService, LoginService>();
        }

        /// <summary>
        /// 注册安全相关服务
        /// </summary>
        public void RegisterSecurityServices()
        {
            services.AddSingleton<MaskingConfig>();
            services.AddSingleton<DataMaskingService>();
            services.AddSingleton<LogAuditService>();

            services.AddSingleton<RateLimitConfig>();
            services.AddSingleton<RateLimitService>();

            services.AddSingleton<JwtService>();
            services.AddSingleton<RsaKeyManager>();
        }
    }
}