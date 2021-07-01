// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;

namespace Xtensive.Orm.Web
{

  /// <summary>
  /// DataObjects.Net Session providing middleware.
  /// </summary>
  public class OpenSessionMiddlewere
  {
    private readonly RequestDelegate next;

    /// <summary>
    /// Opens session and puts it to registered <see cref="SessionAccessor"/> service before
    /// next middleware execution and releases resources when execution returns.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/>.</param>
    /// <returns>Task perfroming this operation.</returns>
    public async Task Invoke(HttpContext context)
    {
      var sessionAccessor = (SessionAccessor) context.RequestServices.GetService(WellKnownTypes.SessionAccessorType);
      if (sessionAccessor != null) {
        var domain = GetDomainFromServices(context);
        var session = await OpenSessionAsync(domain, context);
        var transaction = await OpenTransactionAsync(session, context);
        BindResourcesToContext(session, transaction, context);

        using (sessionAccessor.BindHttpContext(context)) {
          try {
            await next.Invoke(context);
          }
          catch(Exception exception) {
            // if we caught exception here then
            // 1) it is unhendeled exception
            // or
            // 2) it was thrown intentionally
            if(CompleteTransactionOnException(exception, context)) {
              transaction.Complete();
            }
            if(RethrowException(exception, context)) {
              ExceptionDispatchInfo.Throw(exception);
            }
          }
          finally {
            RemoveResourcesFromContext(context);
            await transaction.DisposeAsync();
            await session.DisposeAsync();
          }
        }
      }
      else {
        await next.Invoke(context);
      }
    }

    /// <summary>
    /// Opens session asynchronously.
    /// </summary>
    /// <param name="domain">Domain instance</param>
    /// <param name="context">Action executing context</param>
    /// <returns>Instance of <see cref="Session"/>.</returns>
    protected virtual Task<Session> OpenSessionAsync(Domain domain, HttpContext context) => domain.OpenSessionAsync();

    /// <summary>
    /// Opens transaction scope asynchronously.
    /// </summary>
    /// <param name="session">Domain instance.</param>
    /// <param name="context">Action executing context.</param>
    /// <returns>Instance of <see cref="TransactionScope"/>.</returns>
    protected virtual async Task<TransactionScope> OpenTransactionAsync(Session session, HttpContext context) =>
      await session.OpenTransactionAsync();

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

    private static Domain GetDomainFromServices(HttpContext context)
    {
      var domain = (Domain) context.RequestServices.GetService(WellKnownTypes.DomainType);
      return domain == null
        ? throw new InvalidOperationException("Domain is not found among registered services.")
        : domain;
    }

    /// <summary>
    /// Creates an instance of <see cref="OpenSessionMiddlewere"/>
    /// </summary>
    /// <param name="next"></param>
    public OpenSessionMiddlewere(RequestDelegate next)
    {
      this.next = next;
    }
  }
}
