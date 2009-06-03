// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.15

using System;
using Microsoft.Practices.Unity;
using Xtensive.Core.Aspects;

namespace Xtensive.Storage
{
  /// <summary>
  /// Provides access to services.
  /// </summary>
  public sealed class ServiceProvider : SessionBound
  {
    private readonly IUnityContainer serviceContainer;

    /// <summary>
    /// Gets the service of specified type.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <returns>Resolved service.</returns>
    [Infrastructure]
    public TService Get<TService>()
     where TService : class
    {
      using (Session.Activate()) {
        return serviceContainer.Resolve<TService>();
      }
    }

    /// <summary>
    /// Gets the service with the specified type and name.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <param name="name">The name of the service.</param>
    /// <returns>Resolved service.</returns>
    [Infrastructure]
    public TService Get<TService>(string name)
      where TService : class
    {
      using (Session.Activate()) {
        return serviceContainer.Resolve<TService>(name);
      }
    }

    /// <summary>
    /// Gets the service of specified type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>Resolved service.</returns>
    [Infrastructure]
    public object Get(Type type)
    {
      using (Session.Activate()) {
        return serviceContainer.Resolve(type);
      }
    }

    /// <summary>
    /// Gets the service with the specified type and name.
    /// </summary>
    /// <param name="type">The service type type.</param>
    /// <param name="name">The service name.</param>
    /// <returns>Resolved service.</returns>
    [Infrastructure]
    public object Get(Type type, string name)
    {
      using (Session.Activate()) {
        return serviceContainer.Resolve(type, name);
      }
    }


    // Constructor
    
    internal ServiceProvider(Session session)
      : base(session)
    {
      serviceContainer = session.Domain.ServiceContainer.CreateChildContainer();
    }
  }
}