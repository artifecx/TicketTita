using AutoMapper;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;

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
                CreateMap<UserViewModel, User>().ReverseMap();
                CreateMap<TicketViewModel, Ticket>();
                CreateMap<Ticket, TicketViewModel>()
                        .ForMember(dest => dest.Attachment, opt => opt.MapFrom(src => src.Attachments.FirstOrDefault()))
                        .ForMember(dest => dest.Agent, opt => opt.MapFrom(src => src.TicketAssignment != null ? src.TicketAssignment.Agent : null))
                        .ForMember(dest => dest.Team, opt => opt.MapFrom(src => src.TicketAssignment != null ? src.TicketAssignment.Team : null))
                        .ForMember(dest => dest.TicketAssignment, opt => opt.MapFrom(src => src.TicketAssignment))
                        .ForMember(dest => dest.Feedback, opt => opt.MapFrom(src => src.Feedback))
                        .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments));
                CreateMap<Feedback, FeedbackViewModel>().ReverseMap();
                CreateMap<Comment, CommentViewModel>()
                        .ForMember(dest => dest.Replies, opt => opt.MapFrom(src => src.InverseParent))
                        .ReverseMap()
                        .ForMember(dest => dest.InverseParent, opt => opt.MapFrom(src => src.Replies));
                CreateMap<Team, TeamViewModel>().ReverseMap();
                CreateMap<KnowledgeBaseViewModel, KnowledgeBaseArticle>().ReverseMap();
                CreateMap<IEnumerable<KnowledgeBaseViewModel>, IEnumerable<KnowledgeBaseArticle>>();
            }
        }
    }
}
