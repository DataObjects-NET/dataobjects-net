// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.15

using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;
using Xtensive.Core.Aspects;
using ServiceLocator=Xtensive.Core.IoC.ServiceLocator;

namespace Xtensive.Storage
{
  /// <summary>
  /// Provides access to services.
  /// </summary>
  public sealed class SessionServiceLocator : SessionBound,
    IServiceLocator
  {
    private IServiceLocator localProvider;

    /// <summary>
    /// Set the delegate that is used to retrieve the current container.
    /// </summary>
    /// <param name="newProvider">Delegate that, when called, will return
    /// the current session container.</param>
    [Infrastructure]
    public void SetLocatorProvider(ServiceLocatorProvider newProvider)
    {
      localProvider = newProvider();
    }

    /// <inheritdoc/>
    [Infrastructure]
    public TService GetInstance<TService>()
    {
      using (Session.Activate()) {
        if (localProvider!=null)
          return localProvider.GetInstance<TService>();
        else
          return ServiceLocator.GetInstance<TService>();
      }
    }

    /// <inheritdoc/>
    [Infrastructure]
    public TService GetInstance<TService>(string name)
    {
      using (Session.Activate()) {
        if (localProvider!=null)
          return localProvider.GetInstance<TService>(name);
        else
          return ServiceLocator.GetInstance<TService>(name);
      }
    }

    /// <inheritdoc/>
    [Infrastructure]
    public object GetInstance(Type type)
    {
      using (Session.Activate()) {
        if (localProvider!=null)
          return localProvider.GetInstance(type);
        else
          return ServiceLocator.GetInstance(type);
      }
    }

    /// <inheritdoc/>
    [Infrastructure]
    public object GetInstance(Type type, string name)
    {
      using (Session.Activate()) {
        if (localProvider!=null)
          return localProvider.GetInstance(type, name);
        else
          return ServiceLocator.GetInstance(type, name);
      }
    }

    /// <inheritdoc/>
    [Infrastructure]
    public IEnumerable<TService> GetAllInstances<TService>()
    {
        if (localProvider!=null)
          return localProvider.GetAllInstances<TService>();
        else
          return ServiceLocator.GetAllInstances<TService>();
    }

    /// <inheritdoc/>
    [Infrastructure]
    public IEnumerable<object> GetAllInstances(Type serviceType)
    {
        if (localProvider!=null)
          return localProvider.GetAllInstances(serviceType);
        else
          return ServiceLocator.GetAllInstances(serviceType);
    }

    /// <inheritdoc/>
    [Infrastructure]
    public object GetService(Type serviceType)
    {
      return ServiceLocator.GetInstance(serviceType, null);
    }


    // Constructors

    internal SessionServiceLocator(Session session)
      : base(session)
    {
    }
  }
}