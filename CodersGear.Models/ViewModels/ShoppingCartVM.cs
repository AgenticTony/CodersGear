using System.Collections.Generic;
using System.Text;
using CodersGear.Models;

namespace CodersGear.Models.ViewModels
{
    public class ShoppingCartVM
    {
        public IEnumerable<ShoppingCart> ShoppingCartList { get; set; }
        public OrderHeader OrderHeader { get; set; }
    }
}
