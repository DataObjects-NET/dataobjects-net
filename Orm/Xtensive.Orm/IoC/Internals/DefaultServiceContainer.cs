// Copyright (C) 2010-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2010.01.30

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Xtensive.Collections;


namespace Xtensive.IoC
{
  internal sealed class DefaultServiceContainer : ServiceContainerBase
  {
    private ConcurrentDictionary<Assembly, Lazy<IServiceContainer>> containers =
      new ConcurrentDictionary<Assembly, Lazy<IServiceContainer>>();

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
      static Lazy<IServiceContainer> ServiceContainerFactory(Assembly assembly)
      {
        return new Lazy<IServiceContainer>(() => {
          var typeRegistry = new TypeRegistry(new ServiceTypeRegistrationProcessor());
          typeRegistry.Register(assembly);
          return new ServiceContainer(typeRegistry.SelectMany(type => ServiceRegistration.CreateAll(type, true)));
        });
      }

      return containers.GetOrAdd(serviceType.Assembly, ServiceContainerFactory).Value;
    }
  }
}