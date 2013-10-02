// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.21

using System;

namespace Xtensive.Conversion
{
  /// <summary>
  /// Provides <see cref="AdvancedConverter{TFrom,TTo}"/>s.
  /// </summary>
  public interface IAdvancedConverterProvider
  {
    /// <summary>
    /// Gets <see cref="IAdvancedConverter{TFrom,TTo}"/> for specified types <typeparamref name="TFrom"/> and <typeparamref name="TTo"/>.
    /// </summary>
    /// <typeparam name="TFrom">Type to convert from.</typeparam>
    /// <typeparam name="TTo">Type to convert to.</typeparam>
    /// <returns><see cref="IAdvancedConverter{TFrom,TTo}"/> instance  for specified types <typeparamref name="TFrom"/> and <typeparamref name="TTo"/>.</returns>
    AdvancedConverter<TFrom, TTo> GetConverter<TFrom, TTo>();

    /// <summary>
    /// Gets base time for <see cref="DateTime"/> conversions.
    /// </summary>
    DateTime BaseTime{ get;}
  }
}