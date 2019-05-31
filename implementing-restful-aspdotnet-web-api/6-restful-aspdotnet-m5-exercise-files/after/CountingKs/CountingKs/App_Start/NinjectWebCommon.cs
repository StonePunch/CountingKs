[assembly: WebActivator.PreApplicationStartMethod(typeof(CountingKs.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(CountingKs.App_Start.NinjectWebCommon), "Stop")]

namespace CountingKs.App_Start
{
  using System;
  using System.Web;
  using System.Web.Http;
  using System.Web.Http.Filters;
  using CountingKs.Data;
  using CountingKs.Services;
  using Microsoft.Web.Infrastructure.DynamicModuleHelper;
  using Ninject;
  using Ninject.Web.Common;
  using WebApiContrib.IoC.Ninject;

  public static class NinjectWebCommon
  {
    private static readonly Bootstrapper bootstrapper = new Bootstrapper();

    /// <summary>
    /// Starts the application
    /// </summary>
    public static void Start()
    {
      DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
      DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
      bootstrapper.Initialize(CreateKernel);
    }

    /// <summary>
    /// Stops the application.
    /// </summary>
    public static void Stop()
    {
      bootstrapper.ShutDown();
    }

    /// <summary>
    /// Creates the kernel that will manage your application.
    /// </summary>
    /// <returns>The created kernel.</returns>
    private static IKernel CreateKernel()
    {
      var kernel = new StandardKernel();
      kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
      kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

      // Support WebAPI
      GlobalConfiguration.Configuration.DependencyResolver = 
        new NinjectResolver(kernel);
      GlobalConfiguration.Configuration.Services.Add(typeof(IFilterProvider), 
        new NinjectWebApiFilterProvider(kernel));


      RegisterServices(kernel);
      return kernel;
    }

    /// <summary>
    /// Load your modules or register your services here!
    /// </summary>
    /// <param name="kernel">The kernel.</param>
    private static void RegisterServices(IKernel kernel)
    {
      kernel.Bind<ICountingKsRepository>().To<CountingKsRepository>();
      kernel.Bind<CountingKsContext>().To<CountingKsContext>();
      kernel.Bind<ICountingKsIdentityService>().To<CountingKsIdentityService>();
    }
  }
}
