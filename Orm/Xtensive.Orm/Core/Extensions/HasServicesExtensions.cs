// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.15

using System;
using Xtensive.Core;
using Xtensive.Reflection;


namespace Xtensive.Core
{
  /// <summary>
  /// <see cref="IHasServices"/> related extension methods.
  /// </summary>
  public static class HasServicesExtensions
  {
    /// <summary>
    /// Gets the service of specified type <typeparamref name="T"/>;
    /// throws <see cref="InvalidOperationException"/>, if there is no such service.
    /// </summary>
    /// <typeparam name="T">The type of service to get.</typeparam>
    /// <param name="serviceProvider">The service provider to query for the service.</param>
    /// <param name="failIfNone">If set to <see langword="true"/>, an exception will be thrown 
    /// if there is no requested service.</param>
    /// <returns>Requested service;
    /// <see langword="null" />, if <paramref name="failIfNone"/>==<see langword="false" /> 
    /// and there is no requested service.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="failIfNone"/>==<see langword="true" /> 
    /// and there is no requested service.</exception>
    public static T GetService<T>(this IHasServices serviceProvider, bool failIfNone)
      where T: class
    {
      ArgumentValidator.EnsureArgumentNotNull(serviceProvider, "serviceProvider");
      var service = serviceProvider.GetService<T>();
      if (failIfNone && service == null)
        throw new InvalidOperationException(
          string.Format(Strings.ExServiceNotFound, typeof(T).GetShortName()));
      return service;
    }

    /// <summary>
    /// Gets the service of the specified <paramref name="serviceType"/>;
    /// throws <see cref="InvalidOperationException"/>, if there is no such service.
    /// </summary>
    /// <param name="serviceProvider">The service provider to query for the service.</param>
    /// <param name="serviceType">Type of the service to get.</param>
    /// <param name="failIfNone">If set to <see langword="true"/>, an exception will be thrown 
    /// if there is no requested service.</param>
    /// <returns>Requested service;
    /// <see langword="null" />, if <paramref name="failIfNone"/>==<see langword="false" /> 
    /// and there is no requested service.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="failIfNone"/>==<see langword="true" /> 
    /// and there is no requested service.</exception>
    public static object GetService(this IHasServices serviceProvider, Type serviceType, bool failIfNone)
    {
      var getServiceInternal = DelegateHelper.CreateDelegate<Func<IHasServices, bool, object>>(
        null, typeof (HasServicesExtensions), "GetServiceInternal", serviceType);
      return getServiceInternal(serviceProvider, failIfNone);
    }

    /// <summary>
    /// Gets the service of the specified <paramref name="serviceType"/>.
    /// </summary>
    /// <param name="serviceProvider">The service provider to query for the service.</param>
    /// <param name="serviceType">Type of the service to get.</param>
    /// <returns>The service of specified type.</returns>
    public static object GetService(this IHasServices serviceProvider, Type serviceType)
    {
      return serviceProvider.GetService(serviceType, false);
    }

    private static object GetServiceInternal<T>(this IHasServices serviceProvider, bool failIfNone)
      where T: class
    {
      return serviceProvider.GetService<T>(failIfNone);
    }
  }
}