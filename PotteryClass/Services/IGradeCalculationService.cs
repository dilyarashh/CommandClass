using PotteryClass.Data.DTOs;

namespace PotteryClass.Services;

public interface IGradeCalculationService
{
    GradeCalculationResultDto Calculate(GradeCalculationRequest request);
}
