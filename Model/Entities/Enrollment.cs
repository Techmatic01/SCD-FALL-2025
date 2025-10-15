using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFrameWorkExample.Model.Entities
{
    [Table("Enrollments")]
    public class Enrollment
    {
        [Column("id")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("student_id")]
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        [Column("course_id")]
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        [Column("grade")]
        public string? Grade { get; set; }
    }
}
