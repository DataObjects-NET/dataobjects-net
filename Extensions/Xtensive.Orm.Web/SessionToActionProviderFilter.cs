// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Xtensive.Core;

namespace Xtensive.Orm.Web.Filters
{
  /// <summary>
  /// Filter that 
  /// </summary>
  public class SessionToActionProviderFilter : IActionFilter, IAsyncActionFilter
  {
    private SessionAccessor sessionAccessor;
    private IDisposable contextBindResource;
    private bool skipResourcesRelease;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context) =>
      ExecuteBeforeAction(context, false).GetAwaiter().GetResult();

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) =>
      ExecuteAfterAction(context, false).GetAwaiter().GetResult();

    /// <inheritdoc/>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
      await ExecuteBeforeAction(context, true);

      var actionExecutionContext = await next.Invoke();

      await ExecuteAfterAction(actionExecutionContext, true);
    }

    /// <summary>
    /// Opens session.
    /// </summary>
    /// <param name="domain">Domain instance</param>
    /// <param name="context">Action executing context</param>
    /// <returns>Instance of <see cref="Session"/>.</returns>
    protected virtual Session OpenSession(Domain domain, ActionExecutingContext context) => domain.OpenSession();

    /// <summary>
    /// Opens session asynchronously.
    /// </summary>
    /// <param name="domain">Domain instance</param>
    /// <param name="context">Action executing context</param>
    /// <returns>Instance of <see cref="Session"/>.</returns>
    protected virtual Task<Session> OpenSessionAsync(Domain domain, ActionExecutingContext context) => domain.OpenSessionAsync();

    /// <summary>
    /// Opens transaction scope.
    /// </summary>
    /// <param name="session">Domain instance.</param>
    /// <param name="context">Action executing context.</param>
    /// <returns>Instance of <see cref="TransactionScope"/>.</returns>
    protected virtual TransactionScope OpenTransaction(Session session, ActionExecutingContext context) =>
      session.OpenTransaction();

    /// <summary>
    /// Opens transaction scope asynchronously.
    /// </summary>
    /// <param name="session">Domain instance.</param>
    /// <param name="context">Action executing context.</param>
    /// <returns>Instance of <see cref="TransactionScope"/>.</returns>
    protected virtual async Task<TransactionScope> OpenTransactionAsync(Session session, ActionExecutingContext context) =>
      await session.OpenTransactionAsync();

    /// <summary>
    /// Executes before <see cref="TransactionScope"/> dispose.
    /// </summary>
    /// <param name="transactionScope">The transaction scope which is about to be disposed.</param>
    /// <param name="session">The session transaction scope belongs to.</param>
    /// <param name="context">The <see cref="ActionExecutedContext"/>.</param>
    protected virtual void OnTransactionScopeDisposing(TransactionScope transactionScope, Session session, ActionExecutedContext context)
    {
    }

    /// <summary>
    /// Executes after <see cref="TransactionScope"/> is dispose.
    /// </summary>
    /// <param name="session">The session the disposed transaction scope belonged to.</param>
    /// <param name="context">The <see cref="ActionExecutedContext"/>.</param>
    protected virtual void OnTransactionScopeDisposed(Session session, ActionExecutedContext context)
    {
    }

    /// <summary>
    /// Executes before <see cref="Session"/> dispose.
    /// </summary>
    /// <param name="session">The session which is about to be disposed.</param>
    /// <param name="context">The <see cref="ActionExecutedContext"/>.</param>
    protected virtual void OnSessionDisposing(Session session, ActionExecutedContext context)
    {
    }

    /// <summary>
    /// Executes after <see cref="Session"/> is disposed.
    /// </summary>
    /// <param name="context">The <see cref="ActionExecutedContext"/>.</param>
    protected virtual void OnSessionDisposed(ActionExecutedContext context)
    {
    }

    //internal void Persist(PersistReason reason) => Persist(reason, false).GetAwaiter().GetResult();
    private async ValueTask ExecuteBeforeAction(ActionExecutingContext context, bool isAsync)
    {
      var actionParameters = context.ActionDescriptor.Parameters;

      //this search can probably be improved by caching action names with needed parameters
      foreach (var p in actionParameters) {
        if (p.ParameterType == WellKnownTypes.SessionAccessorType) {
          // trying to get registered accessor as service
          var accessor = GetSessionAccesorFromServices(context.HttpContext);
          if (accessor != null) {
            // this is registered as service and probably filled with middlewere
            skipResourcesRelease = true;
            sessionAccessor = accessor;
            if (accessor.ContextIsBound) {
              return;// middleware is in pipeline, it did the work
            }
            else {
              contextBindResource = await CreateSessionAndBindContext(sessionAccessor, context, isAsync);
              skipResourcesRelease = false;
            }
          }
          else {
            //we're using the instance Asp.Net infrastructure created for us
            sessionAccessor = (SessionAccessor) context.ActionArguments[p.Name];
            contextBindResource = await CreateSessionAndBindContext(sessionAccessor, context, isAsync);
            skipResourcesRelease = false;
          }
          break;
        }
      }
    }

    private async ValueTask ExecuteAfterAction(ActionExecutedContext context, bool isAsync)
    {
      if (skipResourcesRelease) {
        return;
      }
      // session and tx is created here
      // so we need to clean HttpContext.
      var session = sessionAccessor.Session;
      var tx = sessionAccessor.TransactionScope;

      var httpContext = context.HttpContext;
      _ = httpContext.Items.Remove(SessionAccessor.ScopeIdentifier);
      _ = httpContext.Items.Remove(SessionAccessor.SessionIdentifier);

      OnTransactionScopeDisposing(tx, session, context);
      if (isAsync) {
        await tx.DisposeSafelyAsync();
      }
      else {
        tx.DisposeSafely();
      }
      OnTransactionScopeDisposed(session, context);

      OnSessionDisposing(session, context);
      if (isAsync) {
        await session.DisposeAsync();
      }
      else {
        session.Dispose();
      }
      OnSessionDisposed(context);

      contextBindResource.DisposeSafely(); // unbind httpcontext
      sessionAccessor = null;
    }

    private async ValueTask<IDisposable> CreateSessionAndBindContext(SessionAccessor accessor, ActionExecutingContext context, bool isAsync)
    {
      var httpContext = context.HttpContext;
      var domain = GetDomainFromServices(httpContext);

      Session session;
      TransactionScope tx;
      if (isAsync) {
        session = await OpenSessionAsync(domain, context);
        tx = await OpenTransactionAsync(session, context);
      }
      else {
        session = OpenSession(domain, context);
        tx = OpenTransaction(session, context);
      }
      httpContext.Items.Add(SessionAccessor.SessionIdentifier, session);
      httpContext.Items.Add(SessionAccessor.ScopeIdentifier, tx);
      return accessor.BindHttpContext(httpContext);
    }

    private static Domain GetDomainFromServices(HttpContext context)
    {
      var domain = (Domain) context.RequestServices.GetService(WellKnownTypes.DomainType);
      return domain == null
        ? throw new InvalidOperationException("Domain is not found among registered services.")
        : domain;
    }

    private static SessionAccessor GetSessionAccesorFromServices(HttpContext context) =>
      (SessionAccessor) context.RequestServices.GetService(WellKnownTypes.SessionAccessorType);
  }
}
