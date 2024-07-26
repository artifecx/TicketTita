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
        [Required(ErrorMessage = "Category is required.")]
        public string CategoryId { get; set; }
        
        [StringLength(100)]
        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }

        
        [StringLength(800)]
        [Required(ErrorMessage = "Content is required.")]
        public string Content { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
        
        [StringLength(256)]
        [Required]
        public string AuthorId { get; set; }
        public ArticleCategory Category { get; set; }
        public User Author { get; set; }

        // public IEnumerable<KnowledgeBaseViewModel> Articles { get; set; }
        public IEnumerable<ArticleCategory> ArticleCategories { get; set; }
    }
}
