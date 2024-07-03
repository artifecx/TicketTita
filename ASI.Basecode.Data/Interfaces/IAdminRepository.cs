using ASI.Basecode.Data.Models;
using System.Linq;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IAdminRepository
    {
        void Add(Admin model);
        void Update(Admin model);
        void Delete(string adminId);
        Admin FindById(string adminId);
        IQueryable<Admin> GetAll();
    }
}
