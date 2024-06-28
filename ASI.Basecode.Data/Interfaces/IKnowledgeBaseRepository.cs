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
        IEnumerable<KnowledgeBaseArticle> RetrieveAll();
        void Add(KnowledgeBaseArticle model);
        void Update(KnowledgeBaseArticle model);
        void Delete(string id);
        public KnowledgeBaseArticle FindArticleById(string id);
        public ArticleCategory FindArticleCategoryById(int id);
        public IEnumerable<ArticleCategory> GetArticleCategories();
    }
}
