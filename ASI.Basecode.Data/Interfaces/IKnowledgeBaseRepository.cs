using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IKnowledgeBaseRepository
    {
        IQueryable<KnowledgeBaseArticle> RetrieveAll();
        string Add(KnowledgeBaseArticle article);
        string Update(KnowledgeBaseArticle article);
        void Delete(string id);
        KnowledgeBaseArticle FindArticleById(string id);
        ArticleCategory FindArticleCategoryById(string id);
        IQueryable<ArticleCategory> GetArticleCategories();
        public User FindUserById(string id);
        IQueryable<KnowledgeBaseArticle> SearchArticles(string searchTerm, string selectedCategories, string sortBy, string sortOrder, int pageNumber, int pageSize);
        int CountArticles(string searchTerm, string selectedCategories);
    }
}
