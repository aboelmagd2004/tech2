using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechXpress.Data.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(100)]
        public string Slug { get; set; }

        [StringLength(255)]
        public string? ImageUrl { get; set; }

        //Product Count 
        [NotMapped]
        public int ProductCount { set; get; } = 0;

        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public virtual Category? ParentCategory { get; set; }

        public virtual ICollection<Category> ChildCategories { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(70)]
        public string MetaTitle { get; set; }

        [StringLength(160)]
        public string? MetaDescription { get; set; }

        [StringLength(500)]
        public string? MetaKeywords { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        public int Level { get; set; } = 0;

        public virtual ICollection<ProductViewModel> Products { get; set; }

        // Constructor to initialize collections
        public Category()
        {
            ChildCategories = new HashSet<Category>();
            Products = new HashSet<ProductViewModel>();
        }

        public static implicit operator Category(string v)
        {
            throw new NotImplementedException();
        }
    }
}