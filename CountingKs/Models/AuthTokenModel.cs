using System;

namespace CountingKs.Models
{
  public class AuthTokenModel
  {
    public string Token { get; set; }
    public DateTime Expiration { get; set; }
  }
}