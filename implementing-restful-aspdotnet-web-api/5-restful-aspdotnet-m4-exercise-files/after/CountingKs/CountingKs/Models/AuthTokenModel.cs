using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CountingKs.Models
{
  public class AuthTokenModel
  {
    public string Token { get; set; }

    public DateTime Expiration { get; set; }
  }
}
