// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.01

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ServiceLocation;

namespace Xtensive.Core.IoC
{
  /// <summary>
  /// <see cref="Microsoft.Practices.ServiceLocation.ServiceLocator"/> wrapper.
  /// </summary>
  [Serializable]
  public static class ServiceLocator
  {
    private static IServiceLocator defaultLocator;

    /// <summary>
    /// Gets the current <see cref="IServiceLocator"/> implementation.
    /// </summary>
    /// <returns>Current <see cref="IServiceLocator"/> implementation.</returns>
    private static IServiceLocator GlobalLocator
    {
      get
      {
        // Note: Do not cache global locator instance. It can be changed anytime.
        try {
          return Microsoft.Practices.ServiceLocation.ServiceLocator.Current;
        }
        catch (NullReferenceException) {
          return null;
        }
      }
    }

    private static IServiceLocator DefaultLocator
    {
      get
      {
        if (defaultLocator!=null)
          return defaultLocator;

        var container = new ServiceContainer();
        container.Configure();
        defaultLocator = new ServiceLocatorAdapter(container);
        return defaultLocator;
      }
    }

    /// <summary>
    /// Get all instances of the given currently registered in the container. 
    /// </summary>
    /// <typeparam name="TService">Type of object requested.</typeparam>
    /// <returns>A sequence of instances of the requested <typeparamref name="TService"/>.</returns>
    /// <exception cref="ActivationException">if there is are errors resolving the service instance.</exception>
    public static IEnumerable<TService> GetAllInstances<TService>()
    {
      return GetAllInstances(typeof(TService)).Cast<TService>();
    }

    /// <summary>
    /// Get all instances of the given currently registered in the container. 
    /// </summary>
    /// <param name="serviceType">Type of object requested.</typeparam>
    /// <returns>A sequence of instances of the requested <paramref name="serviceType"/>.</returns>
    /// <exception cref="ActivationException">if there is are errors resolving the service instance.</exception>
    public static IEnumerable<object> GetAllInstances(Type serviceType)
    {
      if (GlobalLocator == null)
        return DefaultLocator.GetAllInstances(serviceType);

      try {
        return GlobalLocator.GetAllInstances(serviceType);
      }
      catch(ActivationException) {
        return DefaultLocator.GetAllInstances(serviceType);
      }
    }

    /// <summary>
    /// Get an instance of the given <typeparamref name="TService"/>.
    /// </summary>
    /// <typeparam name="TService">Type of object requested.</typeparam>
    /// <returns>The requested service instance.</returns>
    /// <exception cref="ActivationException">if there is are errors resolving the service instance.</exception>
    public static TService GetInstance<TService>()
    {
      return GetInstance<TService>(null);
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
      return (TService) GetInstance(typeof(TService), key);
    }

    /// <summary>
    /// Get an instance of the given <paramref name="serviceType"/>.
    /// </summary>
    /// <param name="serviceType">Type of object requested.</typeparam>
    /// <returns>The requested service instance.</returns>
    /// <exception cref="ActivationException">if there is are errors resolving the service instance.</exception>
    public static object GetInstance(Type serviceType)
    {
      return GetInstance(serviceType, null);
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
      if (GlobalLocator == null)
        return DefaultLocator.GetInstance(serviceType, key);

      try {
        return GlobalLocator.GetInstance(serviceType, key);
      }
      catch(ActivationException) {
        return DefaultLocator.GetInstance(serviceType, key);
      }
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