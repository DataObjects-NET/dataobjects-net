// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.01

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Practices.ServiceLocation;
using ConfigurationSection=Xtensive.Core.IoC.Configuration.ConfigurationSection;
using CommonServiceLocator=Microsoft.Practices.ServiceLocation.ServiceLocator;

namespace Xtensive.Core.IoC
{
  /// <summary>
  /// <see cref="CommonServiceLocator"/> wrapper.
  /// </summary>
  [Serializable]
  public static class ServiceLocator
  {
    private static IServiceLocator defaultLocator;
    private static Func<ServiceLocatorProvider> currentProviderFieldGetter;

    private static IServiceLocator GlobalLocator {
      get {
        if (currentProviderFieldGetter==null) {
          // Caching fast CommonServiceLocator.currentProvider field getter
          var currentProviderField = typeof (CommonServiceLocator)
            .GetField("currentProvider", BindingFlags.NonPublic | BindingFlags.Static);
          currentProviderFieldGetter = (Func<ServiceLocatorProvider>) 
            Expression.Lambda(typeof(Func<ServiceLocatorProvider>),
              Expression.Field(
                Expression.Constant(null),
                currentProviderField))
              .Compile();
        }
        // We must not cache global locator instance. It can be changed anytime.
        var currentProviderValue = currentProviderFieldGetter.Invoke();
        if (currentProviderValue != null)
          return CommonServiceLocator.Current;
        return null;
      }
    }

    private static IServiceLocator DefaultLocator {
      get {
        if (defaultLocator!=null)
          return defaultLocator;

        // Looking for default IoC configuration section
        var configuration = (ConfigurationSection) ConfigurationManager.GetSection(
          ConfigurationSection.DefaultSectionName);
        var containerConfig = configuration!=null ? configuration.Containers.Default : null;
        var container = new ServiceContainer();
        container.Configure(containerConfig);
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
      if (GlobalLocator != null)
        return GlobalLocator.GetAllInstances(serviceType);

      return DefaultLocator.GetAllInstances(serviceType);
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
      if (GlobalLocator != null)
        return GlobalLocator.GetInstance(serviceType, key);

      return DefaultLocator.GetInstance(serviceType, key);
    }

    /// <summary>
    /// Set the delegate that is used to retrieve the current container.
    /// </summary>
    /// <param name="newProvider">Delegate that, when called, will return
    /// the current ambient container.</param>
    public static void SetLocatorProvider(ServiceLocatorProvider newProvider)
    {
      CommonServiceLocator.SetLocatorProvider(newProvider);
    }
  }
}