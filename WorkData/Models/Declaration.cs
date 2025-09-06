using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WorkData.Models
{
    public class Declaration
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [DataType(DataType.Date)]
        //[DisplayFormat(DataFormatString = "{dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateOnly Date { get; set; }
        [Required]
        [DisplayName("Ordinal Hours")]
        [Range(0, 10)]
        public float OrdinalHours { get; set; }
        [DisplayName("Extra Hours")]
        [Range(0, 10)]

        public float ExtraHours { get; set; } = 0;
        [DisplayName("Sick Hours")]
        [Range(0, 8)]

        public float SickHours { get; set; }
        [DisplayName("Holiday")]
        [Range(0, 8)]

        public float HolidayHours { get; set; }
        [DisplayName("Permission Hours")]
        [Range(0, 8)]

        public float PermissionHours { get; set; }
    }
}
