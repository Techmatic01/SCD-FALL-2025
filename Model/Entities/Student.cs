using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFrameWorkExample.Model.Entities
{
    [Table("Students")]
    public class Student
    {
        [Column("id")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("age")]
        public int Age { get; set; }

        // Navigation Property
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
