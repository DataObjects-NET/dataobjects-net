// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.01.30

using System;
using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.IoC
{
  /// <summary>
  /// Inversion of control container contract.
  /// </summary>
  public interface IServiceContainer : IServiceProvider, 
    IHasServices,
    IDisposable
  {
    /// <summary>
    /// Gets the parent service container.
    /// Parent service container usually resolves services that 
    /// can't be resolved by the current container.
    /// </summary>
    IServiceContainer Parent { get; }

    /// <summary>
    /// Gets all the instances of type <typeparamref name="TService"/>
    /// from the container.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <returns>
    /// A sequence of all the requested instances.
    /// </returns>
    IEnumerable<TService> GetAll<TService>();

    /// <summary>
    /// Gets all the instances of type <paramref name="serviceType"/>
    /// from the container.
    /// </summary>
    /// <param name="serviceType">Type of the service.</param>
    /// <returns>
    /// A sequence of all the requested instances.
    /// </returns>
    IEnumerable<object> GetAll(Type serviceType);

    /// <summary>
    /// Gets the instance of <typeparamref name="TService"/> type
    /// from the container.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <returns>Requested instance.</returns>
    TService Get<TService>();

    /// <summary>
    /// Gets the instance of <typeparamref name="TService"/> type
    /// identified by the specified <paramref name="name"/>
    /// from the container.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <param name="name">The identifier of the service to get.</param>
    /// <returns>Requested instance.</returns>
    TService Get<TService>(string name);

    /// <summary>
    /// Gets the instance of <paramref name="serviceType"/>
    /// from the container.
    /// </summary>
    /// <param name="serviceType">Type of the service.</param>
    /// <returns>Requested instance.</returns>
    object Get(Type serviceType);

    /// <summary>
    /// Gets the instance of <paramref name="serviceType"/>
    /// identified by the specified <paramref name="name"/>
    /// from the container.
    /// </summary>
    /// <param name="serviceType">Type of the service.</param>
    /// <param name="name">The identifier of the service to get.</param>
    /// <returns>Requested instance.</returns>
    object Get(Type serviceType, string name);
  }
}