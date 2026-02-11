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

            // Company Mappings
            CreateMap<Company, CompanyViewModel>()
                .ForMember(dest => dest.DepartmentCount, opt => opt.MapFrom(src => src.Departments != null ? src.Departments.Count : 0))
                .ForMember(dest => dest.LayoutCount, opt => opt.MapFrom(src => src.Layouts != null ? src.Layouts.Count : 0));
            CreateMap<CompanyViewModel, Company>();
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

            // Layout Mappings
            CreateMap<Layout, LayoutViewModel>();
            CreateMap<LayoutViewModel, Layout>();
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

            // Content Mappings
            CreateMap<Content, ContentViewModel>();
            CreateMap<ContentViewModel, Content>();
            CreateMap<CreateContentDTO, Content>()
                .ForMember(dest => dest.ContentID, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

            // Schedule Mappings
            CreateMap<Schedule, ScheduleViewModel>()
                .ForMember(dest => dest.PageCount, opt => opt.MapFrom(src => src.SchedulePages != null ? src.SchedulePages.Count : 0));
            CreateMap<ScheduleViewModel, Schedule>();

            // UserCompanyRole Mappings
            CreateMap<UserCompanyRole, UserCompanyRoleViewModel>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.CompanyName : string.Empty))
                .ForMember(dest => dest.IsOffice365User, opt => opt.MapFrom(src => src.User != null && src.User.IsOffice365User));

            // CompanyConfiguration Mappings
            CreateMap<CompanyConfiguration, CompanyConfigurationDTO>();
            CreateMap<CompanyConfigurationDTO, CompanyConfiguration>()
                .ForMember(dest => dest.ConfigurationID, opt => opt.Ignore())
                .ForMember(dest => dest.CompanyID, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.MapFrom(src => DateTime.UtcNow));
        }
    }
}
