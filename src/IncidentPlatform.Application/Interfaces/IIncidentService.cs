using IncidentPlatform.Domain.Entities;

namespace IncidentPlatform.Application.Interfaces;

public interface IIncidentService
{
    Task<Incident> CreateIncidentAsync(IncidentDTO dto);
    Task<IEnumerable<Incident>> GetAllIncidentsAsync();
    Task<Incident?> GetIncidentByIdAsync(Guid id);
    Task<Incident?> UpdateIncidentAsync(Guid id, IncidentDTO dto);
    Task<bool> DeleteIncidentAsync(Guid id);
}
