namespace PotteryClass.Data.DTOs;

public class CreateGradeRequest
{
    public Guid StudentId { get; set; }
    public int Value { get; set; }
}