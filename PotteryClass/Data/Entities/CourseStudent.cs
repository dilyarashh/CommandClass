namespace PotteryClass.Data.Entities
{
    public class CourseStudent
    {
        public Guid CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public bool IsBlocked { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
