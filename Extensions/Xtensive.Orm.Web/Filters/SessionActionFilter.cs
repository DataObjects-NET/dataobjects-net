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
  /// DataObjects.Net Session providing action filter.
  /// </summary>
  public class SessionActionFilter : IActionFilter, IAsyncActionFilter
  {
    private IDisposable contextBindResource;
    private bool skipResourcesDisposal;

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
    /// Opens <see cref="Session"/>.
    /// </summary>
    /// <param name="domain"><see cref="Domain"/> instance.</param>
    /// <param name="context">Action executing context.</param>
    /// <returns>Instance of <see cref="Session"/>.</returns>
    protected virtual Session OpenSession(Domain domain, ActionExecutingContext context) => domain.OpenSession();

    /// <summary>
    /// Opens <see cref="Session"/> asynchronously.
    /// </summary>
    /// <param name="domain">A <see cref="Domain"/> instance.</param>
    /// <param name="context">The <see cref="ActionExecutingContext"/>.</param>
    /// <returns>Instance of <see cref="Session"/>.</returns>
    protected virtual Task<Session> OpenSessionAsync(Domain domain, ActionExecutingContext context) => domain.OpenSessionAsync();

    /// <summary>
    /// Opens <see cref="TransactionScope"/>.
    /// </summary>
    /// <param name="session">The <see cref="Session"/> to open transaction scope.</param>
    /// <param name="context">The <see cref="ActionExecutingContext"/>.</param>
    /// <returns>Instance of <see cref="TransactionScope"/>.</returns>
    protected virtual TransactionScope OpenTransaction(Session session, ActionExecutingContext context) =>
      session.OpenTransaction();

    /// <summary>
    /// Opens <see cref="TransactionScope"/> asynchronously.
    /// </summary>
    /// <param name="session">The <see cref="Session"/> to open transaction scope.</param>
    /// <param name="context">The <see cref="ActionExecutingContext"/>.</param>
    /// <returns>Instance of <see cref="TransactionScope"/>.</returns>
    protected virtual async Task<TransactionScope> OpenTransactionAsync(Session session, ActionExecutingContext context) =>
      await session.OpenTransactionAsync();

    /// <summary>
    /// Executes before <paramref name="transactionScope"/> disposing.
    /// </summary>
    /// <param name="transactionScope">The transaction scope which is about to be disposed.</param>
    /// <param name="session">The <see cref="Session"/> transaction scope belongs to.</param>
    /// <param name="context">The <see cref="ActionExecutedContext"/>.</param>
    protected virtual void OnTransactionScopeDisposing(TransactionScope transactionScope, Session session, ActionExecutedContext context)
    {
      if (context.Exception == null || CompleteTransactionOnException(context.Exception, context)) {
        transactionScope.Complete();
      }
    }

    /// <summary>
    /// Executes after <see cref="TransactionScope"/> is disposed.
    /// </summary>
    /// <param name="session">The <see cref="Session"/> the disposed transaction scope belonged to.</param>
    /// <param name="context">The <see cref="ActionExecutedContext"/>.</param>
    protected virtual void OnTransactionScopeDisposed(Session session, ActionExecutedContext context)
    {
    }

    /// <summary>
    /// Executes before <paramref name="session"/> disposing.
    /// </summary>
    /// <param name="session">The <see cref="Session"/> which is about to be disposed.</param>
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


    /// <summary>
    /// Allows to define what exceptions will not leat to transaction rollback.
    /// </summary>
    /// <param name="exception">The exception thrown by action.</param>
    /// <param name="context">The <see cref="ActionExecutedContext"/>.</param>
    /// <returns><see langword="true"/> for complete transaction scope, otherwise, <see langword="false"/>.</returns>
    protected virtual bool CompleteTransactionOnException(Exception exception, ActionExecutedContext context) => false;

    private async ValueTask ExecuteBeforeAction(ActionExecutingContext context, bool isAsync)
    {
      var actionParameters = context.ActionDescriptor.Parameters;

      skipResourcesDisposal = true;

      // accessor is always in services as scoped instance (one per request)
      // so, if there is no session opened we need to open it,
      // put it to the httpcontext and bind it to the first mention of SessionAccessor

      //this search can probably be improved by caching action names with needed parameters
      foreach (var p in actionParameters) {
        if (p.ParameterType == WellKnownTypes.SessionAccessorType) {
          // trying to get registered accessor as service
          var accessor = GetSessionAccesorFromServices(context.HttpContext);
          if (!accessor.ContextIsBound) {
            if (SessionInContextExists(context.HttpContext)) {
              // something opened session but does not bind context to accessor
              // bind context and let outer code manage Session instance.
              contextBindResource = accessor.BindHttpContext(context.HttpContext);
              skipResourcesDisposal = true;
            }
            else {
              // nothing is in context, create and bind,
              // also remember that we need to dispose opened session and transaction scope
              contextBindResource = await CreateSessionAndBindContext(accessor, context, isAsync);
              skipResourcesDisposal = false;
            }
          }
          break;
        }
      }
    }

    private async ValueTask ExecuteAfterAction(ActionExecutedContext context, bool isAsync)
    {
      contextBindResource.DisposeSafely();
      if (skipResourcesDisposal) {
        return;
      }

      // session and tx is created here
      // so we need to clean HttpContext.

      var httpContext = context.HttpContext;
      var session = (Session) httpContext.Items[SessionAccessor.SessionIdentifier];
      var tx = (TransactionScope) httpContext.Items[SessionAccessor.ScopeIdentifier];

      if (session == null || tx == null) {
        // just in case user clears items, this will cause session leakage.
        throw new InvalidOperationException("Session or TransactionScope no longer exists in HttpContext. Do not remove it from HttpContext manually.");
      }

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
      return domain ?? throw new InvalidOperationException("Domain is not found among registered services.");
    }

    private static SessionAccessor GetSessionAccesorFromServices(HttpContext context)
    {
      var sessionAccessor = (SessionAccessor) context.RequestServices.GetService(WellKnownTypes.SessionAccessorType);
      return sessionAccessor ?? throw new InvalidOperationException("SessionAccessor should be registered as Scoped service.");
    }

    private static bool SessionInContextExists(HttpContext context) =>
      context.Items.TryGetValue(SessionAccessor.SessionIdentifier, out var sessionRaw) && sessionRaw != null;
  }
}