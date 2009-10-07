// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.01

using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;

namespace Xtensive.Core.IoC
{
  /// <summary>
  /// <see cref="Microsoft.Practices.ServiceLocation.ServiceLocator"/> wrapper.
  /// </summary>
  [Serializable]
  public static class ServiceLocator
  {
    /// <summary>
    /// Gets the current <see cref="IServiceLocator"/> implementation.
    /// </summary>
    /// <returns>Current <see cref="IServiceLocator"/> implementation.</returns>
    private static IServiceLocator GetImplementation()
    {
      // Note: Do not cache current locator instance. It can be changed anytime.
      var implementation = Microsoft.Practices.ServiceLocation.ServiceLocator.Current;

      if (implementation == null)
        throw new InvalidOperationException("Unable to get current service locator.");

      return implementation;
    }

    /// <summary>
    /// Get all instances of the given currently registered in the container. 
    /// </summary>
    /// <typeparam name="TService">Type of object requested.</typeparam>
    /// <returns>A sequence of instances of the requested <typeparamref name="TService"/>.</returns>
    /// <exception cref="ActivationException">if there is are errors resolving the service instance.</exception>
    public static IEnumerable<TService> GetAllInstances<TService>()
    {
      return GetImplementation().GetAllInstances<TService>();
    }

    /// <summary>
    /// Get all instances of the given currently registered in the container. 
    /// </summary>
    /// <param name="serviceType">Type of object requested.</typeparam>
    /// <returns>A sequence of instances of the requested <paramref name="serviceType"/>.</returns>
    /// <exception cref="ActivationException">if there is are errors resolving the service instance.</exception>
    public static IEnumerable<object> GetAllInstances(Type serviceType)
    {
      return GetImplementation().GetAllInstances(serviceType);
    }

    /// <summary>
    /// Get an instance of the given <typeparamref name="TService"/>.
    /// </summary>
    /// <typeparam name="TService">Type of object requested.</typeparam>
    /// <returns>The requested service instance.</returns>
    /// <exception cref="ActivationException">if there is are errors resolving the service instance.</exception>
    public static TService GetInstance<TService>()
    {
      return GetImplementation().GetInstance<TService>();
    }

    /// <summary>
    /// Get an instance of the given <typeparamref name="TService"/>.
    /// </summary>
    /// <typeparam name="TService">Type of object requested.</typeparam>
    /// <param name="key">Name the object was registered with.</param>
    /// <returns>The requested service instance.</returns>
    /// <exception cref="ActivationException">if there is are errors resolving the service instance.</exception>
    public static TService GetInstance<TService>(string key)
    {
      return GetImplementation().GetInstance<TService>(key);
    }

    /// <summary>
    /// Get an instance of the given <paramref name="serviceType"/>.
    /// </summary>
    /// <param name="serviceType">Type of object requested.</typeparam>
    /// <returns>The requested service instance.</returns>
    /// <exception cref="ActivationException">if there is are errors resolving the service instance.</exception>
    public static object GetInstance(Type serviceType)
    {
      return GetImplementation().GetInstance(serviceType);
    }

    /// <summary>
    /// Get an instance of the given <paramref name="serviceType"/>.
    /// </summary>
    /// <param name="serviceType">Type of object requested.</typeparam>
    /// <param name="key">Name the object was registered with.</param>
    /// <returns>The requested service instance.</returns>
    /// <exception cref="ActivationException">if there is are errors resolving the service instance.</exception>
    public static object GetInstance(Type serviceType, string key)
    {
      return GetImplementation().GetInstance(serviceType, key);
    }

    /// <summary>
    /// Set the delegate that is used to retrieve the current container.
    /// </summary>
    /// <param name="newProvider">Delegate that, when called, will return
    /// the current ambient container.</param>
    public static void SetLocatorProvider(ServiceLocatorProvider newProvider)
    {
      Microsoft.Practices.ServiceLocation.ServiceLocator.SetLocatorProvider(newProvider);
    }
  }
}