using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using be_dotnet_ecommerce1.Model;

namespace dotnet.Model
{
  public class ShoppingCart
  {
    public int id { get; set; }
    public int accountid { get; set; }
    public int variantid { get; set; }
    public int quantity { get; set; }
    public Account? account { get; set; }
    public Variant? variant { get; set; }
  }
}