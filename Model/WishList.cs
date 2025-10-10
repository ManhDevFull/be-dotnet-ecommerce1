using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using be_dotnet_ecommerce1.Model;

namespace dotnet.Model
{
  public class WishList
  {
    public int id { get; set; }
    public int accountid { get; set; }
    public int productid { get; set; }
    public Account? account { get; set; }
    public Product? product { get; set; }
  }
}