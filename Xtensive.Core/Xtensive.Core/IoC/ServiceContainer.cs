// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.12

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.IoC
{
  [Serializable]
  internal class ServiceContainer
  {
    private Dictionary<Type, List<ServiceInfo>> types = new Dictionary<Type, List<ServiceInfo>>();
    private Dictionary<ServiceInfo, object> instances = new Dictionary<ServiceInfo, object>();

    public object GetInstance(Type type, string key)
    {
      List<ServiceInfo> list;
      if (!types.TryGetValue(type, out list))
        throw new ActivationException();

      var serviceInfo = list.Where(item => item.Name==key).FirstOrDefault();
      if (serviceInfo == null)
        throw new ActivationException();

      return GetOrCreateInstance(serviceInfo);
    }

    public IEnumerable<object> GetAllInstances(Type type)
    {
      List<ServiceInfo> list;
      if (!types.TryGetValue(type, out list))
        throw new ActivationException();

      return list.Select(item => GetOrCreateInstance(item));
    }

    private object GetOrCreateInstance(ServiceInfo serviceInfo)
    {
      object result;
      if (serviceInfo.IsSingleton && instances.TryGetValue(serviceInfo, out result))
        return result;

      var activator = DelegateHelper.CreateConstructorDelegate<Func<object>>(serviceInfo.MapTo);
      result = activator();

      if (serviceInfo.IsSingleton)
        instances[serviceInfo] = result;

      return result;
    }

    public void Configure()
    {
      // TODO: Implement configuration from app.config

      // If app.config doesn't contain required configuration then apply the default one.
      ApplyDefaultConfiguration();
    }

    private void ApplyDefaultConfiguration()
    {
      RegisterType(typeof(ILogProvider), typeof(LogProviderImplementation), null, true);
    }

    internal void RegisterType(Type type, Type mapTo, string name, bool isSingleton)
    {
      List<ServiceInfo> list;
      if (!types.TryGetValue(type, out list)) {
        list = new List<ServiceInfo>();
        types[type] = list;
      }
      list.Add(new ServiceInfo(type, mapTo, name, isSingleton));
    }
  }
}