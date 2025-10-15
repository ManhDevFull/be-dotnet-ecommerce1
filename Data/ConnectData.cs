using System.Reflection.Emit;
using be_dotnet_ecommerce1.Model;
using dotnet.Dtos;
using Microsoft.EntityFrameworkCore;

namespace be_dotnet_ecommerce1.Data
{
  public class ConnectData : DbContext
  {
    public ConnectData() { }
    public ConnectData(DbContextOptions<ConnectData> options) : base(options) { }
    public DbSet<Category> categories { get; set; }
    // public DbSet<CategoryAdmin> categorieAdmin { get; set; }
    public DbSet<Account> accounts { get; set; }
    public DbSet<UserDTO> users { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Category>(entity =>
      {
        entity.ToTable("category");
        entity.HasKey(e => e._id);
        entity.Property(e => e._id).HasColumnName("id");
        entity.Property(e => e.name_category).HasColumnName("namecategory");
        entity.Property(e => e.parent_id).HasColumnName("parent_id");
      });
      modelBuilder.Entity<Account>(entity =>
        {
          entity.ToTable("account");
          entity.HasKey(e => e.id);
          entity.Property(e => e.id).HasColumnName("id");
          entity.Property(e => e.email).HasColumnName("email");
          entity.Property(e => e.password).HasColumnName("password");
          entity.Property(e => e.first_name).HasColumnName("firstname");
          entity.Property(e => e.last_name).HasColumnName("lastname");
          entity.Property(e => e.rule).HasColumnName("role");
          entity.Property(e => e.avatar_img).HasColumnName("avatarimg");
          entity.Property(e => e.refresh_token).HasColumnName("refreshtoken");
          entity.Property(e => e.refresh_token_expires).HasColumnName("refreshtokenexpires");
        });
 

    }
  }
}