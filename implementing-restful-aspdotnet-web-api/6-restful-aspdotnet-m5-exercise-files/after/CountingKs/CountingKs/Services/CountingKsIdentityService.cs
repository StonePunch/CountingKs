using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace CountingKs.Services
{
  public class CountingKsIdentityService : CountingKs.Services.ICountingKsIdentityService
  {
    public string CurrentUser 
    {
      get
      {
#if DEBUG
        return "shawnwildermuth";
#else
        return Thread.CurrentPrincipal.Identity.Name;
#endif
      }
    }
  }
}