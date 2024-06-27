using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class KnowledgeBaseService : IKnowledgeBaseService
    {
        private readonly IKnowledgeBaseRepository _knowledgeBaseRepository;
        private readonly IMapper _mapper;

        /// <summary>Initializes a new instance of the <see cref="KnowledgeBaseService" /> class.</summary>
        /// <param name="knowledgeBaseRepository">The knowledge base repository.</param>
        /// <param name="mapper">The mapper.</param>
        public KnowledgeBaseService(IKnowledgeBaseRepository knowledgeBaseRepository, IMapper mapper)
        {
            _knowledgeBaseRepository = knowledgeBaseRepository;
            _mapper = mapper;
        }

        /// <summary>Retrieves all.</summary>
        /// <returns>
        ///   <br />
        /// </returns>
        public IEnumerable<KnowledgeBaseViewModel> RetrieveAll()
        {
            var data = _knowledgeBaseRepository.RetrieveAll().Select(s => new KnowledgeBaseViewModel
            {
                ArticleId = s.ArticleId,
                Title = s.Title,
                Category = s.Category,
                Content = s.Content,
            });
            return data;
        }

        /// <summary>Adds the specified model.</summary>
        /// <param name="model">The model.</param>
        public void Add(KnowledgeBaseViewModel model)
        {
            var newModel = new KnowledgeBase();
            _mapper.Map(model, newModel);
            newModel.AuthorId = "David";
            newModel.DateCreated = DateTime.Now;
            newModel.DateUpdated = DateTime.Now;

            _knowledgeBaseRepository.Add(newModel);
        }

        /// <summary>Updates the specified model.</summary>
        /// <param name="model">The model.</param>
        public void Update(KnowledgeBaseViewModel model)
        {
            var existingData = _knowledgeBaseRepository.RetrieveAll().Where(s => s.ArticleId == model.ArticleId).FirstOrDefault();
            _mapper.Map(model, existingData);
            existingData.EditorId = "David";
            existingData.DateUpdated = DateTime.Now;

            _knowledgeBaseRepository.Update(existingData);
        }

        /// <summary>Deletes the specified identifier.</summary>
        /// <param name="id">The identifier.</param>
        public void Delete(int id)
        {
            _knowledgeBaseRepository.Delete(id);
        }
    }
}
