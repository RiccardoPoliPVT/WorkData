using Microsoft.EntityFrameworkCore;
using WorkData.Models;

namespace WorkData.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Declaration> Declarations { get; set; }
    }
}
