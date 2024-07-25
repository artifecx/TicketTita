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
        /// <summary>
        /// Gets the article by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public KnowledgeBaseViewModel GetArticleById(string id)
        {
            var article = _knowledgeBaseRepository.FindArticleById(id);
            return _mapper.Map<KnowledgeBaseViewModel>(article);
        }
        /// <summary>
        /// Retrieves all.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<KnowledgeBaseViewModel> RetrieveAll()
        {
            var articles = _knowledgeBaseRepository.RetrieveAll();
            return _mapper.Map<IEnumerable<KnowledgeBaseViewModel>>(articles);
        }
        /// <summary>
        /// Gets the article categories.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ArticleCategory> GetArticleCategories()
        {
            return _knowledgeBaseRepository.GetArticleCategories();
        }
        /// <summary>
        /// Searches the articles.
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        /// <param name="selectedCategories">The selected categories.</param>
        /// <param name="sortBy">The sort by.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns></returns>
        public IEnumerable<KnowledgeBaseViewModel> SearchArticles(string searchTerm, string selectedCategories, string sortBy, string sortOrder, int pageNumber, int pageSize)
        {
            var articles = _knowledgeBaseRepository.SearchArticles(searchTerm, selectedCategories, sortBy, sortOrder, pageNumber, pageSize);
            return _mapper.Map<IEnumerable<KnowledgeBaseViewModel>>(articles);
        }
        /// <summary>
        /// Counts the articles.
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        /// <param name="selectedCategories">The selected categories.</param>
        /// <returns></returns>
        public int CountArticles(string searchTerm, string selectedCategories)
        {
            return _knowledgeBaseRepository.CountArticles(searchTerm, selectedCategories);
        }
    }
}
