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
    private const string CachedResultKey = "Prepared";
    private HashSet<Type> supportedServices = new HashSet<Type>();

    /// <summary>
    /// Gets the provider this provider is compiled from.
    /// </summary>
    public CompilableProvider Origin { get; private set; }

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
      else
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
    /// Gets the sequence this provider provides in the specified <see cref="EnumerationContext"/>.
    /// Invoked by <see cref="Enumerate"/> method in case there is no cached result for the specified context.
    /// </summary>
    /// <param name="context">The enumeration context.</param>
    protected abstract IEnumerable<Tuple> OnEnumerate(EnumerationContext context);

    /// <summary>
    /// Called when this provider's enumerator is disposed.
    /// </summary>
    /// <param name="context">The enumeration context.</param>
    protected internal virtual void OnAfterEnumerate(EnumerationContext context)
    {
      foreach (Provider source in Sources) {
        var ep = source as ExecutableProvider;
        if (ep!=null)
          ep.OnAfterEnumerate(context);
      }
    }

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
        supportedServices.Add(serviceType);
        serviceType = serviceType.BaseType;
      }
      // TODO: Add all supported interfaces as well
    }

    #endregion

    #region ICachingProvider methods

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

    #endregion

    #region IEnumerable<...> methods

    /// <inheritdoc/>
    public sealed override IEnumerator<Tuple> GetEnumerator()
    {
      var context = new EnumerationContext(this);
      try {
        return CreateDisposingEnumerator(Enumerate(context), context);
      }
      catch {
        context.Dispose();
        throw;
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

    private IEnumerator<Tuple> CreateDisposingEnumerator(IEnumerable<Tuple> sourceEnumerable, IDisposable toDispose)
    {
      foreach (var item in sourceEnumerable)
        yield return item;
      toDispose.DisposeSafely();
    }

    #endregion


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="origin">The <see cref="Origin"/> property value.</param>
    /// <param name="sources">The <see cref="Provider.Sources"/> property value.</param>
    protected ExecutableProvider(CompilableProvider origin, params ExecutableProvider[] sources)
      : base(sources)
    {
      Origin = origin;
    }
  }
}