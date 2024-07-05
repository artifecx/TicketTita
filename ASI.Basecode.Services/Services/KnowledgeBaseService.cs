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

        /// <summary>Adds the specified model.</summary>
        /// <param name="model">The model.</param>
        public void Add(KnowledgeBaseViewModel article)
        {
            var newArticle = new KnowledgeBaseArticle();
            _mapper.Map(article, newArticle);
            newArticle.CreatedDate = DateTime.Now;
            newArticle.UpdatedDate = null;

            _knowledgeBaseRepository.Add(newArticle);
        }

        /// <summary>Updates the specified model.</summary>
        /// <param name="model">The model.</param>
        public void Update(KnowledgeBaseViewModel article)
        {
            var existingArticle = _knowledgeBaseRepository.FindArticleById(article.ArticleId);
            _mapper.Map(article, existingArticle);
            existingArticle.UpdatedDate = DateTime.Now;

            _knowledgeBaseRepository.Update(existingArticle);
        }

        /// <summary>Deletes the specified identifier.</summary>
        /// <param name="id">The identifier.</param>
        public void Delete(string id)
        {
            _knowledgeBaseRepository.Delete(id);
        }

        public KnowledgeBaseViewModel GetArticleById(string id)
        {
            var article = _knowledgeBaseRepository.FindArticleById(id);
            return _mapper.Map<KnowledgeBaseViewModel>(article);
        }

        public IEnumerable<KnowledgeBaseViewModel> RetrieveAll()
        {
            var articles = _knowledgeBaseRepository.RetrieveAll();
            return _mapper.Map<IEnumerable<KnowledgeBaseViewModel>>(articles);
        }

        public IEnumerable<ArticleCategory> GetArticleCategories()
        {
            return _knowledgeBaseRepository.GetArticleCategories();
        }

        public IEnumerable<KnowledgeBaseViewModel> SearchArticles(string searchTerm, List<string> selectedCategories)
        {
            var articles = _knowledgeBaseRepository.SearchArticles(searchTerm, selectedCategories);
            return _mapper.Map<IEnumerable<KnowledgeBaseViewModel>>(articles);
        }
    }
}
