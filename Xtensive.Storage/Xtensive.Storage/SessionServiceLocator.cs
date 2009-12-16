// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.15

using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;
using Xtensive.Storage.Aspects;

namespace Xtensive.Storage
{
  /// <summary>
  /// Provides access to session-level services.
  /// </summary>
  public sealed class SessionServiceLocator : SessionBound,
    IServiceLocator
  {
    private IServiceLocator locator;

    /// <summary>
    /// Set the delegate that is used to retrieve the current container.
    /// </summary>
    /// <param name="newProvider">Delegate that, when called, will return
    /// the current session container.</param>
    [Transactional(false)]
    public void SetLocatorProvider(ServiceLocatorProvider newProvider)
    {
      locator = newProvider();
    }

    /// <inheritdoc/>
    [Transactional(false)]
    public TService GetInstance<TService>()
    {
      return GetLocator().GetInstance<TService>();
    }

    /// <inheritdoc/>
    [Transactional(false)]
    public TService GetInstance<TService>(string name)
    {
      return GetLocator().GetInstance<TService>(name);
    }

    /// <inheritdoc/>
    [Transactional(false)]
    public object GetInstance(Type type)
    {
      return GetLocator().GetInstance(type);
    }

    /// <inheritdoc/>
    [Transactional(false)]
    public object GetInstance(Type type, string name)
    {
      return GetLocator().GetInstance(type, name);
    }

    /// <inheritdoc/>
    [Transactional(false)]
    public IEnumerable<TService> GetAllInstances<TService>()
    {
      return GetLocator().GetAllInstances<TService>();
    }

    /// <inheritdoc/>
    [Transactional(false)]
    public IEnumerable<object> GetAllInstances(Type serviceType)
    {
      return GetLocator().GetAllInstances(serviceType);
    }

    /// <inheritdoc/>
    [Transactional(false)]
    public object GetService(Type serviceType)
    {
      return GetInstance(serviceType, null);
    }

    private IServiceLocator GetLocator()
    {
      return locator ?? Session.Domain.Services;
    }


    // Constructors

    internal SessionServiceLocator(Session session)
      : base(session)
    {
    }
  }
}