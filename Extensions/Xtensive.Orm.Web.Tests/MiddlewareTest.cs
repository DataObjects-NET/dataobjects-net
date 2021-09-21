// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Xtensive.Orm.Web.Tests.Middleware;
using Microsoft.AspNetCore.Builder;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace Xtensive.Orm.Web.Tests.Middleware
{
  public class MiddlewareTestController : Controller
  {
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "parameter required for test")]
    public IActionResult NoDomainAsService([FromServices] SessionAccessor accessor)
    {
      return Ok();
    }

    public IActionResult NoAccessors()
    {
      var sessionObject = (Session) HttpContext.Items[SessionAccessor.SessionIdentifier];
      if (sessionObject == null)
        return BadRequest("Session not initialized");
      return Ok("Ok");
    }

    public IActionResult WithOneAccessorNoMiddleware([FromServices] SessionAccessor accessor)
    {
      var sessionObject = (Session) HttpContext.Items[SessionAccessor.SessionIdentifier];
      if (sessionObject == null && !accessor.ContextIsBound)
        return Ok();
      return BadRequest("Session initialized or context bound");
    }

    public IActionResult WithOneAccessorFromServices([FromServices] SessionAccessor accessor)
    {
      if (!accessor.ContextIsBound)
        return BadRequest("ContextIsBound=false");
      if (accessor.Session == null)
        return BadRequest("SessionIsNull");
      return Ok();
    }

    public IActionResult WithTwoAccessorsFromServices(
      [FromServices] SessionAccessor accessor1,
      [FromServices] SessionAccessor accessor2)
    {
      if (!accessor1.ContextIsBound || !accessor2.ContextIsBound)
        return BadRequest("ContextIsBound=false");
      if (accessor1.Session == null || accessor2.Session == null)
        return BadRequest("SessionIsNull");
      return Ok();
    }

    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "parameter required for test")]
    public IActionResult ManualSessionRemoval([FromServices] SessionAccessor accessor)
    {
      _ = HttpContext.Items.Remove(SessionAccessor.SessionIdentifier);
      return Ok();
    }

    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "parameter required for test")]
    public IActionResult ManualTransactionRemoval([FromServices] SessionAccessor accessor)
    {
      _ = HttpContext.Items.Remove(SessionAccessor.ScopeIdentifier);
      return Ok();
    }

    public IActionResult AutoCompleteTransaction([FromServices] SessionAccessor accessor)
    {
      _ = new DummyEntity(accessor.Session);
      return Ok();
    }
  }
}

namespace Xtensive.Orm.Web.Tests
{
  public class MiddlewareTest : WebTestBase
  {
    [Test]
    public async Task NoDomainAsServiceTest()
    {
      var hostBuilder = GetConfiguredHostBuilder<MiddlewareTestController>(
        nameof(MiddlewareTestController.NoDomainAsService),
        (s) => {
          // no domain registration
          AddTestRequiredServices(s);
          AddControllers(s);
        });

      using (var host = await hostBuilder.StartAsync()) {
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await host.GetTestClient().GetAsync("/"));
      }
    }

    [Test]
    public async Task NoAccessorsTest()
    {
      var hostBuilder = GetConfiguredHostBuilder<MiddlewareTestController>(
        nameof(MiddlewareTestController.NoAccessors));

      using (var host = await hostBuilder.StartAsync()) {
        var response = await host.GetTestClient().GetAsync("/");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      }
    }

    [Test]
    public async Task WithOneAccessorNoMiddlewareTest()
    {
      var hostBuilder = GetConfiguredHostBuilder<MiddlewareTestController>(
        nameof(MiddlewareTestController.WithOneAccessorNoMiddleware),
        (app, controller, action)=> ConfigureRouting(app, controller, action));

      using (var host = await hostBuilder.StartAsync()) {
        var response = await host.GetTestClient().GetAsync("/");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      }
    }

    [Test]
    public async Task WithOneAccessorFromServicesTest()
    {
      var hostBuilder = GetConfiguredHostBuilder<MiddlewareTestController>(
        nameof(MiddlewareTestController.WithOneAccessorFromServices));

      using (var host = await hostBuilder.StartAsync()) {
        var response = await host.GetTestClient().GetAsync("/");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      }
    }

    [Test]
    public async Task WithTwoAccessorsFromServicesTest()
    {
      var hostBuilder = GetConfiguredHostBuilder<MiddlewareTestController>(
        nameof(MiddlewareTestController.WithTwoAccessorsFromServices));

      using (var host = await hostBuilder.StartAsync()) {
        var response = await host.GetTestClient().GetAsync("/");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      }
    }

    [Test]
    public async Task ManualSessionRemovalTest()
    {
      var hostBuilder = GetConfiguredHostBuilder<MiddlewareTestController>(
        nameof(MiddlewareTestController.ManualSessionRemoval));

      using (var host = await hostBuilder.StartAsync()) {
        Assert.DoesNotThrowAsync(async () => await host.GetTestClient().GetAsync("/"));
      }
    }

    [Test]
    public async Task ManualTransactionRemovalTest()
    {
      var hostBuilder = GetConfiguredHostBuilder<MiddlewareTestController>(
        nameof(MiddlewareTestController.ManualTransactionRemoval));

      using (var host = await hostBuilder.StartAsync()) {
        Assert.DoesNotThrowAsync(async () => await host.GetTestClient().GetAsync("/"));
      }
    }

    [Test]
    public async Task AutoCompleteTransactionTest()
    {
      var hostBuilder = GetConfiguredHostBuilder<MiddlewareTestController>(
        nameof(MiddlewareTestController.AutoCompleteTransaction));

      using (var host = await hostBuilder.StartAsync()) {
        var response = await host.GetTestClient().GetAsync("/");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      }

      await using (var session = await Domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        Assert.That(session.Query.All<DummyEntity>().FirstOrDefault(), Is.Not.Null);
      }
    }

    protected override IApplicationBuilder ConfigurePreRoutingPart(IApplicationBuilder app) =>
      app.UseDataObjectsSessionOpener();

    protected override void AddTestRequiredServices(IServiceCollection services)
    {
      base.AddTestRequiredServices(services);
      _ = services.AddScoped<SessionAccessor>();
    }
  }
}
