// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Core;
using Microsoft.AspNetCore.Http;

namespace Xtensive.Orm.Web
{
  /// <summary>
  /// A wrapper that provides access <see cref="Xtensive.Orm.Session">Session</see>
  /// and <see cref="Xtensive.Orm.TransactionScope">TransactionScope</see>.
  /// </summary>
  public sealed class SessionAccessor
  {
    internal static readonly object SessionIdentifier = new object();
    internal static readonly object ScopeIdentifier = new object();

    private HttpContext context;

    /// <summary>
    /// Provides session from bound to <see cref="HttpContext"/>.
    /// </summary>
    public Session Session =>
      context != null && context.Items.TryGetValue(SessionIdentifier, out var instance)
        ? (Session) instance
        : null;

    /// <summary>
    /// Provides opened transaction scope bound to <see cref="HttpContext"/>.
    /// </summary>
    public TransactionScope TransactionScope =>
      context != null && context.Items.TryGetValue(TransactionScope, out var instance)
        ? (TransactionScope) instance
        : null;

    internal bool ContextIsBound => context != null;

    internal IDisposable BindHttpContext(HttpContext context)
    {
      this.context = context;
      return new Disposable(NullTheContext);
    }

    private void NullTheContext(bool disposing) => context = null;
  }
}
