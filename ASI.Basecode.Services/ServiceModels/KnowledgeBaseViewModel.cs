using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Services.ServiceModels
{
    public class KnowledgeBaseViewModel
    {
        [StringLength(256)]
        public string ArticleId { get; set; }
        [Required]
        public int CategoryId { get; set; }
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
        [Required]
        [StringLength(256)]
        public string AuthorId { get; set; }
        public ArticleCategory ArticleCategory { get; set; }
        public User Author { get; set; }

        // public IEnumerable<KnowledgeBaseViewModel> Articles { get; set; }
        public IEnumerable<ArticleCategory> ArticleCategories { get; set; }
    }
}
