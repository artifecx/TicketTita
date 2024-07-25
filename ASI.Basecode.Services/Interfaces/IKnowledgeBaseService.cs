using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IKnowledgeBaseService
    {
        public void Add(KnowledgeBaseViewModel article);
        public void Update(KnowledgeBaseViewModel article);
        public void Delete(string id);
        public KnowledgeBaseViewModel GetArticleById(string id);
        public IEnumerable<KnowledgeBaseViewModel> RetrieveAll();
        public IEnumerable<ArticleCategory> GetArticleCategories();
        public IEnumerable<KnowledgeBaseViewModel> SearchArticles(string searchTerm, string selectedCategories, string sortBy, string sortOrder, int pageNumber, int pageSize);
        public int CountArticles(string searchTerm, string selectedCategories);
    }
}
