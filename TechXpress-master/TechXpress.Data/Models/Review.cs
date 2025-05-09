using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechXpress.Data.Models
{
    public class Review
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }  // Changed from ReviewId to Id for consistency

        [Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }

        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; }  // Added user relationship

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string? Comment { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // Changed to UtcNow

        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }  // Added for tracking updates

        // Navigation properties
        public virtual ProductViewModel Product { get; set; }
        public virtual ApplicationUser User { get; set; }  // Added user navigation
    }
}