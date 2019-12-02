# Web extension

The extension adds integration for DataObjects.Net  and ASP.NET Core. It contains `SessionManager` class which is middleware and can be used as part of ASP.NET Core Pipeline. `SessionManager` opens session and transaction on going forward the pipeline and disposes them on going backwards.

`SessionManager` has the following features:
1. When `Session.Current` is accessed and there is no current `Session`, it will provide a new instance of `Session`. In that case a new `TransactionScope` will be created. Created `TransactionScope` will be committed when the pipeline execution returns to `SessionManager` without any exception, otherwise it will be rolled back.
2. Setting `SessionManager.Demand().HasErrors` to true will lead to rollback of this transaction.
3. `SessionManager.Current` (and `SessionManager.Demand()`) returns the instance of the `SessionManager` bound to the current pipeline execution, i.e. current `SessionManager`. Its `Session` property (if not null) is the same value as the one provided by `Session.Current`.

Note that presence of `SessionManager` does not prevent you from creating `Sessions` manually. It operates relying on `Session.Resolver` event, which is raised only when there is no current Session.

Finally, no automatic `Session` + `TransactionScope` will be provided, if you don't use `Session.Current/Session.Demand()` methods in your code (directly or indirectly). So e.g. requests to static web pages won't lead to any DB interaction.

### Installation

The extension is available on Nuget

    dotnet add package Xtensive.Orm.Tracking.Core

### Usage

To start using SessionManager it should be added to ASP.NET Middleware pipeline in `Startup` class like in example below

    public class Startup
    {
      public Startup(IConfiguration configuration)
      {

      }

      public void ConfigureServices(IServiceCollection services)
      {
        // Configure services
      }

      public void Configure(IApplicationBuilder app, IHostingEnvironment env)
      {
        // Configure parts of the pipeline which are before SessionManager.
        // It will be unable to use SessionManager functionality in these parts
        // For instance,
        app.UseStaticFiles()

        // Add session manager to the pipeline
        app.UseSessionManager();

        // Configure parts of the pipeline which are after SessionManager. 
        // These parts will work with SessionManager.

        // For instance, MVC controllers will be able to query data using DataObjects.Net
        app.UseMvc(routes => {
            routes.MapRoute(
                name: "default",
                template: "{controller=Home}/{action=Index}/{id?}");
        });
      }
    }