namespace dotnet.Dtos
{
  public class LoginRequest
  {
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
  }

  public class TokenRequest
{
    public string IdToken { get; set; } = null!;
}

  public class RefreshTokenRequest
  {
    public string RefreshToken { get; set; } = "";
  }
}
