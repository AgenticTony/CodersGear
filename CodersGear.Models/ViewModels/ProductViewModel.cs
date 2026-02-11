using System.ComponentModel.DataAnnotations;

namespace CodersGear.Models.ViewModels
{
    /// <summary>
    /// ViewModel for Product Create/Edit operations.
    /// Combines the Product entity with additional data needed for the view.
    /// </summary>
    public class ProductViewModel
    {
        /// <summary>
        /// The product being created or edited
        /// </summary>
        public Product? Product { get; set; }

        /// <summary>
        /// Categories as a dictionary for dropdown (Key = CategoryId, Value = CategoryName)
        /// This keeps the Models project independent of ASP.NET Core
        /// </summary>
        public Dictionary<int, string> CategoryDictionary { get; set; } = new Dictionary<int, string>();

        // ========== Validation-only properties (not in Product model) ==========

        /// <summary>
        /// Selected category ID from dropdown (used for validation/display)
        /// This is a separate property because Product.CategoryId might be null
        /// </summary>
        [Display(Name = "Category")]
        public int? SelectedCategoryId { get; set; }
    }
}
