using Microsoft.EntityFrameworkCore;
using OnionArchitecture.Domain;

namespace Onion.Data
{
    public class ApplicationDbContext : DbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
          : base(options) { }


        public DbSet<User> Users { get; set; }
    }
}
