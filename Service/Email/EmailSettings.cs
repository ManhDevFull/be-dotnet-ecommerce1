namespace dotnet.Service.Email;

public class EmailSettings
{
  public string Host { get; set; } = string.Empty;
  public int Port { get; set; } = 587;
  public string Username { get; set; } = string.Empty;
  public string Password { get; set; } = string.Empty;
  public string FromAddress { get; set; } = string.Empty;
  public string FromName { get; set; } = string.Empty;
  public bool EnableSsl { get; set; } = true;
  public int ResendCooldownSeconds { get; set; } = 60;
  public int CodeExpiryMinutes { get; set; } = 10;
  public int MaxAttempts { get; set; } = 5;
}
