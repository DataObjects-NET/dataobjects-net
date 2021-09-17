// Copyright (C) 2019-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Core;
using Microsoft.AspNetCore.Builder;

namespace Xtensive.Orm.Web
{
  /// <summary>
  /// Contains helper methods to apply <see cref="SessionManager"/> middleware to ASP.NET Core pipeline.
  /// </summary>
  [Obsolete]
  public static class ApplicationBuilderExtensions
  {
    /// <summary>
    /// Adds <see cref="SessionManager"/> to ASP.NET Core middleware pipeline.
    /// </summary>
    /// <param name="builder"><see cref="IApplicationBuilder"/> instance.</param>
    /// <returns><paramref name="builder"/> with <see cref="SessionManager"/>.</returns>
    public static IApplicationBuilder UseSessionManager(this IApplicationBuilder builder)
    {
      return builder.UseMiddleware<SessionManager>();
    }

    /// <summary>
    /// Adds <see cref="SessionManager"/> to ASP.NET Core middleware pipeline.
    /// </summary>
    /// <param name="builder"><see cref="IApplicationBuilder"/> instance.</param>
    /// <param name="sessionProvider">User-defined session provider which will be used instead of built-in provider.</param>
    /// <returns><paramref name="builder"/> with <see cref="SessionManager"/>.</returns>
    public static IApplicationBuilder UseSessionManager(this IApplicationBuilder builder, Func<Pair<Session, System.IDisposable>> sessionProvider)
    {
      SessionManager.SessionProvider = sessionProvider;
      return builder.UseMiddleware<SessionManager>();
    }
  }
}
