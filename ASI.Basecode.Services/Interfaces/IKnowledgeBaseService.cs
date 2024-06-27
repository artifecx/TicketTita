using ASI.Basecode.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IKnowledgeBaseService
    {
        IEnumerable<KnowledgeBaseViewModel> RetrieveAll();
        void Add(KnowledgeBaseViewModel model);
        void Update(KnowledgeBaseViewModel model);
        void Delete(int id);
    }
}
