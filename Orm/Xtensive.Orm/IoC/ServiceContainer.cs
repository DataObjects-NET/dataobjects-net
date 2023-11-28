// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2009.10.12

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.IoC.Configuration;
using AttributeSearchOptions = Xtensive.Reflection.AttributeSearchOptions;
using AppConfiguration = System.Configuration.Configuration;
using ConfigurationSection = Xtensive.IoC.Configuration.ConfigurationSection;
using Key = System.ValueTuple<System.Type, string>;

namespace Xtensive.IoC
{
  /// <summary>
  /// Default IoC (inversion of control) container implementation.
  /// </summary>
  [Serializable]
  public class ServiceContainer : ServiceContainerBase
  {
    private static readonly Type iServiceContainerType = typeof(IServiceContainer);

#if NET8_0_OR_GREATER
    private static readonly Func<ServiceRegistration, Pair<ConstructorInvoker, ParameterInfo[]>> ConstructorFactory = serviceInfo => {
#else
    private static readonly Func<ServiceRegistration, Pair<ConstructorInfo, ParameterInfo[]>> ConstructorFactory = serviceInfo => {
#endif
      var mappedType = serviceInfo.MappedType;
      var ctor = (
        from c in mappedType.GetConstructors()
        where c.GetAttribute<ServiceConstructorAttribute>(AttributeSearchOptions.InheritNone) != null
        select c
        ).SingleOrDefault() ?? mappedType.GetConstructor(Array.Empty<Type>());
      var @params = ctor?.GetParameters();
#if NET8_0_OR_GREATER
      return new(ctor is null ? null : ConstructorInvoker.Create(ctor), @params);
#else
      return new(ctor, @params);
#endif
    };

    private readonly IReadOnlyDictionary<Key, List<ServiceRegistration>> types;

    private readonly ConcurrentDictionary<ServiceRegistration, Lazy<object>> instances =
      new ConcurrentDictionary<ServiceRegistration, Lazy<object>>();

#if NET8_0_OR_GREATER
    private readonly ConcurrentDictionary<ServiceRegistration, Pair<ConstructorInvoker, ParameterInfo[]>> constructorCache = new();
#else
    private readonly ConcurrentDictionary<ServiceRegistration, Pair<ConstructorInfo, ParameterInfo[]>> constructorCache = new();
#endif

    private readonly ConcurrentDictionary<(Type, int), bool> creating = new ConcurrentDictionary<(Type, int), bool>();

    #region Protected virtual methods (to override)

    /// <inheritdoc/>
    /// <exception cref="AmbiguousMatchException">Multiple services match to the specified arguments.</exception>
    protected override object HandleGet(Type serviceType, string name)
    {
      if (!types.TryGetValue(GetKey(serviceType, name), out var list))
        return null;
      return list.Count switch {
        0 => null,
        1 => GetOrCreateInstance(list[0]),
        _ => throw new AmbiguousMatchException(Strings.ExMultipleServicesMatchToTheSpecifiedArguments)
      };
    }

    /// <inheritdoc/>
    protected override IEnumerable<object> HandleGetAll(Type serviceType) =>
      types.TryGetValue(GetKey(serviceType, null), out var list)
        ? list.Select(GetOrCreateInstance)
        : Array.Empty<object>();

    /// <summary>
    /// Creates the service instance for the specified <paramref name="serviceInfo"/>.
    /// </summary>
    /// <param name="serviceInfo">The service info.</param>
    /// <returns>Specified service instance.</returns>
    /// <exception cref="ActivationException">Something went wrong.</exception>
    protected virtual object CreateInstance(ServiceRegistration serviceInfo)
    {
      var cachedInfo = constructorCache.GetOrAdd(serviceInfo, ConstructorFactory);
      var cInfo = cachedInfo.First;
      if (cInfo == null) {
        return null;
      }
      var pInfos = cachedInfo.Second;
      var nArg = pInfos.Length;
      if (nArg == 0) {
        return Activator.CreateInstance(serviceInfo.MappedType);
      }
      var managedThreadId = Environment.CurrentManagedThreadId;
      var key = (serviceInfo.Type, managedThreadId);
      if (!creating.TryAdd(key, true)) {
        throw new ActivationException(Strings.ExRecursiveConstructorParameterDependencyIsDetected);
      }
      var args = new object[nArg];
      try {
        for (var i = 0; i < nArg; i++) {
          var type = pInfos[i].ParameterType;
          if (creating.ContainsKey((type, managedThreadId))) {
            throw new ActivationException(Strings.ExRecursiveConstructorParameterDependencyIsDetected);
          }
          args[i] = Get(type);
        }
      }
      finally {
        _ = creating.TryRemove(key, out _);
      }
#if NET8_0_OR_GREATER
      return cInfo.Invoke(new Span<object>(args));
#else
      return cInfo.Invoke(args);
#endif
    }

#endregion

    #region Private \ internal methods

    private static Key GetKey(Type serviceType, string name) =>
      new Key(serviceType, string.IsNullOrEmpty(name) ? null : name);

    private object InstanceFactory(ServiceRegistration registration) =>
      registration.MappedInstance ?? CreateInstance(registration);

    private Lazy<object> LazyFactory(ServiceRegistration registration) =>
      new Lazy<object>(() => InstanceFactory(registration));

    private object GetOrCreateInstance(ServiceRegistration registration) =>
      registration.Singleton
        ? instances.GetOrAdd(registration, LazyFactory).Value
        : InstanceFactory(registration);

    private static void Register(Dictionary<Key, List<ServiceRegistration>> types, ServiceRegistration serviceRegistration)
    {
      var key = GetKey(serviceRegistration.Type, serviceRegistration.Name);
      if (!types.TryGetValue(key, out var list)) {
        types[key] = list = new List<ServiceRegistration>();
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
      if (!iServiceContainerType.IsAssignableFrom(containerType))
        throw new ArgumentException(string.Format(
          Strings.ExContainerTypeMustImplementX, iServiceContainerType.Name), "containerType");

      Type configurationType = configuration?.GetType(),
        parentType = parent?.GetType();
      return (IServiceContainer) (
#if NET8_0_OR_GREATER
        FindConstructorInvoker(containerType, configurationType, parentType)?.Invoke(configuration, parent)
        ?? FindConstructorInvoker(containerType, configurationType)?.Invoke(configuration)
        ?? FindConstructorInvoker(containerType, parentType)?.Invoke(parent)
#else
        FindConstructor(containerType, configurationType, parentType)?.Invoke(new[] { configuration, parent })
        ?? FindConstructor(containerType, configurationType)?.Invoke(new[] { configuration })
        ?? FindConstructor(containerType, parentType)?.Invoke(new[] { parent })
#endif
        ?? throw new ArgumentException(Strings.ExContainerTypeDoesNotProvideASuitableConstructor, "containerType")
      );
    }

#if NET8_0_OR_GREATER
    private static ConstructorInvoker FindConstructorInvoker(Type containerType, params Type[] argumentTypes) =>
      containerType.GetSingleConstructorInvokerOrDefault(argumentTypes);
#else
    private static ConstructorInfo FindConstructor(Type containerType, params Type[] argumentTypes) =>
      containerType.GetSingleConstructorOrDefault(argumentTypes);
#endif

#endregion

    /// <summary>
    /// Creates <see cref="IServiceContainer"/> by default configuration.
    /// </summary>
    /// <param name="configuration">An <see cref="AppConfiguration"/> instance.</param>
    /// <returns><see cref="IServiceContainer"/> for the default configuration.</returns>
    public static IServiceContainer Create(AppConfiguration configuration)
    {
      return Create(configuration, (string) null);
    }

    /// <summary>
    /// Creates <see cref="IServiceContainer"/> by its configuration.
    /// </summary>
    /// <param name="configuration">An <see cref="AppConfiguration"/> instance.</param>
    /// <param name="name">The name of container configuration to create container for.</param>
    /// <returns><see cref="IServiceContainer"/> for the specified named configuration.</returns>
    public static IServiceContainer Create(AppConfiguration configuration, string name)
    {
      return Create(configuration, name, null);
    }

    /// <summary>
    /// Creates <see cref="IServiceContainer"/> by its configuration.
    /// </summary>
    /// <param name="configuration">An <see cref="AppConfiguration"/> instance.</param>
    /// <param name="name">The name of container configuration to create container for.</param>
    /// <param name="parent">The parent container.</param>
    /// <returns><see cref="IServiceContainer"/> for the specified named configuration.</returns>
    public static IServiceContainer Create(AppConfiguration configuration, string name, IServiceContainer parent)
    {
      var serviceSection = (ConfigurationSection) configuration.GetSection(
        ConfigurationSection.DefaultSectionName);
      return Create(serviceSection, name, parent);
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

      ContainerElement configuration = section?.Containers[name]
        ?? new ContainerElement();

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
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="parent">The parent container.</param>
    public ServiceContainer(IEnumerable<ServiceRegistration> configuration = null, IServiceContainer parent = null)
      : base(parent)
    {
      var typesDictionary = new Dictionary<Key, List<ServiceRegistration>>();
      if (configuration != null) {
        foreach (var serviceRegistration in configuration) {
          Register(typesDictionary, serviceRegistration);
        }
      }
      types = typesDictionary;
    }

    // Dispose implementation

    public override void Dispose()
    {
      using (var toDispose = new DisposableSet()) {
        foreach (var lazy in instances.Values) {
          if (lazy.IsValueCreated && lazy.Value is IDisposable disposable) {
            toDispose.Add(disposable);
          }
        }
      }
    }
  }
}
