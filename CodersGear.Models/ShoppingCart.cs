using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CodersGear.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product? Product { get; set; }
        [Range(1, 1000, ErrorMessage = "Count must be at least 1")]
        public int Count { get; set; }
        [NotMapped]
        public decimal Price { get; set; }
        // For guest carts - either ApplicationUserId OR SessionId will be populated (not both)
        public string? ApplicationUserId { get; set; }
        public string? SessionId { get; set; }
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser? ApplicationUser { get; set; }
        public decimal OrderTotal { get; set; }



    }
}
