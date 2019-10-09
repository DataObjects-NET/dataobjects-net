using System;
using Xtensive.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using System.Threading.Tasks;
using System.Threading;

namespace Xtensive.Orm.Web
{
  /// <summary>
  /// Provides access to current <see cref="Session"/>.
  /// </summary>
  public class SessionManager
  {
    private static SessionManager current;
    private static Func<Pair<Session, IDisposable>> sessionProvider;

    private static AsyncLocal<Pair<Session, IDisposable>> sessionAndTransactionPair = new AsyncLocal<Pair<Session, IDisposable>>();
    private static readonly AsyncLocal<bool> hasErrors = new AsyncLocal<bool>();

    private readonly RequestDelegate nextMiddlewareRunner;

    /// <summary>
    /// Gets or sets a delegate which will be used to provide session instead of build-in mechanisms.
    /// </summary>
    public static Func<Pair<Session, IDisposable>> SessionProvider
    {
      protected get
      {
        return sessionProvider;
      }
      set
      {
        if (sessionProvider != null)
          throw Exceptions.AlreadyInitialized("SessionProvider");
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        sessionProvider = value;
      }
    }

    /// <summary>
    /// Gives access to current SessionManager.
    /// </summary>
    public static SessionManager Current
    {
      get
      {
        return current;
      }
    }

    /// <summary>
    /// Gives access to current SessionManager.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">SessionManager is not introduced to the pipeline or the the pipeline has not been configured yet.</exception>
    public static SessionManager Demand()
    {
      var current = Current;
      if (current == null)
        throw new InvalidOperationException("SessionManager is not introduced to the pipeline or the the pipeline has not been configured yet.");
      return current;
    }


    /// <summary>
    /// Shows whether session is opened
    /// </summary>
    public bool HasSession
    {
      get { return sessionAndTransactionPair.Value != null && sessionAndTransactionPair.Value.First != null; }
    }

    /// <summary>
    /// Gets provided session.
    /// </summary>
    /// <exception cref="InvalidOperationException">Session hasn't been opened yet</exception>
    public Session Session
    {
      get
      {
        EnsureSessionIsProvided();
        return sessionAndTransactionPair.Value.First;
      }
    }

    /// <summary>
    /// Gets or sets value indicating whether an error has occurred 
    /// on execution of the current request and transaction should be rollbacked.
    /// </summary>
    public bool HasErrors
    {
      get { return hasErrors.Value; }
      set { hasErrors.Value = value; }
    }

    /// <summary>
    /// Current step of pipeline executor
    /// </summary>
    /// <param name="context">The HttpContext for current pipeline execution</param>
    /// <returns>Task</returns>
    public async Task Invoke(HttpContext context)
    {
      BeforeActions(context);

      try
      {
        await nextMiddlewareRunner.Invoke(context);
      }
      catch (Exception)
      {
        HasErrors = true;
        throw;
      }
      finally
      {
        AfterActions(context);
      }

    }

    private static Domain GetDomain(HttpContext context)
    {
      return (Domain)context.RequestServices.GetService(typeof(Domain));
    }

    private static void EnableSessionResolver()
    {
      if (Session.Resolver == null)
        Session.Resolver = () => Demand().Session;
    }

    private static void DisableSessionResolver()
    {
      Session.Resolver = null;
    }

    private void BeforeActions(HttpContext context)
    {
      var domain = GetDomain(context);
      if (domain == null)
        throw new InvalidOperationException("There is no domain registered in services. Make sure you add it in Startup.ConfigureServices(IServiceCollection) method.");

      hasErrors.Value = false;
      ProvideSession(domain, context);
      EnableSessionResolver();
    }

    private void AfterActions(HttpContext context)
    {
      DisableSessionResolver();
      var pair = sessionAndTransactionPair.Value;
      sessionAndTransactionPair.Value = new Pair<Session, IDisposable>(null, null);

      pair.Second.Dispose();
      pair.First.Dispose();
    }

    private void ProvideSession(Domain domain, HttpContext context)
    {
      if (sessionAndTransactionPair.Value.First!=null)
        throw new InvalidOperationException("Session has already provided.");

      Pair<Session, IDisposable> pair;

      if (sessionProvider == null)
        pair = ProvideSessionInternal(domain, context);
      else
        pair = SessionProvider.Invoke();
      sessionAndTransactionPair.Value = pair;
    }

    private Pair<Session, IDisposable> ProvideSessionInternal(Domain domain, HttpContext context)
    {
      var newSession = domain.OpenSession(); // Open, but don't activate!
      var transactionScope = newSession.OpenTransaction();
      var newResource = transactionScope.Join(newSession);
      return new Pair<Session, IDisposable>(newSession, new Disposable(disposing => {
        try
        {
          if (!HasErrors)
            transactionScope.Complete();
        }
        finally
        {
          newResource.DisposeSafely();
        }
      }));
    }

    private void EnsureSessionIsProvided()
    {
      if (sessionAndTransactionPair.Value.First == null)
        throw new InvalidOperationException("Session is not provided");
    }

    public SessionManager(RequestDelegate next)
    {
      nextMiddlewareRunner = next;
      current = this;
    }
  }
}