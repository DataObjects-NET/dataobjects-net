// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.15

using System;
using Microsoft.Practices.Unity;

namespace Xtensive.Storage
{
  /// <summary>
  /// Provides access to services.
  /// </summary>
  public sealed class ServiceProvider : SessionBound
  {
    private readonly IUnityContainer servicesContainer;

    /// <summary>
    /// Gets the service of specified type.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <returns>Resolved service.</returns>
    public TService Get<TService>()
     where TService : class
    {
      return servicesContainer.Resolve<TService>();
    }

    /// <summary>
    /// Gets the service with the specified type and name.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <param name="name">The name of the service.</param>
    /// <returns>Resolved service.</returns>
    public TService Get<TService>(string name)
      where TService : class
    {
      return servicesContainer.Resolve<TService>(name);
    }

    /// <summary>
    /// Gets the service of specified type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>Resolved service.</returns>
    public object Get(Type type)
    {
      return servicesContainer.Resolve(type);
    }

    /// <summary>
    /// Gets the service with the specified type and name.
    /// </summary>
    /// <param name="type">The service type type.</param>
    /// <param name="name">The service name.</param>
    /// <returns>Resolved service.</returns>
    public object Get(Type type, string name)
    {
      return servicesContainer.Resolve(type, name);
    }


    // Constructor
    
    internal ServiceProvider(Session session)
      : base(session)
    {
      servicesContainer = session.Domain.ServiceContainer.CreateChildContainer();
    }
  }
}