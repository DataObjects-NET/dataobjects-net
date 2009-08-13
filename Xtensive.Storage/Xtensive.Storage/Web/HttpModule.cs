using System;
using System.Web;
using System.Web.Configuration;
using Xtensive.Core;
using Xtensive.Core.Disposing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Web
{
  /// <summary>
  /// Provides access to current <see cref="Domain"/> and <see cref="Session"/> for web applications;
  /// ensures <see cref="Domain"/> is built, and built just once.
  /// </summary>
  /// <remarks>
  /// To initialize this class, add it to <see cref="HttpModulesSection"/> configuration section 
  /// in <c>web.config</c> file and set its <see cref="DomainBuilder"/> in
  /// <c>Application_Start</c> method of your <c>Global.asax.cs</c> file.
  /// </remarks>
  /// <example>
  /// <c>web.config</c>:
  /// <code>
  /// &lt;configuration&gt;
  ///   &lt;system.web&gt;
  ///     &lt;httpModules&gt;
  ///       &lt;add name="HttpModule" type="Xtensive.Storage.Web.HttpModule, Xtensive.Storage"/&gt;
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
  ///       HttpModule.DomainBuilder = DomainBuilder.Build;
  ///     }
  ///   }
  /// </code>
  /// <c>DomainInitializer.cs</c>:
  /// <code>
  ///   public static class DomainBuilder
  ///   {
  ///     public static Domain Build()
  ///     {
  ///       // loading domain configuration ...
  ///       var domain = Domain.Build(config);
  ///       return domain;
  ///     }
  ///   }
  /// </code>
  /// </example>
  public class HttpModule : IHttpModule
  {
    private static readonly object currentItemKey   = new object();
    private static readonly object domainBuildLock = new object();
    private static Func<Domain> domainBuilder;
    private static Func<Pair<Session, IDisposable>> sessionProvider;
    private static Domain domain;

    private readonly object provideSessionLock = new object();
    private Session session;
    private IDisposable disposeOnEndRequest;
    private bool hasErrors;


    /// <summary>
    /// Sets the domain builder delegate.
    /// This delegate is invoked to build the domain on first request.
    /// </summary>
    /// <remarks>
    /// The setter of this property can be invoked just once per application lifetime; 
    /// assigned domain builder can not be changed.
    /// </remarks>
    /// <exception cref="NotSupportedException">Domain builder is already assigned.</exception>
    public static Func<Domain> DomainBuilder {
      protected get {
        return domainBuilder;
      }
      set {
        if (domainBuilder!=null)
          throw Exceptions.AlreadyInitialized("DomainBuilder");
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        domainBuilder = value;
      }
    }

    /// <summary>
    /// Sets the session provider delegate.
    /// This delegate is invoked to open the <see cref="Session"/> on the first attempt
    /// to read this property (see <see cref="HasSession"/> as well) in each web request.
    /// Normally, this delegate must also ensure a transaction is created there as well - 
    /// either by opening it explicitly, or by using 
    /// <see cref="SessionOptions.AmbientTransactions"/>.
    /// </summary>
    /// <remarks>
    /// The setter of this property can be invoked just once per application lifetime; 
    /// assigned session provider can not be changed.
    /// </remarks>
    /// <exception cref="NotSupportedException">Session provider is already assigned.</exception>
    public static Func<Pair<Session, IDisposable>> SessionProvider {
      protected get {
        return sessionProvider;
      }
      set {
        if (sessionProvider!=null)
          throw Exceptions.AlreadyInitialized("SessionProvider");
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        sessionProvider = value;
      }
    }

    /// <summary>
    /// Gets the <see cref="HttpModule"/> instance 
    /// bound to the current <see cref="HttpRequest"/>.
    /// </summary>
    public static HttpModule Current {
      get {
        var httpContext = HttpContext.Current;
        return httpContext==null ? null : (HttpModule) httpContext.Items[currentItemKey];
      }
      protected set {
        HttpContext.Current.Items[currentItemKey] = value;
      }
    }

    /// <summary>
    /// Gets the <see cref="HttpModule"/> instance 
    /// bound to the current <see cref="HttpRequest"/>.
    /// or throws <see cref="InvalidOperationException"/>, 
    /// if <see cref="Current"/> is <see langword="null" />.
    /// </summary>
    /// <returns>Current <see cref="HttpModule"/>.</returns>
    /// <exception cref="InvalidOperationException"><see cref="Current"/> is <see langword="null" />.</exception>
    public static HttpModule Demand()
    {
      var current = Current;
      if (current==null)
        throw new InvalidOperationException(Strings.ExThereIsNoCurrentHttpRequestOrHttpModuleIsnTBoundToItYet);
      return current;
    }

    /// <summary>
    /// Gets the domain used in web application.
    /// </summary>
    public static Domain Domain {
      get {
        EnsureDomainIsBuilt();
        return domain;
      }
    }

    /// <summary>
    /// Gets or sets the session for the current <see cref="HttpContext"/>.
    /// </summary>
    public static Session Session {
      get {
        var module = Current;
        if (module==null)
          return null;
        module.EnsureSessionIsProvided();
        return module.session;
      }
    }

    /// <summary>
    /// Gets a value indicating whether an error has occurred 
    /// on execution of the current request.
    /// </summary>
    public bool HasErrors
    {
      get { return hasErrors; }
      set { hasErrors = value; }
    }

    #region Protected BeginRequest, Error, EndRequest & ProvideSession methods

    /// <summary>
    /// Handles request beginning.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    protected virtual void BeginRequest(object sender, EventArgs e)
    {
      Current = this;
      HasErrors = false;
    }

    /// <summary>
    /// Handles request processing error.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    protected void Error(object sender, EventArgs e)
    {
      HasErrors = true;
    }

    /// <summary>
    /// Completes request processing.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    protected void EndRequest(object sender, EventArgs e)
    {
      lock (provideSessionLock) {
        var disposeOnEndRequestBackup = disposeOnEndRequest;
        disposeOnEndRequest = null;
        session = null;
        disposeOnEndRequestBackup.DisposeSafely();
      }
    }

    /// <summary>
    /// Default <see cref="SessionProvider"/> implementation.
    /// </summary>
    /// <returns>A pair of <see cref="Session"/> and <see cref="IDisposable"/> 
    /// to invoke on request completion.</returns>
    protected virtual Pair<Session, IDisposable> ProvideSession()
    {
      var sessionScope = Session.Open(Domain, false); // Open, but don't activate!
      var session = sessionScope.Session;
      var transactionScope = Transaction.Open(session);
      var toDispose = transactionScope.Join(sessionScope);
      return new Pair<Session, IDisposable>(session, new Disposable(disposing => {
        try {
          if (!Demand().HasErrors)
            transactionScope.Complete();
        }
        finally {
          toDispose.DisposeSafely();
        }
      }));
    }

    #endregion

    #region IHttpModule members

    /// <inheritdoc/>
    void IHttpModule.Init(HttpApplication context)
    {
      context.BeginRequest += BeginRequest;
      context.EndRequest += EndRequest;
      context.Error += Error;
      
      if (Session.Resolver==null)
        Session.Resolver = () => Session;
    }

    /// <inheritdoc/>
    void IHttpModule.Dispose()
    {
    }

    #endregion

    #region Private \ internal methods

    private static void EnsureDomainIsBuilt()
    {
      if (domain == null) lock (domainBuildLock) if (domain == null)
        domain = DomainBuilder.Invoke();
    }

    private void EnsureSessionIsProvided()
    {
      if (session==null) lock (provideSessionLock) if (session==null) {
        var pair = sessionProvider==null ? ProvideSession() : SessionProvider.Invoke();
        session = pair.First;
        disposeOnEndRequest = pair.Second;
      }
    }

    #endregion
  }
}