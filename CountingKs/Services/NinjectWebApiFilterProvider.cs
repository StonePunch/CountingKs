using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace CountingKs.Services
{
  public class NinjectWebApiFilterProvider : IFilterProvider
  {
    private readonly IKernel _kernel;

    public NinjectWebApiFilterProvider(IKernel kernel)
    {
      _kernel = kernel;
    }

    public IEnumerable<FilterInfo> GetFilters(HttpConfiguration configuration, HttpActionDescriptor actionDescriptor)
    {
      IEnumerable<FilterInfo> controllerFilters = actionDescriptor.ControllerDescriptor.GetFilters()
        .Select(filter => new FilterInfo(filter, FilterScope.Controller));
      IEnumerable<FilterInfo> actionFilters = actionDescriptor.GetFilters()
        .Select(filter => new FilterInfo(filter, FilterScope.Action));

      IEnumerable<FilterInfo> filters = controllerFilters.Concat(actionFilters);

      foreach (FilterInfo filterInfo in filters)
      {
        // Injection

        _kernel.Inject(filterInfo.Instance);
      }

      return filters;
    }
  }
}