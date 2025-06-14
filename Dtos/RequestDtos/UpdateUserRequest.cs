using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Mvc;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class UpdateUserRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; } = "";

        [EmailAddress]
        public string? Email { get; set; }

        public bool IsActive { get; set; }

      
        public int? CompanyId { get; set; }

        public string? Phone { get; set; }

        public string? Identifier { get; set; }

        [FromForm(Name = "imageFile")]
        public IFormFile? ImageFile { get; set; }

        public string? ImageUrl { get; set; }

        public List<int>? RoleIds { get; set; }
    }
}