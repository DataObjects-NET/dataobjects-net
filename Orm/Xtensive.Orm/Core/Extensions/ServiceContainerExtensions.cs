// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.02

using System;
using JetBrains.Annotations;
using Xtensive.Reflection;
using Xtensive.IoC;


namespace Xtensive.Core
{
  /// <summary>
  /// <see cref="IServiceContainer"/> related extension methods.
  /// </summary>
  [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
  public static class ServiceContainerExtensions
  {
    /// <summary>
    /// Demands the specified service 
    /// using <see cref="IServiceContainer.Get{TService}()"/> method
    /// and ensures the result is not <see langword="null" />.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <param name="container">The container to demand the service on.</param>
    /// <returns></returns>
    /// <exception cref="ActivationException">There was an error on activation of some instance(s),
    /// or service is not available.</exception>
    public static TService Demand<TService>(this IServiceContainer container)
    {
      var service = container.Get<TService>();
      EnsureNotNull(service, null);
      return service;
    }

    /// <summary>
    /// Demands the specified service
    /// using <see cref="IServiceContainer.Get{TService}(string)"/> method
    /// and ensures the result is not <see langword="null"/>.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <param name="container">The container to demand the service on.</param>
    /// <param name="name">The service name.</param>
    /// <returns></returns>
    /// <exception cref="ActivationException">There was an error on activation of some instance(s),
    /// or service is not available.</exception>
    public static TService Demand<TService>(this IServiceContainer container, string name)
    {
      var service = container.Get<TService>(name);
      EnsureNotNull(service, name);
      return service;
    }

    /// <summary>
    /// Demands the specified service
    /// using <see cref="IServiceContainer.Get(System.Type)"/> method
    /// and ensures the result is not <see langword="null"/>.
    /// </summary>
    /// <param name="container">The container to demand the service on.</param>
    /// <param name="serviceType">Type of the service.</param>
    /// <returns></returns>
    /// <exception cref="ActivationException">There was an error on activation of some instance(s),
    /// or service is not available.</exception>
    public static object Demand(this IServiceContainer container, Type serviceType)
    {
      var service = container.Get(serviceType);
      EnsureNotNull(service, serviceType, null);
      return service;
    }

    /// <summary>
    /// Demands the specified service
    /// using <see cref="IServiceContainer.Get(System.Type,string)"/> method
    /// and ensures the result is not <see langword="null"/>.
    /// </summary>
    /// <param name="container">The container to demand the service on.</param>
    /// <param name="serviceType">Type of the service.</param>
    /// <param name="name">The service name.</param>
    /// <returns></returns>
    /// <exception cref="ActivationException">There was an error on activation of some instance(s),
    /// or service is not available.</exception>
    public static object Demand(this IServiceContainer container, Type serviceType, string name)
    {
      var service = container.Get(serviceType, name);
      EnsureNotNull(service, serviceType, name);
      return service;
    }

    #region Private / internal methods

    private static void EnsureNotNull<TService>(TService service, string name)
    {
      if (service!=null)
        return;
      EnsureNotNull(service, typeof(TService), name);
    }

    /// <exception cref="ActivationException">Service is not available.</exception>
    private static void EnsureNotNull(object service, Type serviceType, string name)
    {
      if (service!=null)
        return;
      if (name==null)
        throw new ActivationException(
          Strings.ExServiceOfTypeXIsNotAvailable.FormatWith(
            serviceType.GetShortName()));
      else {
        object arg1 = serviceType.GetShortName();
        throw new ActivationException(string.Format(Strings.ExServiceWithNameXOfTypeYIsNotAvailable, name, arg1));
      }
    }

    #endregion
  }
}