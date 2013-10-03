// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.12

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Diagnostics;
using Xtensive.Reflection;
using Xtensive.Internals.DocTemplates;
using Xtensive.IoC.Configuration;

using AttributeSearchOptions = Xtensive.Reflection.AttributeSearchOptions;
using ConfigurationSection=Xtensive.IoC.Configuration.ConfigurationSection;
using DelegateHelper = Xtensive.Reflection.DelegateHelper;

namespace Xtensive.IoC
{
  /// <summary>
  /// Default IoC (inversion of control) container implementation.
  /// </summary>
  [Serializable]
  public class ServiceContainer : ServiceContainerBase
  {
    private readonly Dictionary<object, List<ServiceRegistration>> types = 
      new Dictionary<object, List<ServiceRegistration>>();
    private readonly Dictionary<ServiceRegistration, object> instances = 
      new Dictionary<ServiceRegistration, object>();
    private readonly Dictionary<ServiceRegistration, Pair<ConstructorInfo,ParameterInfo[]>> constructorCache = 
      new Dictionary<ServiceRegistration, Pair<ConstructorInfo, ParameterInfo[]>>();
    private readonly HashSet<Type> creating = new HashSet<Type>();
    private static readonly object _lock = new object();
    private static IServiceContainer @default;

    public static IServiceContainer Default {
      get {
        if (@default==null) lock (_lock) if (@default==null) {
          @default = Create();
        }
        return @default;
      }
    }

    #region Protected virtual methods (to override)

    /// <inheritdoc/>
    /// <exception cref="AmbiguousMatchException">Multiple services match to the specified arguments.</exception>
    protected override object HandleGet(Type serviceType, string name)
    {
      // Not very optimal, but...
      lock (_lock) {
        List<ServiceRegistration> list;
        if (!types.TryGetValue(GetKey(serviceType, name), out list))
          return null;
        if (list.Count==0)
          return null;
        if (list.Count > 1)
          throw new AmbiguousMatchException(Strings.ExMultipleServicesMatchToTheSpecifiedArguments);
        return GetOrCreateInstances(list).Single();
      }
    }

    /// <inheritdoc/>
    protected override IEnumerable<object> HandleGetAll(Type serviceType)
    {
      // Not very optimal, but...
      lock (_lock) {
        List<ServiceRegistration> list;
        if (!types.TryGetValue(GetKey(serviceType, null), out list))
          return EnumerableUtils<object>.Empty;
        return GetOrCreateInstances(list);
      }
    }

    /// <summary>
    /// Creates the service instance for the specified <paramref name="serviceInfo"/>.
    /// </summary>
    /// <param name="serviceInfo">The service info.</param>
    /// <returns>Specified service instance.</returns>
    /// <exception cref="ActivationException">Something went wrong.</exception>
    protected virtual object CreateInstance(ServiceRegistration serviceInfo)
    {
      if (creating.Contains(serviceInfo.Type))
        throw new ActivationException(Strings.ExRecursiveConstructorParemeterDependencyIsDetected);
      Pair<ConstructorInfo,ParameterInfo[]> cachedInfo;
      var mappedType = serviceInfo.MappedType;
      if (!constructorCache.TryGetValue(serviceInfo, out cachedInfo)) {
        var @ctor = (
          from c in mappedType.GetConstructors()
          where c.GetAttribute<ServiceConstructorAttribute>(AttributeSearchOptions.InheritNone)!=null
          select c
          ).SingleOrDefault();
        if (@ctor==null)
          @ctor = mappedType.GetConstructor(ArrayUtils<Type>.EmptyArray);
        var @params = @ctor==null ? null : @ctor.GetParameters();
        cachedInfo = new Pair<ConstructorInfo, ParameterInfo[]>(@ctor, @params);
        constructorCache[serviceInfo] = cachedInfo;
      }
      var cInfo = cachedInfo.First;
      if (cInfo==null)
        return null;
      var pInfos = cachedInfo.Second;
      if (pInfos.Length==0) {
        var activator = DelegateHelper.CreateConstructorDelegate<Func<object>>(mappedType);
        return activator.Invoke();
      }
      var args = new object[pInfos.Length];
      creating.Add(serviceInfo.Type);
      try {
        for (int i = 0; i < pInfos.Length; i++)
          args[i] = Get(pInfos[i].ParameterType);
      }
      finally {
        creating.Remove(serviceInfo.Type);
      }
      return cInfo.Invoke(args);
    }

    #endregion

    #region Private \ internal methods

    private static object GetKey(Type serviceType, string name)
    {
      if (name.IsNullOrEmpty())
        return serviceType;
      else
        return new ObjectPair(serviceType, name);
    }

    private IEnumerable<object> GetOrCreateInstances(IEnumerable<ServiceRegistration> services)
    {
      foreach (var registration in services) {
        object result;
        if (registration.Singleton && instances.TryGetValue(registration, out result)) {
          yield return result;
          continue;
        }

        if (registration.MappedInstance!=null)
          result = registration.MappedInstance;
        else
          result = CreateInstance(registration);

        if (registration.Singleton)
          instances[registration] = result;
        yield return result;
      }
    }

    private void Register(ServiceRegistration serviceRegistration)
    {
      List<ServiceRegistration> list;
      var key = GetKey(serviceRegistration.Type, serviceRegistration.Name);
      if (!types.TryGetValue(key, out list)) {
        list = new List<ServiceRegistration>();
        types[key] = list;
      }
      list.Add(serviceRegistration);
    }

    #endregion

    #region Create(Type containerType, ...) methods

    /// <summary>
    /// Creates <see cref="IServiceContainer"/> of the specified type.
    /// </summary>
    /// <param name="containerType">Type of the container to create.</param>
    /// <returns>Created service container.</returns>
    public static IServiceContainer Create(Type containerType)
    {
      return Create(containerType, null, null);
    }

    /// <summary>
    /// Creates <see cref="IServiceContainer"/> of the specified type
    /// and with the specified <see cref="IServiceContainer.Parent"/>.
    /// </summary>
    /// <param name="containerType">Type of the container to create.</param>
    /// <param name="parent">The parent container.</param>
    /// <returns>Created service container.</returns>
    public static IServiceContainer Create(Type containerType, IServiceContainer parent)
    {
      return Create(containerType, null, parent);
    }

    /// <summary>
    /// Creates <see cref="IServiceContainer"/> of the specified type
    /// with specified <paramref name="configuration"/>.
    /// </summary>
    /// <param name="containerType">Type of the container to create.</param>
    /// <param name="configuration">The container's configuration.</param>
    /// <returns>Created service container.</returns>
    public static IServiceContainer Create(Type containerType, object configuration)
    {
      return Create(containerType, configuration, null);
    }

    /// <summary>
    /// Creates <see cref="IServiceContainer"/> of the specified type
    /// with the specified <see cref="IServiceContainer.Parent"/>
    /// and <paramref name="configuration"/>.
    /// </summary>
    /// <param name="containerType">Type of the container to create.</param>
    /// <param name="configuration">The container's configuration.</param>
    /// <param name="parent">The parent container.</param>
    /// <returns>Created service container.</returns>
    /// <exception cref="ArgumentException">Wrong container type.</exception>
    public static IServiceContainer Create(Type containerType, object configuration, IServiceContainer parent)
    {
      ArgumentValidator.EnsureArgumentNotNull(containerType, "containerType");
      if (!typeof(IServiceContainer).IsAssignableFrom(containerType))
        throw new ArgumentException(string.Format(
          Strings.ExContainerTypeMustImplementX, typeof(IServiceContainer).GetShortName()), "containerType");

      var possibleArgs =
        Enumerable.Empty<object[]>()
          .AddOne(new[] {configuration, parent})
          .AddOne(new[] {configuration})
          .AddOne(new[] {parent});
      foreach (var args in possibleArgs) {
        var ctor = containerType.GetConstructor(args);
        if (ctor!=null)
          return (IServiceContainer) ctor.Invoke(args);
      }

      throw new ArgumentException(
        Strings.ExContainerTypeDoesNotProvideASuitableConstructor, "containerType");
    }

    #endregion

    /// <summary>
    /// Creates <see cref="IServiceContainer"/> by default configuration.
    /// </summary>
    /// <returns><see cref="IServiceContainer"/> for the default configuration.</returns>
    public static IServiceContainer Create()
    {
      return Create((string) null);
    }

    /// <summary>
    /// Creates <see cref="IServiceContainer"/> by its configuration.
    /// </summary>
    /// <param name="name">The name of container configuration to create container for.</param>
    /// <returns><see cref="IServiceContainer"/> for the specified named configuration.</returns>
    public static IServiceContainer Create(string name)
    {
      return Create(name, null);
    }

    /// <summary>
    /// Creates <see cref="IServiceContainer"/> by its configuration.
    /// </summary>
    /// <param name="name">The name of container configuration to create container for.</param>
    /// <param name="parent">The parent container.</param>
    /// <returns><see cref="IServiceContainer"/> for the specified named configuration.</returns>
    public static IServiceContainer Create(string name, IServiceContainer parent)
    {
      var configuration = (ConfigurationSection) ConfigurationManager.GetSection(
        ConfigurationSection.DefaultSectionName);
      return Create(configuration, name, parent);
    }

    /// <summary>
    /// Creates <see cref="IServiceContainer"/> by its configuration.
    /// </summary>
    /// <param name="section">IoC configuration section.</param>
    /// <param name="name">The name of container configuration to create container for.</param>
    /// <returns>
    /// <see cref="IServiceContainer"/> for the specified named configuration.
    /// </returns>
    public static IServiceContainer Create(ConfigurationSection section, string name)
    {
      return Create(section, name, null);
    }

      /// <summary>
    /// Creates <see cref="IServiceContainer"/> by its configuration.
    /// </summary>
    /// <param name="section">IoC configuration section.</param>
    /// <param name="name">The name of container configuration to create container for.</param>
    /// <param name="parent">The parent container.</param>
    /// <returns>
    /// <see cref="IServiceContainer"/> for the specified named configuration.
    /// </returns>
    public static IServiceContainer Create(ConfigurationSection section, string name, IServiceContainer parent)
    {
      if (name.IsNullOrEmpty())
        name = string.Empty;

      ContainerElement configuration = section==null ? null : section.Containers[name];
      if (configuration==null)
        configuration = new ContainerElement();

      var registrations = new List<ServiceRegistration>();
      var typeRegistry = new TypeRegistry(new ServiceTypeRegistrationProcessor());

      foreach (var typeRegistrationElement in configuration.Auto)
        typeRegistry.Register(typeRegistrationElement.ToNative());
      foreach (var type in typeRegistry)
        registrations.AddRange(ServiceRegistration.CreateAll(type));
      foreach (var serviceRegistrationElement in configuration.Explicit)
        registrations.Add(serviceRegistrationElement.ToNative());

      var currentParent = configuration.Parent.IsNullOrEmpty() 
        ? parent 
        : Create(section, configuration.Parent, parent);
      
      var containerType = configuration.Type.IsNullOrEmpty() ? 
        typeof(ServiceContainer) : 
        Type.GetType(configuration.Type);
      return Create(containerType, registrations, currentParent);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ServiceContainer()
      : this(null, null)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    public ServiceContainer(IEnumerable<ServiceRegistration> configuration)
      : this(configuration, null)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="parent">The parent container.</param>
    public ServiceContainer(IEnumerable<ServiceRegistration> configuration, IServiceContainer parent)
      : base(parent)
    {
      if (configuration==null)
        return;
      foreach (var serviceRegistration in configuration)
        Register(serviceRegistration);
    }

    // Dispose implementation

    public override void Dispose()
    {
      using (var toDispose = new DisposableSet()) {
        foreach (var pair in instances) {
          var service = pair.Value;
          var disposable = service as IDisposable;
          if (disposable!=null)
            toDispose.Add(disposable);
        }
      }
    }
  }
}