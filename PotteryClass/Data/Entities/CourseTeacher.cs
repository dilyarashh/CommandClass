namespace PotteryClass.Data.Entities
{
    public class CourseTeacher
    {
        public Guid CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime CreatedAtUtc { get; set; }
    }
}
