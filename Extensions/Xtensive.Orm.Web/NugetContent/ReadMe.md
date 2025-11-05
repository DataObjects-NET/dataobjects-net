Xtensive.Orm.Web
================

Summary
-------
The extension adds integration for DataObjects.Net and ASP.NET Core. It contains an action filter called SessionActionFilter
and a middleware called OpenSessionMiddleware. The action filter is useful for providing session per MVC action. The middleware,
though, has wider coverage and can provide session to actions, controllers, razor pages and to other middleware down the pipeline.
Both of them open session and transaction and at the end dispose them. As obsolete SessionManager, they complete transacton scope
by default unless an exeption appeared. (more info on https://dataobjects.net)

Prerequisites
-------------
DataObjects.Net 7.1 or later (https://dataobjects.net)

Usage of action filter
----------------------

To start using action filter it should be added to action filters collection like so

```csharp
public class Startup
{
  public Startup(IConfiguration configuration)
  {
    
  }

  public void ConfigureServices(IServiceCollection services)
  {
    var domain = BuildDomain();

    // Domain should be available as service to have
    // access to it from action filter.
    services.AddSingleton<Domain>(domain);

    // Adds SessionAccessor as scoped service (one instance per request).
    // Session accessor will be able to access Session and TransactionScope
    // instances which are in HttpContext
    services.AddDataObjectsSessionAccessor();

    // Adds the action filter
    services.AddControllers(options => options.Filters.AddDataObjectsSessionActionFilter());
  }

  public void Configure(IApplicationBuilder app, IHostingEnvironment env)
  {
    if (env.IsDevelopment()) {
      app.UseDeveloperExceptionPage();
    }
    else {
      app.UseExceptionHandler("/Home/Error");
    }
    app.UseStaticFiles()
      .UseRouting()
      .UseAuthorization()
      .UseEndpoints(endpoints => {
        endpoints.MapControllerRoute(
          name: "default",
          pattern: "{controller=Home}/{action=Index}/{id?}");
      });
  }
}


After action filter is added you can use it like in the example bellow

public class HomeController : Controller
{
  // If action require Session and TransactionScope to be opened then
  // just put parameter like in this method.
  // Action filter will find it wrap action with session
  public IActionResult Index([FromServices] SessionAccessor sessionAccessor)
  {
    var sessionInstance = sessionAccessor.Session;
    var transactionScopeInstance = sessionAccessor.TransactionScope;

    // some queries to database

    return View();
  }

  // If action does not require opened Session
  // then don't put SessionAccessor as parameter
  // action filter will skip openings
  public IActionResult Privacy()
  {
    return View();
  }
}


Usage of Middleware
-------------------

The middleware is needed to be placed to pipeline before any other middleware that require access
to session. Pipeline may be configured like so

public class Startup
{
  public Startup(IConfiguration configuration)
  {
    
  }

  public void ConfigureServices(IServiceCollection services)
  {
    var domain = BuildDomain();

    // Domain should be available as service to have
    // access to it from action filter.
    services.AddSingleton<Domain>(domain);

    // Adds SessionAccessor as scoped service (one instance per request).
    // Session accessor will be able to access Session and TransactionScope
    // instances which are in HttpContext
    services.AddDataObjectsSessionAccessor();

    // Adds the action filter
    services.AddControllers();
  }

  public void Configure(IApplicationBuilder app, IHostingEnvironment env)
  {
    if (env.IsDevelopment()) {
      app.UseDeveloperExceptionPage();
    }
    else {
      app.UseExceptionHandler("/Home/Error");
    }
    app.UseStaticFiles()
      .UseRouting()// this middleware won't have opened session
      .UseDataObjectsSessionOpener()// open Session and Transaction scope
      .UseAuthorization()// this middleware and the rest down the pipe have Session access
      .UseEndpoints(endpoints => {
        endpoints.MapControllerRoute(
          name: "default",
          pattern: "{controller=Home}/{action=Index}/{id?}");
      });
  }
}

After that you can access opened Session and TransactionScope

public class HomeController : Controller
{
  // access from controller's constructor
  public HomeController(SessionAccessor sessionAccessor)
  {
    // some work
  }

  // access from action
  public IActionResult Index([FromServices] SessionAccessor sessionAccessor)
  {
    var sessionInstance = sessionAccessor.Session;
    var transactionScopeInstance = sessionAccessor.TransactionScope;

    // some queries to database

    return View();
  }

  // NOTE that here session is opened too,
  // even there is no SessionAccessor as parameter
  // this is the difference in work of middleware and action filter
  public IActionResult Privacy()
  {
    return View();
  }
}


The middleware is also usable in Razor Pages projects. In this case
your Startup class may look like this:

public class Startup
{
  public Startup(IConfiguration configuration)
  {
    Configuration = configuration;
  }

  public IConfiguration Configuration { get; }

  // This method gets called by the runtime. Use this method to add services to the container.
  public void ConfigureServices(IServiceCollection services)
  {
    var domain = Domain.Build();
    services.AddSingleton(domain);
    services.AddDataObjectsSessionAccessor();
    services.AddRazorPages();
  }

  // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
  public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
  {
    if (env.IsDevelopment()) {
      app.UseDeveloperExceptionPage();
    }
    else {
      app.UseExceptionHandler("/Error");
    }

    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthorization();

    app.UseDataObjectsSessionOpener();

    app.UseEndpoints(endpoints =>
    {
      endpoints.MapRazorPages();
    });
  }
}
```

And then in actual pages you can use SessionAccessor like below

public class IndexModel : PageModel
{

  public IndexModel(SessionAccessor accessor)
  {
    _logger = logger;
  }

  public void OnGet([FromServices] SessionAccessor sessionAccessor)
  {
    var sessionInstance = sessionAccessor.Session;

    // query some data from database
  }
}
