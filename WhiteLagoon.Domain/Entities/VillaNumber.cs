using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WhiteLagoon.Domain.Entities
{
    public class VillaNumber
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "Villa Number")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Villa Number must be an integer")]

        public int Villa_Number { get; set; }

        [ForeignKey("villa")]
        public int VillaId { get; set; }
        public string? SpecialDetails { get; set; }

        //navgations properties
        [ValidateNever]
        public Villa villa { get; set; }
    }
}
