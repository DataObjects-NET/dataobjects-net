// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.07

using System;
using System.Collections.Generic;
using System.Threading;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse.Resources;

namespace Xtensive.Storage.Rse.Providers
{
  /// <summary>
  /// Abstract base class for any <see cref="RecordSet"/> <see cref="RecordSet.Provider"/>,
  /// that does not need to be compiled.
  /// </summary>
  [Serializable]
  public abstract class ExecutableProvider : Provider,
    IPreparingProvider
  {
    private const string PreparedPropertyKey = "Prepared";
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
    /// Gets the sequence this provider provides.
    /// </summary>
    protected abstract IEnumerable<Tuple> Execute();

    #region SetCachedValue, GetCachedValue methods

    /// <summary>
    /// Caches the value in the current <see cref="ExecutionContext"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    protected void SetCachedValue<T>(string key, T value)
      where T: class
    {
      var c = ExecutionContext.Current;
      if (c==null)
        return;
      c.SetCachedValue(new Pair<object, string>(this, key), value);
    }

    /// <summary>
    /// Gets the cached value from the current <see cref="ExecutionContext"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <returns>Cached value with the specified key;
    /// <see langword="null"/>, if no cached value is found, or it is already expired.</returns>
    protected T GetCachedValue<T>(string key)
      where T: class
    {
      var c = ExecutionContext.Current;
      if (c==null)
        return null;
      return c.GetCachedValue<T>(new Pair<object, string>(this, key));
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

    #region IPreparingProvider methods

    private IEnumerable<Tuple> Prepared {
      get {
        return GetCachedValue<IEnumerable<Tuple>>(PreparedPropertyKey);
      }
      set {
        SetCachedValue(PreparedPropertyKey, value);
      }
    }

    /// <inheritdoc/>
    public bool IsPrepared {
      get {
        var pp = GetService<IPreparingProvider>();
        if (pp==null)
          return true;
        return Prepared!=null;
      }
    }

    /// <inheritdoc/>
    public void Prepare()
    {
      var pp = GetService<IPreparingProvider>();
      if (pp==null)
        return;
      ExecutionContext.GetCurrent(true);
      if (Prepared==null)
        Prepared = Execute();
    }

    #endregion

    #region IEnumerable<...> methods

    /// <inheritdoc/>
    public sealed override IEnumerator<Tuple> GetEnumerator()
    {
      var c = ExecutionContext.Current;
      IDisposable d = null;
      if (c==null) {
        c = new ExecutionContext();
        d = c.Activate();
      }
      using (d) {
        if (Prepared!=null)
          return Prepared.GetEnumerator();
        else
          return Execute().GetEnumerator();
      }
    }

    #endregion


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="origin">The <see cref="Origin"/> property value.</param>
    /// <param name="sources">The <see cref="Provider.Sources"/> property value.</param>
    protected ExecutableProvider(CompilableProvider origin, params Provider[] sources)
      : base(sources)
    {
      Origin = origin;
    }
  }
}