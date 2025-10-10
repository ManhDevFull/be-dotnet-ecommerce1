using System.Reflection.Emit;
using System.Runtime.Intrinsics.Arm;
using be_dotnet_ecommerce1.Model;
using dotnet.Dtos;
using dotnet.Dtos.admin;
using dotnet.Model;
using Microsoft.EntityFrameworkCore;

namespace be_dotnet_ecommerce1.Data
{
  public class ConnectData : DbContext
  {
    public ConnectData() { }
    public ConnectData(DbContextOptions<ConnectData> options) : base(options) { }
    public DbSet<Account> accounts { get; set; }
    public DbSet<Address> address { get; set; }
    public DbSet<Category> categories { get; set; }
    public DbSet<Discount> discounts { get; set; }
    public DbSet<DiscountProduct> discountProducts { get; set; }
    public DbSet<Order> orders { get; set; }
    public DbSet<Product> products { get; set; }
    public DbSet<Review> reviews { get; set; }
    public DbSet<ShoppingCart> shoppingCarts { get; set; }
    public DbSet<Variant> variants { get; set; }
    public DbSet<WishList> wishLists { get; set; }
    public DbSet<CategoryAdmin> categoryAdmins { get; set; }
    public DbSet<UserDTO> userDTOAdmins { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      //account
      modelBuilder.Entity<Account>(entity =>
      {
        entity.ToTable("account");
        entity.HasKey(e => e.id);

        entity.Property(e => e.id).HasColumnName("id");
        entity.Property(e => e.email).HasColumnName("email");
        entity.Property(e => e.lastname).HasColumnName("lastname");
        entity.Property(e => e.firstname).HasColumnName("firstname");
        entity.Property(e => e.bod).HasColumnName("bod");
        entity.Property(e => e.password).HasColumnName("password");
        entity.Property(e => e.role).HasColumnName("role");
        entity.Property(e => e.avatarimg).HasColumnName("avatarimg");
        entity.Property(e => e.createdate).HasColumnName("createdate");
        entity.Property(e => e.updatedate).HasColumnName("updatedate");
        entity.Property(e => e.isdeleted).HasColumnName("isdeleted");
        entity.Property(e => e.refreshtoken).HasColumnName("refreshtoken");
        entity.Property(e => e.refreshtokenexpires).HasColumnName("refreshtokenexpires");
      });
      //address
      modelBuilder.Entity<Address>(entity =>
      {
        entity.ToTable("address");
        entity.HasKey(e => e.id);

        entity.Property(e => e.id).HasColumnName("id");
        entity.Property(e => e.accountid).HasColumnName("account_id");
        entity.Property(e => e.title).HasColumnName("title");
        entity.Property(e => e.namerecipient).HasColumnName("namerecipient");
        entity.Property(e => e.tel).HasColumnName("tel");
        entity.Property(e => e.codeward).HasColumnName("codeward");
        entity.Property(e => e.description).HasColumnName("description");
        entity.Property(e => e.detail).HasColumnName("detail");
        entity.Property(e => e.createdate).HasColumnName("createdate");
        entity.Property(e => e.updatedate).HasColumnName("updatedate");
        entity.HasOne(a => a.account).WithMany(u => u.addresses).HasForeignKey(a => a.accountid).OnDelete(DeleteBehavior.Cascade);
      });
      //category
      modelBuilder.Entity<Category>(entity =>
      {
        entity.ToTable("category");
        entity.HasKey(e => e.id);

        entity.Property(e => e.id).HasColumnName("id");
        entity.Property(e => e.namecategory).HasColumnName("namecategory");
        entity.Property(e => e.idparent).HasColumnName("parent_id");

        entity.HasMany(c => c.Children).WithOne(c => c.Parent).HasForeignKey(c => c.idparent).OnDelete(DeleteBehavior.Restrict);
        entity.HasMany(c => c.Products).WithOne(p => p.category).HasForeignKey(p => p.categoryId).OnDelete(DeleteBehavior.Restrict);
      });
      //discount
      modelBuilder.Entity<Discount>(entity =>
      {
        entity.ToTable("discount");
        entity.HasKey(e => e.id);
        entity.Property(e => e.id).HasColumnName("id");
        entity.Property(e => e.typediscount).HasColumnName("typediscount");
        entity.Property(e => e.discount).HasColumnName("discount");
        entity.Property(e => e.starttime).HasColumnName("starttime");
        entity.Property(e => e.endtime).HasColumnName("endtime");
        entity.Property(e => e.createtime).HasColumnName("createtime");

      });
      //discount-product
      modelBuilder.Entity<DiscountProduct>(entity =>
      {
        entity.ToTable("discount_product");
        entity.HasKey(e => e.id);

        entity.Property(e => e.id).HasColumnName("id");
        entity.Property(e => e.discountid).HasColumnName("discount_id");
        entity.Property(e => e.variantid).HasColumnName("variant_id");

        entity.HasOne(dp => dp.variant).WithMany(d => d.discountProduct).HasForeignKey(dp => dp.variantid).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(dp => dp.discount).WithMany(d => d.discountProducts).HasForeignKey(dp => dp.discountid).OnDelete(DeleteBehavior.Restrict);
      });
      //order
      modelBuilder.Entity<Order>(entity =>
      {
        entity.ToTable("orders");
        entity.HasKey(e => e.id);

        entity.Property(e => e.id).HasColumnName("id");
        entity.Property(e => e.accountid).HasColumnName("account_id");
        entity.Property(e => e.variantid).HasColumnName("variant_id");
        entity.Property(e => e.addressid).HasColumnName("address_id");
        entity.Property(e => e.quantity).HasColumnName("quantity");
        entity.Property(e => e.orderdate).HasColumnName("orderdate");
        entity.Property(e => e.statusorder).HasColumnName("statusorder");
        entity.Property(e => e.receivedate).HasColumnName("receivedate");
        entity.Property(e => e.typepay).HasColumnName("typepay");
        entity.Property(e => e.statuspay).HasColumnName("statuspay");

        entity.HasOne(o => o.account).WithMany(a => a.orders).HasForeignKey(o => o.accountid).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(o => o.address).WithMany(a => a.orders).HasForeignKey(o => o.addressid).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(o => o.variant).WithMany(v => v.orders).HasForeignKey(o => o.variantid).OnDelete(DeleteBehavior.Restrict);
      });
      //product
      modelBuilder.Entity<Product>(entity =>
      {
        entity.ToTable("product");
        entity.HasKey(e => e.id);

        entity.Property(e => e.id).HasColumnName("id");
        entity.Property(e => e.nameproduct).HasColumnName("nameproduct");
        entity.Property(e => e.brand).HasColumnName("brand");
        entity.Property(e => e.description).HasColumnName("description");
        entity.Property(e => e.categoryId).HasColumnName("category");
        entity.Property(e => e.imageurls).HasColumnName("imageurls").HasColumnType("text[]");
        entity.Property(e => e.createdate).HasColumnName("createdate");
        entity.Property(e => e.updatedate).HasColumnName("updatedate");
        entity.Property(e => e.isdeleted).HasColumnName("isdeleted");

        entity.HasOne(p => p.category).WithMany(c => c.Products).HasForeignKey(p => p.categoryId).OnDelete(DeleteBehavior.Restrict);
      });
      //review
      modelBuilder.Entity<Review>(entity =>
      {
        entity.ToTable("review");
        entity.HasKey(e => e.id);
        entity.Property(e => e.id).HasColumnName("id");
        entity.Property(e => e.orderid).HasColumnName("order_id");
        entity.Property(e => e.content).HasColumnName("content");
        entity.Property(e => e.rating).HasColumnName("rating");
        entity.Property(e => e.imageurls).HasColumnName("imageurls").HasColumnType("text[]");
        entity.Property(e => e.createdate).HasColumnName("createdate");
        entity.Property(e => e.updatedate).HasColumnName("updatedate");
        entity.Property(e => e.isupdated).HasColumnName("isupdated");

        entity.HasOne(r => r.order).WithOne(o => o.review).HasForeignKey<Review>(r => r.orderid).OnDelete(DeleteBehavior.Cascade);
      });
      //shopping-cart
      modelBuilder.Entity<ShoppingCart>(entity =>
      {
        entity.ToTable("shoppingcart");
        entity.HasKey(e => e.id);
        entity.Property(e => e.id).HasColumnName("id");
        entity.Property(e => e.accountid).HasColumnName("account_id");
        entity.Property(e => e.variantid).HasColumnName("variant_id");
        entity.Property(e => e.quantity).HasColumnName("quantity");

        entity.HasOne(sc => sc.account).WithMany(a => a.carts).HasForeignKey(sc => sc.accountid).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(sc => sc.variant).WithMany(v => v.carts).HasForeignKey(sc => sc.variantid).OnDelete(DeleteBehavior.Restrict);
      });
      //variant
      modelBuilder.Entity<Variant>(entity =>
      {
        entity.ToTable("variant");
        entity.HasKey(e => e.id);

        entity.Property(e => e.id).HasColumnName("id");
        entity.Property(e => e.productid).HasColumnName("product_id");
        entity.Property(e => e.valuevariant).HasColumnName("valuevariant").HasColumnType("jsonb");
        entity.Property(e => e.stock).HasColumnName("stock");
        entity.Property(e => e.inputprice).HasColumnName("inputprice");
        entity.Property(e => e.price).HasColumnName("price");
        entity.Property(e => e.createdate).HasColumnName("createdate");
        entity.Property(e => e.updatedate).HasColumnName("updatedate");
        entity.Property(e => e.isdeleted).HasColumnName("isdeleted");
        entity.HasOne(v => v.product).WithMany(p => p.variants).HasForeignKey(v => v.productid).OnDelete(DeleteBehavior.Restrict);
      });
      //wish-list
      modelBuilder.Entity<WishList>(entity =>
      {
        entity.ToTable("wishlist");
        entity.HasKey(e => e.id);

        entity.Property(e => e.id).HasColumnName("id");
        entity.Property(e => e.accountid).HasColumnName("account_id");
        entity.Property(e => e.productid).HasColumnName("product_id");
        entity.HasOne(wl => wl.product).WithMany(p => p.wishlists).HasForeignKey(wl => wl.productid).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(wl => wl.account).WithMany(a => a.wishlists).HasForeignKey(wl => wl.accountid).OnDelete(DeleteBehavior.Restrict);
      });


      modelBuilder.Entity<CategoryAdmin>().HasNoKey().ToView(null);
      modelBuilder.Entity<UserDTO>().HasNoKey().ToView(null);

      base.OnModelCreating(modelBuilder);
    }
  }
}