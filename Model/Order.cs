using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using be_dotnet_ecommerce1.Model;

namespace dotnet.Model
{
  public class Order
  {
    public int id { get; set; }
    public int accountid { get; set; }
    public int variantid { get; set; }
    public int addressid { get; set; }
    public int quantity { get; set; }
    public DateTime orderdate { get; set; }
    public string? statusorder { get; set; }
    public DateTime? receivedate { get; set; }
    public string? typepay { get; set; }
    public string? statuspay { get; set; }
    public Account? account { get; set; }
    public Variant? variant { get; set; }
    public Address? address { get; set; }
    public Review? review { get; set; }
  }
}