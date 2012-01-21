// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.07

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Xtensive.Core;
using Xtensive.Helpers;
using Xtensive.Internals.DocTemplates;
using Xtensive.Orm.Rse.Compilation;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using IEnumerable = System.Collections.IEnumerable;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Abstract base class for any query provider that can be directly executed.
  /// </summary>
  [Serializable]
  public abstract class ExecutableProvider : Provider,
    IEnumerable<Tuple>,
    IHasServices,
    ICachingProvider
  {
    protected const string ToString_Origin = "[Origin: {0}]";
    private const string CachedResultName = "Results";

    private readonly HashSet<Type> supportedServices = new HashSet<Type>();

    /// <summary>
    /// Gets the provider this provider is compiled from.
    /// </summary>
    public CompilableProvider Origin { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance can be
    /// cached and further returned as result of compilation of
    /// <see cref="Origin"/> once more by the same 
    /// <see cref="CompilationService"/>.
    /// </summary>
    public bool IsCacheable { get; protected set; }

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
    public virtual void OnBeforeEnumerate(EnumerationContext context)
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
    public virtual void OnAfterEnumerate(EnumerationContext context)
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
    public abstract IEnumerable<Tuple> OnEnumerate(EnumerationContext context);

    #endregion

    #region IHasServices methods

    /// <inheritdoc/>
    /// <remarks>
    /// The implementation of this method checks if specified service <typeparamref name="T"/>
    /// was registered by <see cref="AddService{T}"/>, and returns <c>this as T</c>, if this is <see langword="true" />;
    /// otherwise, <see langword="null" />.
    /// </remarks>
    public virtual T GetService<T>() where T :class
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
      return context.GetValue<IEnumerable<Tuple>>(provider, CachedResultName);
    }

    private static void SetCachedResult(EnumerationContext context, ICachingProvider provider, IEnumerable<Tuple> value) 
    {
      context.EnsureIsActive();
      context.SetValue(provider, CachedResultName, value);
    }

    #endregion

    #region Caching related methods

    protected T GetCachedValue<T>(EnumerationContext context, string name)
      where T : class
    {
      context.EnsureIsActive();
      return context.GetValue<T>(this, name);
    }

    protected void SetCachedValue<T>(EnumerationContext context, string name, T value)
      where T : class
    {
      context.EnsureIsActive();
      context.SetValue(this, name, value);
    }

    #endregion

    #region IEnumerable<...> methods

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <inheritdoc/>
    public IEnumerator<Tuple> GetEnumerator()
    {
      const string enumerationMarker = "Enumerated";
      var context = EnumerationScope.CurrentContext;
      var enumerated = context.GetValue<bool>(this, enumerationMarker);
      if (!enumerated)
        OnBeforeEnumerate(context);
      try {
        context.SetValue(this, enumerationMarker, true);
        var enumerable = Enumerate(context);
        foreach (var tuple in enumerable)
          yield return tuple;
      }
      finally {
        if (!enumerated)
          OnAfterEnumerate(context);
      }
    }

    #endregion

    #region ToString related methods

    protected override void AppendDescriptionTo(StringBuilder builder, int indent)
    {
      // Could append Origin to desctiprion part of ToString(),
      // But here it does nothing.
      // AppendOriginTo(builder, indent);
    }

    protected virtual void AppendOriginTo(StringBuilder sb, int indent)
    {
      if (Origin==null)
        return;
      sb.Append(string.Format(ToString_Origin, Origin.TitleToString()).Indent(indent))
        .AppendLine();
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


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="origin">The <see cref="Origin"/> property value.</param>
    /// <param name="sources">The <see cref="Provider.Sources"/> property value.</param>
    protected ExecutableProvider(CompilableProvider origin, params ExecutableProvider[] sources)
      : base(origin.Type, sources)
    {
      if (origin==null)
        throw new ArgumentNullException("origin");
      Origin = origin;
      IsCacheable = true;
    }
  }
}
