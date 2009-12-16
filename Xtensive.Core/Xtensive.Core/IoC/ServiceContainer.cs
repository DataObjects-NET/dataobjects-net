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
using Xtensive.Core.IoC.Configuration;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.IoC
{
  /// <summary>
  /// Simple inversion-of-control container implementation.
  /// </summary>
  [Serializable]
  public class ServiceContainer
  {
    private readonly Dictionary<Type, List<ServiceInfo>> types = new Dictionary<Type, List<ServiceInfo>>();
    private readonly Dictionary<ServiceInfo, object> instances = new Dictionary<ServiceInfo, object>();

    /// <summary>
    /// Get an instance of the given <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type of object requested.</typeparam>
    /// <param name="key">Name the object was registered with.</param>
    /// <returns>The requested service instance.</returns>
    /// <exception cref="ActivationException">if there is are errors resolving the service instance.</exception>
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

    /// <summary>
    /// Get all instances of the given currently registered in the container. 
    /// </summary>
    /// <param name="type">Type of object requested.</typeparam>
    /// <returns>A sequence of instances of the requested <paramref name="type"/>.</returns>
    /// <exception cref="ActivationException">if there is are errors resolving the service instance.</exception>
    public IEnumerable<object> GetAllInstances(Type type)
    {
      List<ServiceInfo> list;
      if (!types.TryGetValue(type, out list))
        throw new ActivationException();

      return list.Select(item => GetOrCreateInstance(item));
    }

    /// <summary>
    /// RegisterType a type mapping with the container.
    /// </summary>
    /// <param name="from"><see cref="Type"/> that will be requested.</param>
    /// <param name="to"><see cref="Type"/> that will actually be returned.</param>
    /// <param name="name">Name to use for registration, null if a default registration.</param>
    /// <param name="singleton">Indicates whether an instance of specified type must be singleton or not.</param>
    public void RegisterType(Type from, Type to, string name, bool singleton)
    {
      List<ServiceInfo> list;
      if (!types.TryGetValue(from, out list)) {
        list = new List<ServiceInfo>();
        types[from] = list;
      }
      list.Add(new ServiceInfo(from, to, name, singleton));
    }

    /// <summary>
    /// Configures this instance with the specified configuration.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    public void Configure(ContainerElement configuration)
    {
      if (configuration!=null && configuration.Types!=null)
        Apply(configuration);
      ApplyDefaultConfiguration();
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

    private void Apply(ContainerElement configuration)
    {
      foreach (var element in configuration.Types) {
        var type = Type.GetType(element.Type);
        var mapTo = Type.GetType(element.MapTo);
        var name = string.IsNullOrEmpty(element.Name) ? null : element.Name;
        RegisterType(type, mapTo, name, element.Singleton);
      }
    }

    private void ApplyDefaultConfiguration()
    {
      // Default logging framework
      if (!types.ContainsKey(typeof(ILogProvider)))
        RegisterType(typeof(ILogProvider), typeof(LogProviderImplementation), null, true);
    }
  }
}