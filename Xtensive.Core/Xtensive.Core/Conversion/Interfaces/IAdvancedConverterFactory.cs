// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.14

using System;

namespace Xtensive.Conversion
{
  /// <summary>
  /// A factory class creating complex <see cref="IAdvancedConverter{TFrom,TTo}"/>.
  /// Usually implemented instead of converter for some complex type, e.g.
  /// <see cref="Nullable{T}"/> to handle requests for its converters.
  /// </summary>
  /// <typeparam name="TFrom">The source type (the type to convert from).</typeparam>
  public interface IAdvancedConverterFactory<TFrom>
  {
    /// <summary>
    /// Creates forward-converting converter.
    /// </summary>
    /// <typeparam name="TTo">The destination type (the type to convert to).</typeparam>
    /// <returns>Forward-converting converter.</returns>
    IAdvancedConverter<TFrom, TTo> CreateForwardConverter<TTo>();

    /// <summary>
    /// Creates backward-converting converter.
    /// </summary>
    /// <typeparam name="TTo">The destination type (the type to convert to).</typeparam>
    /// <returns>Backward-converting converter.</returns>
    IAdvancedConverter<TTo, TFrom> CreateBackwardConverter<TTo>();
  }
}