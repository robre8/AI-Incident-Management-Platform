using IncidentPlatform.Application.Interfaces;
using IncidentPlatform.Domain.Entities;
using IncidentPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IncidentPlatform.Infrastructure.Repositories;

public class IncidentRepository : IIncidentRepository
{
    private readonly IncidentDbContext _context;

    public IncidentRepository(IncidentDbContext context) => _context = context;

    public async Task AddAsync(Incident incident)
    {
        _context.Incidents.Add(incident);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Incident>> GetAllAsync() =>
        await _context.Incidents.ToListAsync();

    public async Task<Incident?> GetByIdAsync(Guid id) =>
        await _context.Incidents.FindAsync(id);

    public async Task UpdateAsync(Incident incident)
    {
        _context.Incidents.Update(incident);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Incident incident)
    {
        _context.Incidents.Remove(incident);
        await _context.SaveChangesAsync();
    }
}
