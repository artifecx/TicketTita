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
        private readonly List<KnowledgeBase> _data = new List<KnowledgeBase>();
        private int _nextId = 1;


        /// <summary>Retrieves all.</summary>
        /// <returns>
        ///   <br />
        /// </returns>
        public IEnumerable<KnowledgeBase> RetrieveAll() { return _data; }


        /// <summary>Adds the specified model.</summary>
        /// <param name="model">The model.</param>
        public void Add(KnowledgeBase model)
        {
            model.ArticleId = _nextId++;
            _data.Add(model);
        }


        /// <summary>Updates the specified model.</summary>
        /// <param name="model">The model.</param>
        public void Update(KnowledgeBase model)
        {
            var existingData = _data.Where(x => x.ArticleId == model.ArticleId).FirstOrDefault();
            if (existingData != null)
            {
                existingData = model;
            }
        }


        /// <summary>Deletes the specified identifier.</summary>
        /// <param name="id">The identifier.</param>
        public void Delete(int id)
        {
            var existingData = _data.Where(x => x.ArticleId == id).FirstOrDefault();
            if (existingData != null)
            {
                _data.Remove(existingData);
            }
        }
    }
}
