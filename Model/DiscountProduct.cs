namespace dotnet.Model
{
  public class DiscountProduct
  {
    public int id { get; set; }
    public int discountid { get; set; }
    public int variantid { get; set; }
    public Variant? variant { get; set; }
    public Discount? discount { get; set; }
  }
}