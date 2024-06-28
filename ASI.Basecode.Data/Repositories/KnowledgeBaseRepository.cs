using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    public class KnowledgeBaseRepository : IKnowledgeBaseRepository
    {
        private readonly List<KnowledgeBaseArticle> _articles = new List<KnowledgeBaseArticle>();
        private readonly List<ArticleCategory> _categories = new List<ArticleCategory>
        {
            new ArticleCategory { CategoryId = 1, CategoryName = "Getting Started", Description = "Articles on how to get started" },
            new ArticleCategory { CategoryId = 2, CategoryName = "Troubleshooting", Description = "Articles on troubleshooting" },
            new ArticleCategory { CategoryId = 3, CategoryName = "Product Features", Description = "Articles on features of a product" },
            new ArticleCategory { CategoryId = 4, CategoryName = "How-to Guides", Description = "Articles on processes" },
            new ArticleCategory { CategoryId = 5, CategoryName = "FAQs", Description = "Articles on frequently asked questions" },
            new ArticleCategory { CategoryId = 6, CategoryName = "Best Practices", Description = "Articles on the best practices" },
            new ArticleCategory { CategoryId = 7, CategoryName = "Release Notes", Description = "Articles on release notes"},
            new ArticleCategory { CategoryId = 8, CategoryName = "Policies and Procedures", Description = "Articles on policies and procedures" },
            new ArticleCategory { CategoryId = 9, CategoryName = "Technical Documentation", Description = "Articles on technical documentations" },
            new ArticleCategory { CategoryId = 10, CategoryName = "Account Management", Description = "Articles on the management of accounts" },
        };

        /// <summary>Retrieves all.</summary>
        /// <returns>
        ///   <br />
        /// </returns>
        public IEnumerable<KnowledgeBaseArticle> RetrieveAll() { return _articles; }


        /// <summary>Adds the specified model.</summary>
        /// <param name="article">The model.</param>
        public void Add(KnowledgeBaseArticle article)
        {
            AssignArticleProperties(article);
            _articles.Add(article);
        }


        /// <summary>Updates the specified model.</summary>
        /// <param name="model">The model.</param>
        public void Update(KnowledgeBaseArticle model)
        {
            var existingData = FindArticleById(model.ArticleId);
            if (existingData != null)
            {
                existingData = model;
            }
        }


        /// <summary>Deletes the specified identifier.</summary>
        /// <param name="id">The identifier.</param>
        public void Delete(string id)
        {
            var existingData = FindArticleById(id);
            if (existingData != null)
            {
                _articles.Remove(existingData);
            }
        }

        public KnowledgeBaseArticle FindArticleById(string id)
        {
            return _articles.Where(x => x.ArticleId.Equals(id)).FirstOrDefault();
        }

        public ArticleCategory FindArticleCategoryById(int id)
        {
            return _categories.Where(x => x.CategoryId.Equals(id)).FirstOrDefault();
        }

        public IEnumerable<ArticleCategory> GetArticleCategories()
        {
            return _categories;
        }

        #region Assign Article Properties
        private void AssignArticleProperties(KnowledgeBaseArticle article)
        {
            int categoryId = article.CategoryId;

            var lastArticleInCategory = _articles.LastOrDefault(t => t.CategoryId == categoryId);
            int categoryArticleCount = lastArticleInCategory == null ? 1 : lastArticleInCategory.NumberInCategory + 1;

            int totalArticleCount = _articles.Count + 1;

            article.ArticleId = $"{categoryId:00}-{categoryArticleCount:00}-{totalArticleCount:00}";

            lastArticleInCategory = _articles.LastOrDefault(t => t.CategoryId == categoryId);
            article.NumberInCategory = lastArticleInCategory == null ? 1 : lastArticleInCategory.NumberInCategory + 1;

            article.ArticleCategory = _categories.Single(x => x.CategoryId == categoryId);
        }
        #endregion
    }
}
