using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class AdminRepository : BaseRepository, IAdminRepository
    {
        public AdminRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        /// <summary>
        /// Adds the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        public void Add(Admin model)
        {
            this.GetDbSet<Admin>().Add(model);
            UnitOfWork.SaveChanges();
        }
        /// <summary>
        /// Updates the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        public void Update(Admin model)
        {
            this.GetDbSet<Admin>().Update(model);
            UnitOfWork.SaveChanges();
        }
        /// <summary>
        /// Deletes the specified admin identifier.
        /// </summary>
        /// <param name="adminId">The admin identifier.</param>
        public void Delete(string adminId)
        {
            var adminToDelete = this.GetDbSet<Admin>().FirstOrDefault(a => a.AdminId == adminId);
            if (adminToDelete != null)
            {
                this.GetDbSet<Admin>().Remove(adminToDelete);
                UnitOfWork.SaveChanges();
            }
        }
        /// <summary>
        /// Finds the by identifier.
        /// </summary>
        /// <param name="adminId">The admin identifier.</param>
        /// <returns></returns>
        public Admin FindById(string adminId)
        {
            return this.GetDbSet<Admin>().FirstOrDefault(a => a.AdminId == adminId);
        }
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns></returns>
        public IQueryable<Admin> GetAll()
        {
            return this.GetDbSet<Admin>();
        }
        /// <summary>
        /// Determines whether [is super admin] [the specified admin identifier].
        /// </summary>
        /// <param name="adminId">The admin identifier.</param>
        /// <returns>
        ///   <c>true</c> if [is super admin] [the specified admin identifier]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsSuperAdmin(string adminId)
        {
            return this.GetDbSet<Admin>().Any(a => a.AdminId == adminId && a.IsSuper == true);
        }

        public string GetSuperAdminId()
        {
            return this.GetDbSet<Admin>().Where(a => a.IsSuper == true).Select(a => a.AdminId).FirstOrDefault();
        }   
    }
}
