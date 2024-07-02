using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Models
{
    public partial class KnowledgeBaseArticle
    {
        public string ArticleId { get; set; }
        public string CategoryId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string AuthorId { get; set; }

        public virtual User Author { get; set; }
        public virtual ArticleCategory Category { get; set; }
    }
}
