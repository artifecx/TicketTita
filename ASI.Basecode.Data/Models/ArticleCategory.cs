using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class ArticleCategory
    {
        public ArticleCategory()
        {
            KnowledgeBaseArticles = new HashSet<KnowledgeBaseArticle>();
        }

        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }

        public virtual ICollection<KnowledgeBaseArticle> KnowledgeBaseArticles { get; set; }
    }
}
