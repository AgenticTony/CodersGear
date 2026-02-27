using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace CodersGear.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public int? CompanyId { get; set; }
        public string? StripeCustomerId { get; set; }
    }
}
