using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Models
{
    public class KnowledgeBaseArticle
    {
        public string ArticleId { get; set; }
        public int CategoryId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string AuthorId { get; set; }
        public int NumberInCategory { get; set; }
        public ArticleCategory ArticleCategory { get; set; }
        public User Author { get; set; }
    }

    public class ArticleCategory
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public virtual ICollection<KnowledgeBaseArticle> KnowledgeBaseArticles { get; set; }
    }
}
