// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.07

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xtensive.Core;
using Xtensive.Core.Disposable;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Core.Helpers;
using Xtensive.Storage.Rse.Compilation;

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
    public CompilableProvider Origin { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance can be
    /// cached and further returned as result of compilation of
    /// <see cref="Origin"/> once more by the same 
    /// <see cref="CompilationContext"/>.
    /// </summary>
    public bool IsCacheable { get; protected set; }

    /// <summary>
    /// Gets or sets execution site location.
    /// </summary>
    public UrlInfo Location { get; set; }

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
        return GetCachedResult(context, cp);
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
          ep.OnAfterEnumerate(context);
      }
    }

    /// <summary>
    /// Gets the sequence this provider provides in the specified <see cref="EnumerationContext"/>.
    /// Invoked by <see cref="Enumerate"/> method in case there is no cached result for the specified context.
    /// </summary>
    /// <param name="context">The enumeration context.</param>
    protected internal abstract IEnumerable<Tuple> OnEnumerate(EnumerationContext context);

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
      while (serviceType!=objectType && serviceType != null) {
        if (!supportedServices.Contains(serviceType))
          supportedServices.Add(serviceType);
        serviceType = serviceType.BaseType;
      }
      var interfaces = typeof(T).GetInterfaces();
      foreach (var interfaceType in interfaces) {
        if (!supportedServices.Contains(interfaceType))
          supportedServices.Add(interfaceType);
      }
    }

    #endregion

    #region ICachingProvider methods

    /// <inheritdoc/>
    bool ICachingProvider.IsResultCached(EnumerationContext context) 
    {
      var cp = GetService<ICachingProvider>();
      if (cp==null)
        return false;
      return GetCachedResult(context, cp)!=null;
    }

    /// <inheritdoc/>
    void ICachingProvider.EnsureResultIsCached(EnumerationContext context) 
    {
      ArgumentValidator.EnsureArgumentNotNull(context, "context");
      var cp = GetService<ICachingProvider>();
      if (cp==null)
        return;
      if (GetCachedResult(context, cp)==null) {
        var ep = (ExecutableProvider) cp;
        SetCachedResult(context, cp, ep.OnEnumerate(context));
      }
    }

    private static IEnumerable<Tuple> GetCachedResult(EnumerationContext context, ICachingProvider provider)
    {
      context.EnsureIsActive();
      return context.GetValue<IEnumerable<Tuple>>(new Pair<object, string>(provider, CachedResultKey));
    }

    private static void SetCachedResult(EnumerationContext context, ICachingProvider provider, IEnumerable<Tuple> value) 
    {
      context.EnsureIsActive();
      context.SetValue(new Pair<object, string>(provider, CachedResultKey), value);
    }

    #endregion

    #region Caching related methods

    protected T GetCachedValue<T>(EnumerationContext context, string key)
      where T : class
    {
      context.EnsureIsActive();
      return context.GetValue<T>(new Pair<object, string>(this, key));
    }

    protected void SetCachedValue<T>(EnumerationContext context, string key, T value)
      where T : class
    {
      context.EnsureIsActive();
      context.SetValue(new Pair<object, string>(this, key), value);
    }

    #endregion

    #region IEnumerable<...> methods

    /// <inheritdoc/>
    public sealed override IEnumerator<Tuple> GetEnumerator()
    {
      var context = EnumerationScope.CurrentContext;
      OnBeforeEnumerate(context);
      try {
        var enumerable = Enumerate(context);
        foreach (var tuple in enumerable)
          yield return tuple;
      }
      finally {
        OnAfterEnumerate(context);
      }
    }

    #endregion

    #region ToString related methods

    protected internal override void AppendBodyTo(StringBuilder sb, int indent)
    {
      int nextIndent = indent + ToString_IndentSize;
      AppendTitleTo(sb, indent);
      AppendOriginTo(sb, nextIndent);
      foreach (Provider source in Sources)
        source.AppendBodyTo(sb, nextIndent);
    }

    protected virtual void AppendOriginTo(StringBuilder sb, int indent)
    {
      if (Origin==null)
        return;
      sb.Append(new string(' ', indent))
        .Append("[Origin: ")
        .Append(Origin.TitleToString())
        .Append("]");
    }

    /// <inheritdoc/>
    public override string ParametersToString()
    {
      return Origin.ParametersToString();
    }

    #endregion

    /// <exception cref="InvalidOperationException"><see cref="Origin"/> is <see langword="null" />.</exception>
    protected override RecordSetHeader BuildHeader()
    {
      return Origin.Header;
    }

    /// <exception cref="ArgumentNullException"><see cref="Origin"/> is null.</exception>
    protected override void Initialize()
    {
      base.Initialize();
      bool isCacheable = IsCacheable;
      foreach (var source in Sources) {
        var ep = source as ExecutableProvider;
        if (ep!=null)
          isCacheable &= ep.IsCacheable;
      }
      IsCacheable = isCacheable;
    }


    // Constructor

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="origin">The <see cref="Origin"/> property value.</param>
    /// <param name="sources">The <see cref="Provider.Sources"/> property value.</param>
    protected ExecutableProvider(CompilableProvider origin, params ExecutableProvider[] sources)
      : base(sources)
    {
      if (origin==null)
        throw new ArgumentNullException("origin");
      Origin = origin;
      IsCacheable = true;
    }
  }
}
