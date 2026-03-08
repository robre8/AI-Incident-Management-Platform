using IncidentPlatform.Domain.Entities;

namespace IncidentPlatform.Application.Interfaces;

public interface IIncidentRepository
{
    Task AddAsync(Incident incident);
    Task<IEnumerable<Incident>> GetAllAsync();
    Task<Incident?> GetByIdAsync(Guid id);
    Task UpdateAsync(Incident incident);
    Task DeleteAsync(Incident incident);
}
