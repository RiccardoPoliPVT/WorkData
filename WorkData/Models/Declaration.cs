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
        [DisplayName("Ore Ordinarie")]
        [Range(0, 10)]
        public float OrdinalHours { get; set; }
        [DisplayName("Ore Straordinarie")]
        [Range(0, 10)]

        public float ExtraHours { get; set; } = 0;
        [DisplayName("Ore malattia")]
        [Range(0, 8)]

        public float SickHours { get; set; }
        [DisplayName("Ore Ferie")]
        [Range(0, 8)]

        public float HolidayHours { get; set; }
        [DisplayName("Ore Permessi")]
        [Range(0, 8)]

        public float PermissionHours { get; set; }
    }
}
