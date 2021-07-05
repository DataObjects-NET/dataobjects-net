// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Xtensive.Orm.Web
{
  /// <summary>
  /// Contains extensions which are used in Startup.cs to configure application
  /// </summary>
  public static class StartupConfigurationExtensions
  {
    /// <summary>
    /// Adds <see cref="SessionManager"/> to ASP.NET Core middleware pipeline.
    /// </summary>
    /// <param name="builder"><see cref="IApplicationBuilder"/> instance.</param>
    /// <returns><paramref name="builder"/> with <see cref="SessionManager"/>.</returns>
    public static IApplicationBuilder UseDataObjectsSessionsOpener(this IApplicationBuilder builder)
    {
      return builder.UseMiddleware<OpenSessionMiddlewere>();
    }

    /// <summary>
    /// Registers a SessionAccessor as scoped service for futher usage with
    /// </summary>
    /// <param name="services"></param>
    /// <returns>The same instance collection with service.</returns>
    public static IServiceCollection AddSessionAccessor(this IServiceCollection services)
    {
      return services.AddScoped<SessionAccessor>();
    }
  }
}
