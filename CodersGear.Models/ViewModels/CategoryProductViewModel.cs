using CodersGear.Models;

namespace CodersGear.Models.ViewModels
{
    public class CategoryProductViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int DisplayOrder { get; set; }
        public IEnumerable<Product> Products { get; set; } = new List<Product>();
    }
}
