using Microsoft.EntityFrameworkCore;
using IncidentPlatform.Domain.Entities;

namespace IncidentPlatform.Infrastructure.Data;

public class IncidentDbContext : DbContext
{
    public IncidentDbContext(DbContextOptions<IncidentDbContext> options)
        : base(options)
    {
    }

    public DbSet<Incident> Incidents { get; set; }
}