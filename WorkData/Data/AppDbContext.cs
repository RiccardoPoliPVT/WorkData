using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WorkData.Models;

namespace WorkData.Data
{
    public class AppDbContext : IdentityDbContext<SampleUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        #region DbSet
        public DbSet<Declaration> Declarations { get; set; }
        #endregion
    }
}
