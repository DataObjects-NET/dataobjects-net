// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Core;
using Microsoft.AspNetCore.Http;

namespace Xtensive.Orm.Web
{
  /// <summary>
  /// A wrapper that provides access <see cref="Xtensive.Orm.Session"></see>
  /// and <see cref="Xtensive.Orm.TransactionScope"> that are in <see cref="HttpContext"/>.</see>.
  /// </summary>
  public sealed class SessionAccessor
  {
    internal static readonly object SessionIdentifier = new object();
    internal static readonly object ScopeIdentifier = new object();

    private HttpContext context;

    /// <summary>
    /// Provides <see cref="Orm.Session"/> from bound to <see cref="HttpContext"/>.
    /// If no <see cref="Orm.Session"/> instance found, return <see langword="null"/>.
    /// </summary>
    public Session Session =>
      context != null && context.Items.TryGetValue(SessionIdentifier, out var instance)
        ? (Session) instance
        : null;

    /// <summary>
    /// Provides opened <see cref="Orm.TransactionScope"/> bound to <see cref="HttpContext"/>.
    /// If no <see cref="Orm.Session"/> instance found, return <see langword="null"/>.
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