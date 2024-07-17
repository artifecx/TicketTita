using System;
using System.Collections.Generic;

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
        public bool IsDeleted { get; set; }

        public virtual User Author { get; set; }
        public virtual ArticleCategory Category { get; set; }
    }
}
