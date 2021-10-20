using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Xtensive.Orm.Web.Tests.Filters;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace Xtensive.Orm.Web.Tests.Filters
{
  public class ActionFilterTestController : Controller 
  {
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "parameter required for test")]
    public IActionResult NoDomainAsService([FromServices] SessionAccessor accessor)
    {
      return Ok();
    }

    public IActionResult NoAccessors()
    {
      var sessionObject = (Session) HttpContext.Items[SessionAccessor.SessionIdentifier];
      if (sessionObject != null)
        return BadRequest("Session Initialized");
      return Ok("Ok");
    }

    public IActionResult WithOneAccessorNoFilter([FromServices] SessionAccessor accessor)
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
  public class ActionFilterTest : WebTestBase
  {
    [Test]
    public async Task NoDomainAsServiceTest()
    {
      var hostBuilder = GetConfiguredHostBuilder<ActionFilterTestController>(
        nameof(ActionFilterTestController.NoDomainAsService),
        (s) => {
          // no domain registration
          AddTestRequiredServices(s);
          AddControllers(s);
        });

      using (var host = await hostBuilder.StartAsync()) {
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await host.GetTestClient().GetAsync("/"));
        Assert.That(ex.Message.Contains("Domain is not found"), Is.True);
      }
    }

    [Test]
    public async Task NoAccessorsTest()
    {
      var hostBuilder = GetConfiguredHostBuilder<ActionFilterTestController>(
        nameof(ActionFilterTestController.NoAccessors));

      using (var host = await hostBuilder.StartAsync()) {
        var response = await host.GetTestClient().GetAsync("/");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      }
    }

    [Test]
    public async Task WithOneAccessorNoFilterTest()
    {
      var hostBuilder = GetConfiguredHostBuilder<ActionFilterTestController>(
        nameof(ActionFilterTestController.WithOneAccessorNoFilter),
        (s) =>{
          AddTestRequiredServices(s);
          AddControllersWithNoFilter(s);
        }); 

      using (var host = await hostBuilder.StartAsync()) {
        var response = await host.GetTestClient().GetAsync("/");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      }
    }

    [Test]
    public async Task WithOneAccessorFromServicesTest()
    {
      var hostBuilder = GetConfiguredHostBuilder<ActionFilterTestController>(
        nameof(ActionFilterTestController.WithOneAccessorFromServices));

      using (var host = await hostBuilder.StartAsync()) {
        var response = await host.GetTestClient().GetAsync("/");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      }
    }

    [Test]
    public async Task WithTwoAccessorsFromServicesTest()
    {
      var hostBuilder = GetConfiguredHostBuilder<ActionFilterTestController>(
        nameof(ActionFilterTestController.WithTwoAccessorsFromServices));

      using (var host = await hostBuilder.StartAsync()) {
        var response = await host.GetTestClient().GetAsync("/");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      }
    }

    [Test]
    public async Task ManualSessionRemovalTest()
    {
      var hostBuilder = GetConfiguredHostBuilder<ActionFilterTestController>(
        nameof(ActionFilterTestController.ManualSessionRemoval));

      using (var host = await hostBuilder.StartAsync()) {
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await host.GetTestClient().GetAsync("/"));
        Assert.That(ex.Message.Contains("Session or TransactionScope no longer exists in HttpContext"), Is.True);
      }
    }

    [Test]
    public async Task ManualTransactionRemovalTest()
    {
      var hostBuilder = GetConfiguredHostBuilder<ActionFilterTestController>(
        nameof(ActionFilterTestController.ManualTransactionRemoval));

      using (var host = await hostBuilder.StartAsync()) {
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await host.GetTestClient().GetAsync("/"));
        Assert.That(ex.Message.Contains("Session or TransactionScope no longer exists in HttpContext"), Is.True);
      }
    }

    [Test]
    public async Task AutoCompleteTransactionTest()
    {
      var hostBuilder = GetConfiguredHostBuilder<ActionFilterTestController>(
        nameof(ActionFilterTestController.AutoCompleteTransaction));

      using (var host = await hostBuilder.StartAsync()) {
        var response = await host.GetTestClient().GetAsync("/");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      }

      await using (var session = await Domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        Assert.That(session.Query.All<DummyEntity>().FirstOrDefault(), Is.Not.Null);
      }
    }

    protected override void AddTestRequiredServices(IServiceCollection services)
    {
      base.AddTestRequiredServices(services);
      _ = services.AddScoped<SessionAccessor>();
    }

    protected override void AddControllers(IServiceCollection services)
    {
      _ = services.AddControllers(options => options.Filters.AddDataObjectsSessionActionFilter())
        .AddApplicationPart(typeof(ActionFilterTest).Assembly);
    }

    private void AddControllersWithNoFilter(IServiceCollection services)
    {
      _ = services.AddControllers()
       .AddApplicationPart(typeof(ActionFilterTest).Assembly);
    }
  }
}