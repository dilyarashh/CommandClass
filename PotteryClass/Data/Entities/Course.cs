namespace PotteryClass.Data.Entities
{
    public class Course
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public string Code { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public DateTime RegistrationStartsAtUtc { get; set; }
        public DateTime RegistrationEndsAtUtc { get; set; }

        public Guid CreatedByUserId { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public ICollection<CourseTeacher> Teachers { get; set; } = new List<CourseTeacher>();
        public ICollection<CourseStudent> Students { get; set; } = new List<CourseStudent>();
    }
}
