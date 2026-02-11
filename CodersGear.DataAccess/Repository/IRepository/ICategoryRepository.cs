using CodersGear.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodersGear.DataAccess.Repository.IRepository
{
    public interface ICategoryRepository : IRepository<Category>
    {
        void Update(Category obj);
    }
}
