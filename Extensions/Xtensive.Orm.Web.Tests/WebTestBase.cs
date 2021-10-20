// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using System;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Web.Tests
{
  [HierarchyRoot]
  public class DummyEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
    
    [Field(Length = 50)]
    public string Name { get; set; }

    public DummyEntity(Session session)
      : base(session)
    {
    }
  }

  public abstract class WebTestBase : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = DomainConfigurationFactory.CreateWithoutSessionConfigurations();
      config.Types.Register(typeof(DummyEntity));
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
    }

    protected virtual void AddTestRequiredServices(IServiceCollection services)
    {

    }

    protected virtual void AddControllers(IServiceCollection services)
    {
      _ = services.AddControllers()
        .AddApplicationPart(typeof(ActionFilterTest).Assembly);
    }

    protected virtual IApplicationBuilder ConfigurePreRoutingPart(IApplicationBuilder app)
    {
      return app;
    }

    protected virtual IApplicationBuilder ConfigurePostRoutingPart(IApplicationBuilder app)
    {
      return app;
    }

    protected virtual IApplicationBuilder ConfigureRouting(IApplicationBuilder app, string controller, string action)
    {
      var controllerPattern = "{controller=" + controller + "}";
      var actionPattern = "{action=" + action + "}";

      return app.UseRouting()
            .UseEndpoints(
                endpoints => endpoints
                  .MapControllerRoute(
                    name: "default",
                    pattern: controllerPattern + "/" + actionPattern + "/{id?}"));
            
    }

    protected virtual IApplicationBuilder ConfigureApp(IApplicationBuilder app, string controller, string action)
    {
      return ConfigurePostRoutingPart(
          ConfigureRouting(
            ConfigurePreRoutingPart(app), controller, action));
    }

    protected IHostBuilder GetConfiguredHostBuilder<TController>(string action)
    {
      var controller = typeof(TController).Name.Replace("Controller", "");
      return new HostBuilder()
        .ConfigureWebHost(webBuilder => {
          _ = webBuilder.UseTestServer()
          .ConfigureServices(services => {
            _ = services.AddSingleton<Domain>(Domain);
            AddTestRequiredServices(services);
            AddControllers(services);
          })
          .Configure(app =>
            ConfigureApp(app, controller, action));
        });
    }

    protected IHostBuilder GetConfiguredHostBuilder<TController>(string action,
      Action<IServiceCollection> configureServicesAction)
    {
      var controller = typeof(TController).Name.Replace("Controller", "");
      return new HostBuilder()
        .ConfigureWebHost(webBuilder => {
          _ = webBuilder.UseTestServer()
          .ConfigureServices(services => {
            configureServicesAction(services);
          })
          .Configure(app =>
            ConfigureApp(app, controller, action));
        });
    }

    protected IHostBuilder GetConfiguredHostBuilder<TController>(string action,
      Action<IApplicationBuilder, string, string> configureAppAction)
    {
      var controller = typeof(TController).Name.Replace("Controller", "");
      return new HostBuilder()
        .ConfigureWebHost(webBuilder => {
          _ = webBuilder.UseTestServer()
          .ConfigureServices(services => {
            _ = services.AddSingleton<Domain>(Domain);
            AddTestRequiredServices(services);
            AddControllers(services);
          })
          .Configure(app =>
            configureAppAction(app, controller, action));
        });
    }

    protected IHostBuilder GetConfiguredHostBuilder<TController>(string action,
      Action<IServiceCollection> configureServicesAction,
      Action<IApplicationBuilder, string, string> configureAppAction)
    {
      var controller = typeof(TController).Name.Replace("Controller", "");
      return new HostBuilder()
        .ConfigureWebHost(webBuilder => {
          _ = webBuilder.UseTestServer()
          .ConfigureServices(services => {
            configureServicesAction(services);
          })
          .Configure(app =>
            configureAppAction(app, controller, action));
        });
    }
  }
}