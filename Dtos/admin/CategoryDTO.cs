using System.ComponentModel.DataAnnotations;
namespace be_dotnet_ecommerce1.Model
{
  public class CategoryAdmin
  {
    [Key]
    public int id { get; set; }
    public string namecategory { get; set; } = null!;
    public int? idparent { get; set; }
    public int? product { get; set; }
  }
}