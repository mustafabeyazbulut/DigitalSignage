using AutoMapper;
using DigitalSignage.Models;
using DigitalSignage.ViewModels;
using DigitalSignage.DTOs;

namespace DigitalSignage.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User Mappings
            CreateMap<User, UserViewModel>();
            CreateMap<UserViewModel, User>();
            CreateMap<CreateUserDTO, User>()
                .ForMember(dest => dest.UserID, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));
            CreateMap<UpdateUserDTO, User>()
                .ForMember(dest => dest.UserID, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.IsOffice365User, opt => opt.Ignore())
                .ForMember(dest => dest.AzureADObjectId, opt => opt.Ignore())
                .ForMember(dest => dest.PhotoUrl, opt => opt.Ignore())
                .ForMember(dest => dest.LastLoginDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<User, UpdateUserDTO>();

            // Company Mappings
            CreateMap<Company, CompanyViewModel>()
                .ForMember(dest => dest.DepartmentCount, opt => opt.MapFrom(src => src.Departments != null ? src.Departments.Count : 0))
                .ForMember(dest => dest.LayoutCount, opt => opt.MapFrom(src => src.Layouts != null ? src.Layouts.Count : 0));
            CreateMap<CompanyViewModel, Company>();
            CreateMap<CreateCompanyDTO, Company>()
                .ForMember(dest => dest.CompanyID, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));
            CreateMap<UpdateCompanyDTO, Company>()
                .ForMember(dest => dest.CompanyID, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Department Mappings
            CreateMap<Department, DepartmentViewModel>()
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.CompanyName : string.Empty))
                .ForMember(dest => dest.PageCount, opt => opt.MapFrom(src => src.Pages != null ? src.Pages.Count : 0))
                .ForMember(dest => dest.ContentCount, opt => opt.MapFrom(src => src.Contents != null ? src.Contents.Count : 0))
                .ForMember(dest => dest.ScheduleCount, opt => opt.MapFrom(src => src.Schedules != null ? src.Schedules.Count : 0));
            CreateMap<DepartmentViewModel, Department>();
            CreateMap<CreateDepartmentDTO, Department>()
                .ForMember(dest => dest.DepartmentID, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));
            CreateMap<UpdateDepartmentDTO, Department>()
                .ForMember(dest => dest.DepartmentID, opt => opt.Ignore())
                .ForMember(dest => dest.CompanyID, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

            // Layout Mappings
            CreateMap<Layout, LayoutViewModel>();
            CreateMap<LayoutViewModel, Layout>();
            CreateMap<CreateLayoutDTO, Layout>()
                .ForMember(dest => dest.LayoutID, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));
            CreateMap<DynamicLayoutDTO, Layout>()
                .ForMember(dest => dest.LayoutID, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));
            CreateMap<UpdateLayoutDTO, Layout>()
                .ForMember(dest => dest.LayoutID, opt => opt.Ignore())
                .ForMember(dest => dest.CompanyID, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore());
            CreateMap<Layout, DynamicLayoutViewModel>()
                .ForMember(dest => dest.Sections, opt => opt.MapFrom(src => src.LayoutSections != null
                    ? src.LayoutSections.Select(ls => new DynamicLayoutViewModel.GridSectionDTO
                    {
                        SectionID = ls.LayoutSectionID,
                        Position = ls.SectionPosition,
                        Column = ls.ColumnIndex,
                        Row = ls.RowIndex,
                        Width = ls.Width ?? "100%",
                        Height = ls.Height ?? "100%"
                    }).ToList()
                    : new List<DynamicLayoutViewModel.GridSectionDTO>()));

            // Page Mappings
            CreateMap<Page, PageViewModel>()
                .ForMember(dest => dest.LayoutName, opt => opt.MapFrom(src => src.Layout != null ? src.Layout.LayoutName : string.Empty))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.DepartmentName : string.Empty))
                .ForMember(dest => dest.ContentCount, opt => opt.MapFrom(src => src.PageContents != null ? src.PageContents.Count : 0));
            CreateMap<PageViewModel, Page>();
            CreateMap<CreatePageDTO, Page>()
                .ForMember(dest => dest.PageID, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));
            CreateMap<UpdatePageDTO, Page>()
                .ForMember(dest => dest.PageID, opt => opt.Ignore())
                .ForMember(dest => dest.DepartmentID, opt => opt.Ignore())
                .ForMember(dest => dest.PageCode, opt => opt.Ignore())
                .ForMember(dest => dest.LayoutID, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore());

            // Content Mappings
            CreateMap<Content, ContentViewModel>();
            CreateMap<ContentViewModel, Content>();
            CreateMap<CreateContentDTO, Content>()
                .ForMember(dest => dest.ContentID, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));
            CreateMap<UpdateContentDTO, Content>()
                .ForMember(dest => dest.ContentID, opt => opt.Ignore())
                .ForMember(dest => dest.DepartmentID, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Schedule Mappings
            CreateMap<Schedule, ScheduleViewModel>()
                .ForMember(dest => dest.PageCount, opt => opt.MapFrom(src => src.SchedulePages != null ? src.SchedulePages.Count : 0));
            CreateMap<ScheduleViewModel, Schedule>();
            CreateMap<CreateScheduleDTO, Schedule>()
                .ForMember(dest => dest.ScheduleID, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.SchedulePages, opt => opt.Ignore());
            CreateMap<UpdateScheduleDTO, Schedule>()
                .ForMember(dest => dest.ScheduleID, opt => opt.Ignore())
                .ForMember(dest => dest.DepartmentID, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore());

            // UserCompanyRole Mappings
            CreateMap<UserCompanyRole, UserCompanyRoleViewModel>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.CompanyName : string.Empty))
                .ForMember(dest => dest.IsOffice365User, opt => opt.MapFrom(src => src.User != null && src.User.IsOffice365User));

            // UserDepartmentRole Mappings
            CreateMap<UserDepartmentRole, UserDepartmentRoleDTO>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.DepartmentName : string.Empty));
            CreateMap<UserDepartmentRoleDTO, UserDepartmentRole>()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Department, opt => opt.Ignore());
            CreateMap<UserDepartmentRole, UserDepartmentRoleViewModel>()
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.DepartmentName : string.Empty))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Department != null && src.Department.Company != null ? src.Department.Company.CompanyName : string.Empty));

            // CompanyConfiguration Mappings
            CreateMap<CompanyConfiguration, CompanyConfigurationDTO>();
            CreateMap<CompanyConfigurationDTO, CompanyConfiguration>()
                .ForMember(dest => dest.ConfigurationID, opt => opt.Ignore())
                .ForMember(dest => dest.CompanyID, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.MapFrom(src => DateTime.UtcNow));
        }
    }
}
