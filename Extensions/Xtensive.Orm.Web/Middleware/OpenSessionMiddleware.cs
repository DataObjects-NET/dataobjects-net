// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;

namespace Xtensive.Orm.Web.Middleware
{
  /// <summary>
  /// DataObjects.Net Session providing middleware.
  /// </summary>
  public class OpenSessionMiddleware
  {
    private readonly RequestDelegate next;

    /// <summary>
    /// Opens session and puts it to registered <see cref="SessionAccessor"/> service before
    /// next middleware execution and releases resources when execution returns.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/>.</param>
    /// <param name="domainFromServices"><see cref="Domain"/> regestered as service.</param>
    /// <param name="sessionAccessor"><see cref="SessionAccessor"/> registered as service.</param>
    /// <returns>Task perfroming this operation.</returns>
    public async Task Invoke(HttpContext context, Domain domainFromServices, SessionAccessor sessionAccessor)
    {
      var session = await OpenSessionAsync(domainFromServices, context);
      var transaction = await OpenTransactionAsync(session, context);
      BindResourcesToContext(session, transaction, context);

      using (sessionAccessor.BindHttpContext(context)) {
        try {
          await next.Invoke(context);
          transaction.Complete();
        }
        catch (Exception exception) {
          // if we caught exception here then
          // 1) it is unhendeled exception
          // or
          // 2) it was thrown intentionally
          if (CompleteTransactionOnException(exception, context)) {
            transaction.Complete();
          }
          if (RethrowException(exception, context)) {
            ExceptionDispatchInfo.Throw(exception);
          }
        }
        finally {
          RemoveResourcesFromContext(context);
          await OnTransactionDisposingAsync(transaction, session, context);
          await transaction.DisposeAsync();
          await OnTransactionDisposedAsync(session, context);

          await OnSessionDisposingAsync(session, context);
          await session.DisposeAsync();
          await OnSessionDisposedAsync(context);
        }
      }
    }

    /// <summary>
    /// Opens <see cref="Session"/> asynchronously.
    /// </summary>
    /// <param name="domain">A <see cref="Domain"/> instance.</param>
    /// <param name="context">The <see cref="HttpContext"/>.</param>
    /// <returns>Instance of <see cref="Session"/>.</returns>
    protected virtual Task<Session> OpenSessionAsync(Domain domain, HttpContext context) => domain.OpenSessionAsync();

    /// <summary>
    /// Opens <see cref="TransactionScope"/> asynchronously.
    /// </summary>
    /// <param name="session">A <see cref="Session"/> instance to open transaction for.</param>
    /// <param name="context">The <see cref="HttpContext"/>.</param>
    /// <returns>Instance of <see cref="TransactionScope"/>.</returns>
    protected virtual async Task<TransactionScope> OpenTransactionAsync(Session session, HttpContext context) =>
      await session.OpenTransactionAsync();

    /// <summary>
    /// Executes before <paramref name="transactionScope"/> disposing.
    /// </summary>
    /// <param name="transactionScope">The <see cref="TransactionScope"/> to be disposed.</param>
    /// <param name="session">The <see cref="Session"/> the <paramref name="transactionScope"/> belongs to.</param>
    /// <param name="context">The <see cref="HttpContext"/>.</param>
    /// <returns>Task performing this operation.</returns>
    protected virtual ValueTask OnTransactionDisposingAsync(TransactionScope transactionScope, Session session, HttpContext context) => default;

    /// <summary>
    /// Executes after <see cref="TransactionScope"/> disposed.
    /// </summary>
    /// <param name="session">The <see cref="Session"/> disposed scoped belonged too.</param>
    /// <param name="context">The <see cref="HttpContext"/>.</param>
    /// <returns>Task performing this operation.</returns>
    protected virtual ValueTask OnTransactionDisposedAsync(Session session, HttpContext context) => default;

    /// <summary>
    /// Executes before <paramref name="session"/> disposing.
    /// </summary>
    /// <param name="session">The <see cref="Session"/> to be disposed.</param>
    /// <param name="context">The <see cref="HttpContext"/>.</param>
    /// <returns>Task performing this operation.</returns>
    protected virtual ValueTask OnSessionDisposingAsync(Session session, HttpContext context) => default;

    /// <summary>
    /// Executes after <see cref="Session"/> disposed.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/>.</param>
    /// <returns>Task performing this operation.</returns>
    protected virtual ValueTask OnSessionDisposedAsync(HttpContext context) => default;

    /// <summary>
    /// Determines whether transaction should be completed even though exception appeared.
    /// </summary>
    /// <param name="thrownException">Thrown exception instance.</param>
    /// <param name="context">The <see cref="HttpContext"/>.</param>
    /// <returns><see langword="true"/>if transaction should be rollbacked, otherwise, <see langword="false"/>.</returns>
    protected virtual bool CompleteTransactionOnException(Exception thrownException, HttpContext context) => false;

    /// <summary>
    /// Determines whether exception should be re-thrown.
    /// </summary>
    /// <param name="thrownException">Thrown exception instance.</param>
    /// <param name="context">The <see cref="HttpContext"/>.</param>
    /// <returns><see langword="true"/>if the exception should be re-thrown, otherwise, <see langword="false"/>.</returns>
    protected virtual bool RethrowException(Exception thrownException, HttpContext context) => true;

    private static void BindResourcesToContext(Session session, TransactionScope transactionScope, HttpContext context)
    {
      context.Items[SessionAccessor.SessionIdentifier] = session;
      context.Items[SessionAccessor.ScopeIdentifier] = transactionScope;
    }

    private static void RemoveResourcesFromContext(HttpContext context)
    {
      _ = context.Items.Remove(SessionAccessor.ScopeIdentifier);
      _ = context.Items.Remove(SessionAccessor.SessionIdentifier);
    }

    /// <summary>
    /// Creates an instance of <see cref="OpenSessionMiddleware"/>
    /// </summary>
    /// <param name="next"></param>
    public OpenSessionMiddleware(RequestDelegate next)
    {
      this.next = next;
    }
  }
}