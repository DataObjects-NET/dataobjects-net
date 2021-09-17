// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Filters;
using Xtensive.Orm.Web.Filters;
using Xtensive.Orm.Web.Middleware;

namespace Xtensive.Orm.Web
{
  /// <summary>
  /// Contains extensions which are used in Startup.cs to configure application
  /// </summary>
  public static class StartupConfigurationExtensions
  {
    /// <summary>
    /// Adds <see cref="OpenSessionMiddleware"/> to ASP.NET Core middleware pipeline.
    /// </summary>
    /// <param name="builder"><see cref="IApplicationBuilder"/> instance.</param>
    /// <returns><paramref name="builder"/> with <see cref="OpenSessionMiddleware"/>.</returns>
    public static IApplicationBuilder UseDataObjectsSessionOpener(this IApplicationBuilder builder) =>
      builder.UseMiddleware<OpenSessionMiddleware>();

    /// <summary>
    /// Registers a <see cref="SessionAccessor"/> as scoped service to be able to
    /// have access to session/transaction as parameter in either MVC controllers' actions or
    /// constuctors, and in Razor Pages.
    /// </summary>
    /// <param name="services">A service collection to add <see cref="SessionAccessor"/>.</param>
    /// <returns>The same collection instance with registered accessor.</returns>
    public static IServiceCollection AddDataObjectsSessionAccessor(this IServiceCollection services) =>
      services.AddScoped<SessionAccessor>();

    /// <summary>
    /// Adds <see cref="SessionActionFilter"/> to action filters.
    /// </summary>
    /// <param name="filters">The filter collection to add the filter.</param>
    /// <returns>A <see cref="IFilterMetadata"/> representing added filter.</returns>
    public static IFilterMetadata AddDataObjectsSessionActionFilter(this FilterCollection filters) =>
      AddDataObjectsSessionActionFilter(filters, 0);

    /// <summary>
    /// Adds <see cref="SessionActionFilter"/> to action filters.
    /// </summary>
    /// <param name="filters">The filter collection to add the filter.</param>
    /// <param name="order">The order of the added filter.</param>
    /// <returns>A <see cref="IFilterMetadata"/> representing added filter.</returns>
    public static IFilterMetadata AddDataObjectsSessionActionFilter(this FilterCollection filters, int order) =>
      filters.Add(typeof(SessionActionFilter), order);
  }
}