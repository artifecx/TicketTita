using AutoMapper;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp
{
    // AutoMapper configuration
    internal partial class StartupConfigurer
    {
        /// <summary>
        /// Configure auto mapper
        /// </summary>
        private void ConfigureAutoMapper()
        {
            var mapperConfiguration = new MapperConfiguration(config =>
            {
                config.AddProfile(new AutoMapperProfileConfiguration());
            });

            this._services.AddSingleton<IMapper>(sp => mapperConfiguration.CreateMapper());
        }

        private class AutoMapperProfileConfiguration : Profile
        {
            public AutoMapperProfileConfiguration()
            {
                
                CreateMap<UserViewModel, User>();
                CreateMap<User, UserViewModel>();
                CreateMap<TicketViewModel, Ticket>();
                /*.ForMember(dest => dest.CategoryType, opt => opt.MapFrom(src => src.CategoryType))
                .ForMember(dest => dest.PriorityType, opt => opt.MapFrom(src => src.PriorityType))
                .ForMember(dest => dest.StatusType, opt => opt.MapFrom(src => src.StatusType))
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User));*/
                CreateMap<Ticket, TicketViewModel>();
                        /*.ForMember(dest => dest.CategoryType, opt => opt.MapFrom(src => src.CategoryType))
                        .ForMember(dest => dest.PriorityType, opt => opt.MapFrom(src => src.PriorityType))
                        .ForMember(dest => dest.StatusType, opt => opt.MapFrom(src => src.StatusType))
                        .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User));*/
                //CreateMap<TicketViewModel, TicketViewModel>();
                CreateMap<Feedback, FeedbackViewModel>();
                CreateMap<FeedbackViewModel, Feedback>();
                CreateMap<KnowledgeBaseViewModel, KnowledgeBaseArticle>();
                CreateMap<KnowledgeBaseArticle, KnowledgeBaseViewModel>();
                CreateMap<IEnumerable<KnowledgeBaseViewModel>, IEnumerable<KnowledgeBaseArticle>>();
            }
        }
    }
}
