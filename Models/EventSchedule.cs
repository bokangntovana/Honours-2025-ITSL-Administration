namespace ITSL_Administration.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class EventSchedule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } // Unique identifier

        [Required(ErrorMessage = "Event title is required")]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters")]
        [Display(Name = "Event Title")]
        public string? Title { get; set; } // Event title

        [Required(ErrorMessage = "Start date and time is required")]
        [Display(Name = "Start Date/Time")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime Start { get; set; } // Start date and time

        [Display(Name = "End Date/Time")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? End { get; set; } // Optional end date and time

        [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters")]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; } // Detailed description

        [Display(Name = "Background Color")]
        [RegularExpression("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Invalid color format. Use hex format like #RRGGBB")]
        public string? BackgroundColor { get; set; } // Custom event color

        [Display(Name = "All Day Event")]
        public bool IsAllDay { get; set; } // Indicates if the event spans the entire day
    }
}
