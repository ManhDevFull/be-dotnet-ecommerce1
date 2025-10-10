using System.Runtime.Intrinsics.Arm;
using be_dotnet_ecommerce1.Model;
using dotnet.Model;
using Microsoft.EntityFrameworkCore;

namespace be_dotnet_ecommerce1.Data
{
  public class ConnectData : DbContext
  {
    public ConnectData() { }
    public ConnectData(DbContextOptions<ConnectData> options) : base(options) { }
    public DbSet<Category> categories { get; set; }
    // public DbSet<CategoryAdmin> categorieAdmin { get; set; }
    public DbSet<CategoryAdmin> CategoryAdmins { get; set; }
    public DbSet<Account> accounts { get; set; }
    public DbSet<Product> products { get; set; }
    public DbSet<Variant> variants { get; set; }
    public DbSet<Discount> discounts { get; set; }
    public DbSet<Order> orders { get; set; }
    public DbSet<DiscountProduct> discountProducts { get; set; }
    public DbSet<Review> reviews { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Category>(entity =>
      {
        entity.ToTable("category");
        entity.HasKey(e => e.id);
        entity.Property(e => e.id).HasColumnName("id");
        entity.Property(e => e.namecategory).HasColumnName("namecategory");
        entity.Property(e => e.idparent).HasColumnName("parent_id");
        entity.HasMany(c => c.Products).WithOne(p => p.Category).HasForeignKey(p => p.categoryId);
      });
      modelBuilder.Entity<Account>(entity =>
        {
          entity.ToTable("account");
          entity.HasKey(e => e.id);

          entity.Property(e => e.id).HasColumnName("id");
          entity.Property(e => e.email).HasColumnName("email");
          entity.Property(e => e.password).HasColumnName("password");
          entity.Property(e => e.firstname).HasColumnName("firstname");
          entity.Property(e => e.lastname).HasColumnName("lastname");
          entity.Property(e => e.role).HasColumnName("role");
          entity.Property(e => e.avatarimg).HasColumnName("avatarimg");
          entity.Property(e => e.refreshtoken).HasColumnName("refreshtoken");
          entity.Property(e => e.refreshtokenexpires).HasColumnName("refreshtokenexpires");
        });
      modelBuilder.Entity<Product>(entity =>
               {
                 entity.ToTable("product");
                 entity.HasKey(e => e.id);

                 entity.Property(e => e.id).HasColumnName("id");
                 entity.Property(e => e.nameproduct).HasColumnName("nameproduct");
                 entity.Property(e => e.brand).HasColumnName("brand");
                 entity.Property(e => e.description).HasColumnName("description");
                 entity.Property(e => e.categoryId).HasColumnName("category");
                 entity.Property(e => e.imageurls)
                    .HasColumnName("imageurls")
                    .HasColumnType("text[]");
                 entity.Property(e => e.createdate).HasColumnName("createdate");
                 entity.Property(e => e.updatedate).HasColumnName("updatedate");
                 entity.Property(e => e.isdeleted).HasColumnName("isdeleted");
                 //          entity.HasOne(p => p.Category)
                 // .WithMany(c => c.Products)
                 // .HasForeignKey(p => p.category)
                 // .HasConstraintName("fk_product_category");
               });
      //variant
      modelBuilder.Entity<Variant>(entity =>
      {
        entity.ToTable("variant");
        entity.HasKey(e => e.id);
        entity.Property(e => e.id).HasColumnName("id");
        entity.Property(e => e.productid).HasColumnName("product_id");
        entity.Property(e => e.valuevariant).HasColumnName("valuevariant").HasColumnType("JSONB");
        entity.Property(e => e.stock).HasColumnName("stock");
        entity.Property(e => e.inputprice).HasColumnName("inputprice");
        entity.HasOne(v => v.Product).WithMany(p => p.Variants).HasForeignKey(v=>v.productid);
        entity.HasMany(v => v.discountProduct).WithOne(dp => dp.variant).HasForeignKey("_idVariant");
        entity.HasMany(v => v.Orders).WithOne(o => o.variant).HasForeignKey("variant_id");
      });
      //category admin
      modelBuilder.Entity<CategoryAdmin>(entity =>
      {
        entity.HasNoKey();
        entity.ToView(null);
        entity.Property(e => e.id).HasColumnName("id");
        entity.Property(e => e.namecategory).HasColumnName("namecategory");
        entity.Property(e => e.idparent).HasColumnName("idparent");
        entity.Property(e => e.product).HasColumnName("product");
      });
      // discount
      modelBuilder.Entity<Discount>(entity =>
      {
        entity.ToTable("discount");
        entity.HasKey(e => e.id);
        entity.Property(e => e.typediscount).HasColumnName("typediscount");
      });
      //discountProduct
      modelBuilder.Entity<DiscountProduct>(entity =>
      {
        entity.ToTable("discount_product");
        entity.HasKey("id");
        entity.Property(e => e.discountid).HasColumnName("discount_id");
        entity.Property(e => e.variantid).HasColumnName("variant_id");
        entity.HasOne(dp => dp.discount).WithMany(d => d.discountProducts).HasForeignKey("discount_id");
        entity.HasOne(dp => dp.variant).WithMany(v => v.discountProduct).HasForeignKey("variant_id");
      });
      //order
      modelBuilder.Entity<Order>(entity =>
      {
        entity.HasKey("id");
        entity.Property(e => e.accountid).HasColumnName("account_id");
        entity.Property(e => e.variantid).HasColumnName("variant_id");
        entity.Property(e => e.addressid).HasColumnName("address_id");
        entity.HasOne(o => o.review).WithOne(r => r.order).HasForeignKey<Review>("order_id");
      });
      // review
      modelBuilder.Entity<Review>(entity =>
      {
        entity.ToTable("review");
        entity.HasKey("id");
        entity.Property(e => e.orderid).HasColumnName("order_id");
      });
    }
  }
}