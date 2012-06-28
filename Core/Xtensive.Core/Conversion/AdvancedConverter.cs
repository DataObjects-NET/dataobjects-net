// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.15

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using Xtensive.Core;
using Xtensive.Helpers;
using Xtensive.Internals.DocTemplates;


namespace Xtensive.Conversion
{
  /// <summary>
  /// Provides delegates allowing to call conversion methods faster.
  /// </summary>
  /// <typeparam name="TFrom">Type of the first <see cref="IAdvancedConverter{TFrom,TTo}"/> generic argument.</typeparam>
  /// <typeparam name="TTo">Type of the second <see cref="IAdvancedConverter{TFrom,TTo}"/> generic argument.</typeparam>
  /// <remarks>
  /// <para id="About"><see cref="HasStaticDefaultDocTemplate" copy="true" /></para>
  /// </remarks>
  [Serializable]
  public sealed class AdvancedConverter<TFrom, TTo> : MethodCacheBase<IAdvancedConverter<TFrom, TTo>>
  {
    private static ThreadSafeCached<AdvancedConverter<TFrom, TTo>> cachedConverter =
      ThreadSafeCached<AdvancedConverter<TFrom, TTo>>.Create(new object());

    /// <summary>
    /// Gets default advanced converter for types <typeparamref name="TFrom"/> and <typeparamref name="TTo"/>.
    /// (uses <see cref="AdvancedConverterProvider.Default"/> <see cref="AdvancedConverter{TFrom,TTo}"/>).
    /// </summary>
    public static AdvancedConverter<TFrom, TTo> Default {
      [DebuggerStepThrough]
      get {
        return cachedConverter.GetValue(
          () => AdvancedConverterProvider.Default.GetConverter<TFrom, TTo>());
      }
    }

    /// <summary>
    /// Gets the provider underlying converter is associated with.
    /// </summary>
    public readonly IAdvancedConverterProvider Provider;

    /// <summary>
    /// Gets <see cref="IAdvancedConverter{TFrom,TTo}.Convert"/> method delegate.
    /// </summary>
    public readonly Converter<TFrom, TTo> Convert;

    /// <summary>
    /// Gets <see cref="IAdvancedConverter{TFrom,TTo}.IsRough"/> value.
    /// </summary>
    public readonly bool IsRough;


    // Constructos

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="implementation">Advanced converter to provide the delegates for.</param>
    public AdvancedConverter(IAdvancedConverter<TFrom, TTo> implementation)
      : base(implementation)
    {
      Provider = Implementation.Provider;
      Convert = Implementation.Convert;
      IsRough = Implementation.IsRough;
    }

    /// <summary>
    /// Deserializes the instance of this class.
    /// </summary>
    /// <param name="info">Serialization info.</param>
    /// <param name="context">Streaming context.</param>
    public AdvancedConverter(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Provider = Implementation.Provider;
      Convert = Implementation.Convert;
      IsRough = Implementation.IsRough;
    }
  }
}