using Microsoft.EntityFrameworkCore;
using Presentation.Models;

namespace Presentation.Data;

public class EventDataContext(DbContextOptions<EventDataContext> options) : DbContext(options)
{
    public DbSet<Event> Events { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}
