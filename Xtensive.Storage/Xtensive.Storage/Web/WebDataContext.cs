using System;
using System.Web;
using System.Web.Configuration;

namespace Xtensive.Storage.Web
{
  /// <summary>
  /// Provides access to <see cref="Domain"/> and current <see cref="Session"/> in web application.
  /// </summary>
  /// <remarks>
  /// To initialize this class add it to <see cref="HttpModulesSection"/> configuration section in <c>web.config</c> file
  /// and assign domain builder delegate to <see cref="DomainBuilder"/> property 
  /// in <c>global.asax</c>, Application_Start method.
  /// </remarks>
  /// <example>
  /// <c>web.config</c>:
  /// <code>
  /// &lt;configuration&gt;
  ///   &lt;system.web&gt;
  ///     &lt;httpModules&gt;
  ///       &lt;add name="WebDataContext" type="AspNetSample.WebDataContext, AspNetSample, Version=1.0.0.0, Culture=neutral"/&gt;
  ///     &lt;/httpModules&gt;
  ///   &lt;/system.web&gt;
  /// &lt;/configuration&gt;
  /// </code>
  /// <c>Global.asax.cs</c>:
  /// <code>
  ///   public class Global : System.Web.HttpApplication
  ///   {
  ///     protected void Application_Start(object sender, EventArgs e)
  ///     {
  ///       WebDataContext.DomainBuilder = DomainInitializer.BuildDomain;
  ///     }
  ///   }
  /// </code>
  /// <c>DomainInitializer.cs</c>:
  /// <code>
  ///   public static class DomainInitializer
  ///   {
  ///     public static Domain BuildDomain()
  ///     {
  ///       // loading domain configuration ...
  ///       var domain = Domain.Build(config);
  ///       return domain;
  ///     }
  ///   }
  /// </code>
  /// </example>
  public class WebDataContext : IHttpModule
  {
    #region Static members

    private static readonly object sessionItemKey = new object();
    private static readonly object domainBuildLock = new object();

    /// <summary>
    /// Gets or sets the domain builder delegate.
    /// </summary>
    /// <remarks>
    /// This delegate will be invoked to build domain on first request.
    /// </remarks>
    /// <value>The domain builder.</value>
    public static Func<Domain> DomainBuilder { get; set; }

    private static Domain domain;

    /// <summary>
    /// Gets the domain used in web application.
    /// </summary>
    /// <value>The domain.</value>
    public static Domain Domain
    {
      get
      {
        EnsureDomainIsBuilt();
        return domain;
      }
    }

    private static void EnsureDomainIsBuilt()
    {
      if (domain == null)
        lock (domainBuildLock)
          if (domain == null) {
            domain = DomainBuilder.Invoke();
            Session.SetCurrentSessionResolver(GetSessionFromHttpContext);
          }
    }

    private static Session GetSessionFromHttpContext()
    {
      var httpContext = HttpContext.Current;
      if (httpContext == null)
        return null;
      return (Session)httpContext.Items[sessionItemKey];
    }

    private static void SetSessionInHttpContext(Session session)
    {
      HttpContext.Current.Items[sessionItemKey] = session;
    }

    #endregion

    private SessionConsumptionScope sessionConsumptionScope;
    private TransactionScope transactionScope;
    private bool rollbackTransaction;

    private void BeginRequest(object sender, EventArgs e)
    {
      SetSessionInHttpContext(sessionConsumptionScope.Session);
      transactionScope = Transaction.Open();
      rollbackTransaction = false;
    }

    private void Error(object sender, EventArgs e)
    {
      rollbackTransaction = true;
    }

    private void EndRequest(object sender, EventArgs e)
    {
      if (!rollbackTransaction)
        transactionScope.Complete();
    }

    #region IHttpModule members

    void IHttpModule.Init(HttpApplication context)
    {
      context.BeginRequest += BeginRequest;
      context.EndRequest += EndRequest;
      context.Error += Error;


      sessionConsumptionScope = Session.Open(Domain, false);
    }

    void IHttpModule.Dispose()
    {
      sessionConsumptionScope.Dispose();

    }

    #endregion
  }
}