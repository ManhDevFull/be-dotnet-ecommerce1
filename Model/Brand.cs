using dotnet.Model;

namespace be_dotnet_ecommerce1.Model
{
    public class Brand
    {
        public int id { get; set; }
        public string name { get; set; } = null!;
        public List<Product>? products { get; set; }
    }
}