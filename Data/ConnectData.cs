using be_dotnet_ecommerce1.Model;
using Microsoft.EntityFrameworkCore;

namespace be_dotnet_ecommerce1.Data
{
    public class ConnectData : DbContext
    {
        public ConnectData() { }
        public ConnectData(DbContextOptions<ConnectData> options) : base(options) { }
        public DbSet<Category> categories { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("category");
                entity.HasKey(e => e._id);
            });
        }

    }
}