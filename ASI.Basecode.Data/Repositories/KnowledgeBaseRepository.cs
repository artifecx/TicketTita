using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    public class KnowledgeBaseRepository : BaseRepository, IKnowledgeBaseRepository
    {
        /*private readonly List<KnowledgeBaseArticle> _articles = new List<KnowledgeBaseArticle>();*/
        private readonly List<ArticleCategory> _categories;
        /*{
            new ArticleCategory { CategoryId = "1", CategoryName = "Getting Started", Description = "Articles on how to get started" },
            new ArticleCategory { CategoryId = "2", CategoryName = "Troubleshooting", Description = "Articles on troubleshooting" },
            new ArticleCategory { CategoryId = "3", CategoryName = "Product Features", Description = "Articles on features of a product" },
            new ArticleCategory { CategoryId = "4", CategoryName = "How-to Guides", Description = "Articles on processes" },
            new ArticleCategory { CategoryId = "5", CategoryName = "FAQs", Description = "Articles on frequently asked questions" },
            new ArticleCategory { CategoryId = "6", CategoryName = "Best Practices", Description = "Articles on the best practices" },
            new ArticleCategory { CategoryId = "7", CategoryName = "Release Notes", Description = "Articles on release notes"},
            new ArticleCategory { CategoryId = "8", CategoryName = "Policies and Procedures", Description = "Articles on policies and procedures" },
            new ArticleCategory { CategoryId = "9", CategoryName = "Technical Documentation", Description = "Articles on technical documentations" },
            new ArticleCategory { CategoryId = "10", CategoryName = "Account Management", Description = "Articles on the management of accounts" },
        };*/

        public KnowledgeBaseRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _categories = GetArticleCategories().ToList();
        }

        public IQueryable<KnowledgeBaseArticle> RetrieveAll()
        {
            var articles = this.GetDbSet<KnowledgeBaseArticle>();

            foreach (KnowledgeBaseArticle a in articles)
            {
                a.Category = _categories.Single(x => x.CategoryId == a.CategoryId);
                a.Author = FindUserById(a.AuthorId);
            }

            return articles;
        }

        /// <summary>Retrieves all.</summary>
        /// <returns>
        ///   <br />
        /// </returns>
        /*public IEnumerable<KnowledgeBaseArticle> RetrieveAll() { return _articles; }*/


        /// <summary>Adds the specified model.</summary>
        /// <param name="article">The model.</param>
        public string Add(KnowledgeBaseArticle article)
        {
            AssignArticleProperties(article);
            this.GetDbSet<KnowledgeBaseArticle>().Add(article);
            UnitOfWork.SaveChanges();

            return article.ArticleId;
        }


        /// <summary>Updates the specified model.</summary>
        /// <param name="model">The model.</param>
        public string Update(KnowledgeBaseArticle article)
        {
            SetNavigation(article);

            this.GetDbSet<KnowledgeBaseArticle>().Update(article);
            UnitOfWork.SaveChanges();

            return article.ArticleId;
        }


        /// <summary>Deletes the specified identifier.</summary>
        /// <param name="id">The identifier.</param>
        public void Delete(string id)
        {
            var existingData = FindArticleById(id);
            if (existingData != null)
            {
                this.GetDbSet<KnowledgeBaseArticle>().Remove(existingData);
                UnitOfWork.SaveChanges();
            }
        }

        public KnowledgeBaseArticle FindArticleById(string id)
        {
            var article = this.GetDbSet<KnowledgeBaseArticle>().Where(x => x.ArticleId.Equals(id)).FirstOrDefault();
            if (article != null)
            {
                article.Author = FindUserById(article.AuthorId);
            }
            return article;
        }

        public ArticleCategory FindArticleCategoryById(string id)
        {
            return this.GetDbSet<ArticleCategory>().Where(x => x.CategoryId.Equals(id)).FirstOrDefault();
        }

        public IQueryable<ArticleCategory> GetArticleCategories()
        {
            return this.GetDbSet<ArticleCategory>();
        }

        public User FindUserById(string id)
        {
            return this.GetDbSet<User>().Where(x => x.UserId.Equals(id)).FirstOrDefault();
        }

        public IQueryable<KnowledgeBaseArticle> SearchArticles(string searchTerm)
        {
            var articles = this.GetDbSet<KnowledgeBaseArticle>().AsQueryable();

            if(!string.IsNullOrEmpty(searchTerm))
            {
                articles = articles.Where(x => x.Title.Contains(searchTerm) || x.Content.Contains(searchTerm));
            }

            foreach (KnowledgeBaseArticle article in articles)
            {
                article.Category = _categories.Single(x => x.CategoryId == article.CategoryId);
                article.Author = FindUserById(article.AuthorId);
            }

            return articles;
        }

        #region Assign Article Properties
        private void AssignArticleProperties(KnowledgeBaseArticle article)
        {
            string categoryId = article.CategoryId;
            int categoryCount = RetrieveAll().Count(t => t.CategoryId == categoryId);
            int overallCount = RetrieveAll().Count();

            article.ArticleId = $"{categoryId:00}-{categoryCount:00}-{overallCount:00}";

            SetNavigation(article);
        }

        private void SetNavigation(KnowledgeBaseArticle article)
        {
            article.Category = _categories.Single(x => x.CategoryId == article.CategoryId);
            article.Author = FindUserById(article.AuthorId);
        }
        #endregion
    }
}
