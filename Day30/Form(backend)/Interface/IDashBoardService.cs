using Form.DTOs;
namespace Form.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync();
    Task<MyDashboardDto> GetMyDashboardAsync(Guid userId);
}