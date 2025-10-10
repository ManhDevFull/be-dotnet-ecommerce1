using be_dotnet_ecommerce1.Model;

namespace dotnet.Model
{
  public class Address
  {
    public int id { get; set; }
    public int accountid { get; set; }
    public string title { get; set; } = null!;
    public string namerecipient { get; set; } = null!;
    public string tel { get; set; } = null!;
    public int codeward { get; set; }
    public string? description { get; set; }
    public string? detail { get; set; }
    public DateTime? createdate { get; set; }
    public DateTime? updatedate { get; set; }
    public Account? account { get; set; }
    public List<Order>? orders { get; set; }
  }
}