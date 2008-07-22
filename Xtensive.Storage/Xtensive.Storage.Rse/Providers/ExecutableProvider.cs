// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.07

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse.Resources;
using Xtensive.Core.Helpers;

namespace Xtensive.Storage.Rse.Providers
{
  /// <summary>
  /// Abstract base class for any <see cref="RecordSet"/> <see cref="RecordSet.Provider"/>,
  /// that does not need to be compiled.
  /// </summary>
  [Serializable]
  public abstract class ExecutableProvider : Provider,
    ICachingProvider
  {
    private const string CachedResultKey = "Results";
    private readonly HashSet<Type> supportedServices = new HashSet<Type>();

    /// <summary>
    /// Gets the provider this provider is compiled from.
    /// </summary>
    public Provider Origin { get; private set; }

    /// <exception cref="InvalidOperationException">Thrown if <see cref="Origin"/> is <see langword="null" />.</exception>
    protected override RecordHeader BuildHeader()
    {
      if (Origin!=null)
        return Origin.Header;
      throw new InvalidOperationException(Strings.ExHeaderIsNotAvailableSinceOriginIsNotProvided);
    }

    /// <summary>
    /// Gets the sequence this provider provides in the specified <see cref="EnumerationContext"/>.
    /// Returns either cached result (if available), or a result of <see cref="OnEnumerate"/>.
    /// </summary>
    /// <param name="context">The enumeration context.</param>
    public IEnumerable<Tuple> Enumerate(EnumerationContext context)
    {
      var cp = GetService<ICachingProvider>();
      if (cp!=null) {
        cp.EnsureResultIsCached(context);
        return GetCachedResult(context);
      }
      return OnEnumerate(context);
    }

    #region OnXxxEnumerate methods (to override)

    /// <summary>
    /// Called when enumerator is created on this provider.
    /// </summary>
    /// <param name="context">The enumeration context.</param>
    protected internal virtual void OnBeforeEnumerate(EnumerationContext context)
    {
      foreach (Provider source in Sources) {
        var ep = source as ExecutableProvider;
        if (ep!=null)
          ep.OnBeforeEnumerate(context);
      }
    }

    /// <summary>
    /// Called when enumeration is finished.
    /// </summary>
    /// <param name="context">The enumeration context.</param>
    protected internal virtual void OnAfterEnumerate(EnumerationContext context)
    {
      foreach (Provider source in Sources) {
        var ep = source as ExecutableProvider;
        if (ep != null)
          ep.OnBeforeEnumerate(context);
      }
    }

    /// <summary>
    /// Gets the sequence this provider provides in the specified <see cref="EnumerationContext"/>.
    /// Invoked by <see cref="Enumerate"/> method in case there is no cached result for the specified context.
    /// </summary>
    /// <param name="context">The enumeration context.</param>
    protected abstract IEnumerable<Tuple> OnEnumerate(EnumerationContext context);

    #endregion

    #region IHasServices methods

    /// <inheritdoc/>
    /// <remarks>
    /// The implementation of this method checks if specified service <typeparamref name="T"/>
    /// was registered by <see cref="AddService{T}"/>, and returns <c>this as T</c>, if this is <see langword="true" />;
    /// otherwise, <see langword="null" />.
    /// </remarks>
    public override T GetService<T>()
    {
      if (!supportedServices.Contains(typeof(T)))
        return null;
      return (this as T);
    }

    /// <summary>
    /// Registers the service as "supported".
    /// This method should be called only from thread-safe methods,
    /// such as <see cref="BuildHeader"/>.
    /// </summary>
    /// <typeparam name="T">The type of service to register.</typeparam>
    protected void AddService<T>()
    {
      var serviceType = typeof (T);
      var objectType = typeof (object);
      while (serviceType!=objectType) {
        if (!supportedServices.Contains(serviceType))
          supportedServices.Add(serviceType);
        serviceType = serviceType.BaseType;
      }
      var interfaces = serviceType.GetInterfaces();
      foreach (var interfaceType in interfaces) {
        if (!supportedServices.Contains(interfaceType))
          supportedServices.Add(interfaceType);
      }
    }

    #endregion

    #region ICachingProvider & cache related methods methods

    /// <inheritdoc/>
    bool ICachingProvider.IsResultCached(EnumerationContext context) 
    {
      ArgumentValidator.EnsureArgumentNotNull(context, "context");
      var cp = GetService<ICachingProvider>();
      if (cp==null)
        return true;
      return GetCachedResult(context)!=null;
    }

    /// <inheritdoc/>
    void ICachingProvider.EnsureResultIsCached(EnumerationContext context) 
    {
      ArgumentValidator.EnsureArgumentNotNull(context, "context");
      var cp = GetService<ICachingProvider>();
      if (cp==null)
        return;
      if (GetCachedResult(context)==null)
        SetCachedResult(context, OnEnumerate(context));
    }

    protected T GetCachedValue<T>(object key, EnumerationContext context)
      where T : class
    {
      if (context.IsActive)
        return context.GetValue<T>(new Pair<object, object>(this, key));
      return null;
    }

    protected void SetCachedValue<T>(object key, T value, EnumerationContext context)
      where T : class
    {
      if (context.IsActive)
        context.SetValue(new Pair<object, object>(this, key), value);
    }

    #endregion

    #region IEnumerable<...> methods

    /// <inheritdoc/>
    public sealed override IEnumerator<Tuple> GetEnumerator()
    {
      var context = EnumerationScope.CurrentContext;
      if (context == null)
        context = new EnumerationContext();
      var scope = context.Activate();
      OnBeforeEnumerate(context);
      try {
        foreach (var tuple in Enumerate(context))
          yield return tuple;
      }
      finally {
        OnAfterEnumerate(context);
        scope.DisposeSafely();
      }
      
    }

    #endregion

    #region Private \ internal methods

    private IEnumerable<Tuple> GetCachedResult(EnumerationContext context)
    {
      return context.GetValue<IEnumerable<Tuple>>(new Pair<object, object>(this, CachedResultKey));
    }

    private void SetCachedResult(EnumerationContext context, IEnumerable<Tuple> value) 
    {
      context.SetValue(new Pair<object, object>(this, CachedResultKey), value);
    }

    #endregion

    
    // Constructor

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="origin">The <see cref="Origin"/> property value.</param>
    /// <param name="sources">The <see cref="Provider.Sources"/> property value.</param>
    protected ExecutableProvider(Provider origin, params ExecutableProvider[] sources)
      : base(sources)
    {
      Origin = origin;
    }
  }
}
