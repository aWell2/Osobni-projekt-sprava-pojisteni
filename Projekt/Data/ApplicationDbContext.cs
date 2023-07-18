using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Projekt.Models;

namespace Projekt.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Projekt.Models.Insurence> Insurence { get; set; } = default!;
        public DbSet<Projekt.Models.InsurenceHolder> InsurenceHolder { get; set; } = default!;
        public DbSet<Projekt.Models.InsurenceEvent> InsurenceEvent { get; set; } = default!;
    }
}