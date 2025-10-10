using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using be_dotnet_ecommerce1.Model;

namespace dotnet.Model
{
  public class Product
  {
    public int id { get; set; }
    public string nameproduct { get; set; } = null!;
    public string? brand { get; set; }
    public string description { get; set; } = null!;
    public int categoryId { get; set; }
    public List<string> imageurls { get; set; } = new();
    public DateTime createdate { get; set; }
    public DateTime updatedate { get; set; }
    public bool isdeleted { get; set; }
    public Category? category { get; set; }
    public List<Variant>? variants { get; set; }
    public List<WishList>? wishlists { get; set; }
  }
}