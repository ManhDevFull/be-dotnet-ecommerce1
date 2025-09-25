using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet.Model
{
  public class Variant
  {
    public int id { get; set; }
    public int productid { get; set; }
    public string valuevariant { get; set; } = null!; // JSONB
    public int stock { get; set; }
    public int inputprice { get; set; }
    public int price { get; set; }
    public DateTime createdate { get; set; }
    public DateTime updatedate { get; set; }
    public bool isdeleted { get; set; }
  }
}