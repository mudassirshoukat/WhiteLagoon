using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WhiteLagoon.Domain.Entities
{
    public class Villa
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public required string Name { get; set; }
        public string? Description { get; set; }

        [Display(Name = "Price Per Night")]
        [Range(10, 10000)]
        public double Price { get; set; }
        public int SqFt { get; set; }

        [Range(0, 10)]
        public int Occupancy { get; set; }

        [NotMapped]
        public IFormFile? Image { get; set; }

        [Display(Name = "Image Url")]
        public string? ImageUrl { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        [NotMapped]
        public bool IsAvailable { get; set; } = true;

        //Navigation Properties
        [ValidateNever]
        public IEnumerable<Amenity>? VillaAmenity { get; set; }
    }
}
