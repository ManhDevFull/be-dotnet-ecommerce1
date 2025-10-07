using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet.Dtos
{
  public class ProductDTO
  {
    public int id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public string brand { get; set; }
    public int categoryId { get; set; }
    public string categoryName { get; set; }
    public List<string> imgUrls { get; set; }
    public DateTime createDate { get; set; }
    public int totalStock { get; set; }
    public ValueVariant[] variant { get; set; }
    public DateTime updateDate { get; set; }
    public class ValueVariant
    {
      public int id { get; set; }
      public string value { get; set; }
      public int stock { get; set; }
      public int inputPrice { get; set; }
      public int price { get; set; }
      public DateTime createDate { get; set; }
      public DateTime updateDate { get; set; }
    }
  }
}
