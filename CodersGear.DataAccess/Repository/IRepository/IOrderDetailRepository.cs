using CodersGear.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodersGear.DataAccess.Repository.IRepository
{
    public interface IOrderDetailRepository : IRepository<OrderDetail>
    {
        void Update(OrderDetail obj);
    }
}
