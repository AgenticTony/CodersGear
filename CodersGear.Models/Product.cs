using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CodersGear.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        [Required]
        [Display(Name = "Product Title")]
        public string? ProductName { get; set; }
        [Required]
        public string? Description { get; set; }
        [Required]
        public string? UPC { get; set; }
        [Required]
        public int? CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }
        [Required]
        [Display(Name ="List Price")]
        [Range(1, 1000, ErrorMessage = "List Price must be between 1-1000")]
        public decimal ListPrice { get; set; }

        [Required]
        [Display(Name = "Price for 1-50")]
        [Range(1, 1000, ErrorMessage = "Price must be between 1-1000")]
        public decimal Price { get; set; }

        [Required]
        [Display(Name = "Price for 50+")]
        [Range(1, 1000, ErrorMessage = "Price must be between 1-1000")]
        public decimal Price50 { get; set; }

        [Required]
        [Display(Name = "Price for 100+")]
        [Range(1, 1000, ErrorMessage = "Price must be between 1-1000")]
        public decimal Price100 { get; set; }

        [DisplayName("Image URL")]
        public string? ImageUrl { get; set; }

        // Store additional images as JSON array for Printify products
        public string? AdditionalImages { get; set; }

        // Store product options (Size, Color, etc.) as JSON for Printify products
        public string? PrintifyOptionsData { get; set; }

        // Printify Integration Fields
        public bool IsPrintifyProduct { get; set; } = false;
        public string? PrintifyProductId { get; set; }
        public string? PrintifyShopId { get; set; }
        public string? PrintifyVariantData { get; set; }  // JSON string containing variant info
        public DateTime? LastSyncedAt { get; set; }

        // Visibility control (used for Printify products that have been unpublished)
        public bool Visible { get; set; } = true;

    }
}
