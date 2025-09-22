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
      modelBuilder.Entity<Account>(entity =>
        {
        entity.ToTable("account");
        entity.HasKey(e => e._id);

        entity.Property(e => e._id).HasColumnName("_id");
        entity.Property(e => e.email).HasColumnName("email");
        entity.Property(e => e.password).HasColumnName("password");
        entity.Property(e => e.first_name).HasColumnName("first_name");
        entity.Property(e => e.last_name).HasColumnName("last_name");
        entity.Property(e => e.rule).HasColumnName("rule");
        entity.Property(e => e.avatar_img).HasColumnName("avatar_img");
        entity.Property(e => e.refresh_token).HasColumnName("refresh_token");
        entity.Property(e => e.refresh_token_expires).HasColumnName("refresh_token_expires");
        });

    }

  }
}