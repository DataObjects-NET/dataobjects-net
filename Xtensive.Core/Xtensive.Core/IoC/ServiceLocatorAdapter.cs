// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.12

using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.IoC
{
  /// <summary>
  /// Adapter for default IoC container implementation.
  /// </summary>
  [Serializable]
  public sealed class ServiceLocatorAdapter : ServiceLocatorImplBase
  {
    private readonly ServiceContainer container;

    /// <inheritdoc/>
    protected override object DoGetInstance(Type serviceType, string key)
    {
      return container.GetInstance(serviceType, key);
    }

    /// <inheritdoc/>
    protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
    {
      return container.GetAllInstances(serviceType);
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="container">The container.</param>
    public ServiceLocatorAdapter(ServiceContainer container)
    {
      this.container = container;
    }
  }
}