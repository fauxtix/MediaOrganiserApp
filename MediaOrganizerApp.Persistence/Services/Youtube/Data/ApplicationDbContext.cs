using MediaOrganizerApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediaOrganizerApp.Persistence.Services.Youtube.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<YouTubeSubscription> VideoNotifications { get; set; }
    }
}
