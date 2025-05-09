using System;
using System.ComponentModel.DataAnnotations;
using TechXpress.Data.Models;

namespace TechXpress.Web.ViewModels
{
    public class ReviewViewModel
    {
        public int? Id { get; set; }

        [Required]
        public int ProductId { get; set; }
        public ProductViewModel? Product { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser? User { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; } = 0;

        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string Comment { get; set; }= string.Empty;

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }
    }
}
