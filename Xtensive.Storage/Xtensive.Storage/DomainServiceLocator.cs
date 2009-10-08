// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.08

using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;

namespace Xtensive.Storage
{
  /// <summary>
  /// Provides access to domain-level services.
  /// </summary>
  public sealed class DomainServiceLocator : IServiceLocator
  {
    private IServiceLocator locator;

    /// <summary>
    /// Set the delegate that is used to retrieve the current container.
    /// </summary>
    /// <param name="newProvider">Delegate that, when called, will return
    /// the current domain container.</param>
    public void SetLocatorProvider(ServiceLocatorProvider newProvider)
    {
      locator = newProvider();
    }

    /// <inheritdoc/>
    public TService GetInstance<TService>()
    {
      return locator.GetInstance<TService>();
    }

    /// <inheritdoc/>
    public TService GetInstance<TService>(string name)
    {
      return locator.GetInstance<TService>(name);
    }

    /// <inheritdoc/>
    public object GetInstance(Type type)
    {
      return locator.GetInstance(type);
    }

    /// <inheritdoc/>
    public object GetInstance(Type type, string name)
    {
      return locator.GetInstance(type, name);
    }

    /// <inheritdoc/>
    public IEnumerable<TService> GetAllInstances<TService>()
    {
      return locator.GetAllInstances<TService>();
    }

    /// <inheritdoc/>
    public IEnumerable<object> GetAllInstances(Type serviceType)
    {
      return locator.GetAllInstances(serviceType);
    }

    /// <inheritdoc/>
    public object GetService(Type serviceType)
    {
      return GetInstance(serviceType, null);
    }


    // Constructors

    internal DomainServiceLocator()
    {
    }
  }
}