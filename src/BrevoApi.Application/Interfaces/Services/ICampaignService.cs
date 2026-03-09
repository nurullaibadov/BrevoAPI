using BrevoApi.Application.Common;
using BrevoApi.Application.DTOs.Campaign;

namespace BrevoApi.Application.Interfaces.Services;

public interface ICampaignService
{
    Task<CampaignDto?> GetByIdAsync(int id);
    Task<PagedResult<CampaignDto>> GetAllAsync(PaginationParams pagination);
    Task<CampaignDto> CreateAsync(CreateCampaignDto dto, int userId);
    Task<CampaignDto> UpdateAsync(int id, UpdateCampaignDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> SendAsync(int id);
    Task<bool> ScheduleAsync(int id, DateTime scheduledAt);
    Task<bool> PauseAsync(int id);
    Task<bool> CancelAsync(int id);
    Task<CampaignStatsDto> GetStatsAsync(int id);
    Task SyncStatsFromBrevoAsync(int id);
}
