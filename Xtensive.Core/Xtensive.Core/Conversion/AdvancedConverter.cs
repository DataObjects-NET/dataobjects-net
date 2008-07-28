// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.15

using System;
using System.Runtime.Serialization;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Conversion
{
  /// <summary>
  /// Provides delegates allowing to call conversion methods faster.
  /// </summary>
  [Serializable]
  public class AdvancedConverter<TFrom, TTo> : MethodCacheBase<IAdvancedConverter<TFrom, TTo>>
  {
    private static readonly object _lock = new object();
    private static volatile AdvancedConverter<TFrom, TTo> @default;

    /// <summary>
    /// Gets default advanced converter for types <typeparamref name="TFrom"/> and <typeparamref name="TTo"/>.
    /// (uses <see cref="AdvancedConverterProvider.Default"/> <see cref="AdvancedConverter{TFrom,TTo}"/>).
    /// </summary>
    public static AdvancedConverter<TFrom, TTo> Default {
      get {
        if (@default==null) lock (_lock) if (@default==null) {
          try {
            @default = AdvancedConverterProvider.Default.GetConverter<TFrom, TTo>();
          }
          catch {
          }
        }
        return @default;
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