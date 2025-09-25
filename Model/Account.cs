namespace be_dotnet_ecommerce1.Model
{
  public class Account
  {
    public int id { get; set; }
    public required string email { get; set; }
    public string? lastname { get; set; }
    public string? firstname { get; set; }
    public DateTime? bod { get; set; }
    public string? password { get; set; }
    public int role { get; set; }
    public string? avatarimg { get; set; }
    public DateTime? createdate { get; set; }
    public DateTime? updatedate { get; set; }
    public bool? isdeleted { get; set; }
    public string? refreshtoken { get; set; }
    public DateTime? refreshtokenexpires { get; set; }

  }
}
