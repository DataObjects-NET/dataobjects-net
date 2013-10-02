// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.01.30

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Xtensive.Collections;
using Xtensive.Core;


namespace Xtensive.IoC
{
  internal sealed class DefaultServiceContainer : ServiceContainerBase
  {
    ThreadSafeDictionary<Assembly, IServiceContainer> containers = 
      ThreadSafeDictionary<Assembly, IServiceContainer>.Create(new object());

    protected override IEnumerable<object> HandleGetAll(Type serviceType)
    {
      var container = GetContainer(serviceType);
      return container.GetAll(serviceType);
    }

    protected override object HandleGet(Type serviceType, string name)
    {
      var container = GetContainer(serviceType);
      return container.Get(serviceType, name);
    }

    private IServiceContainer GetContainer(Type serviceType)
    {
      var assembly = serviceType.Assembly;
      return containers.GetValue(assembly, _assembly => {
        var typeRegistry = new TypeRegistry(new ServiceTypeRegistrationProcessor());
        typeRegistry.Register(_assembly);
        return new ServiceContainer(
          from type in typeRegistry
          from serviceRegistration in ServiceRegistration.CreateAll(type, true)
          select serviceRegistration);
      });
    }
  }
}