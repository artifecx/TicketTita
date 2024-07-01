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
                CreateMap<TicketViewModel, Ticket>();
                CreateMap<Ticket, TicketViewModel>();
                CreateMap<KnowledgeBaseViewModel, KnowledgeBaseArticle>();
                CreateMap<KnowledgeBaseArticle, KnowledgeBaseViewModel>();
                CreateMap<IEnumerable<KnowledgeBaseViewModel>, IEnumerable<KnowledgeBaseArticle>>();

            }
        }
    }
}
