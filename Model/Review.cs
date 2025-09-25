using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet.Model
{
  public class Review
  {
    public int id { get; set; }
    public int orderid { get; set; }
    public string? content { get; set; }
    public int rating { get; set; }
    public List<string>? imageurls { get; set; }
    public DateTime? createdate { get; set; }
    public DateTime? updatedate { get; set; }
    public bool isupdated { get; set; }
  }
}