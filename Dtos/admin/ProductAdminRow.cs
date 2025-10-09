using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace dotnet.Dtos.admin
{
  [Keyless]
  public class ProductAdminRow
  {
    public int product_id { get; set; }
    public string product_name { get; set; } = default!;
    public string product_description { get; set; } = default!;
    public string? brand { get; set; }
    public int category_id { get; set; }
    public string category_name { get; set; } = default!;
    public string[] imageurls { get; set; } = Array.Empty<string>();
    public DateTime product_created { get; set; }
    public DateTime product_updated { get; set; }
    public long total_stock { get; set; }
    public string variants { get; set; } = "[]";
  }
}