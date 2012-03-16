// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Conversion
{
  /// <summary>
  /// An <see cref="IEnumerable{T}"/> implementor performing
  /// conversion from <typeparamref name="TFrom"/> to
  /// <typeparamref name="TTo"/> on the fly.
  /// </summary>
  /// <typeparam name="TFrom">The item type to convert from.</typeparam>
  /// <typeparam name="TTo">The item type to convert to.</typeparam>
  public class ConvertingEnumerable<TFrom, TTo> : IEnumerable<TTo>
  {
    IEnumerable<TFrom> innerEnumerable;
    Converter<TFrom, TTo> converter;

    /// <inheritdoc/>
    public IEnumerator<TTo> GetEnumerator()
    {
      return new ConvertingEnumerator<TFrom, TTo>(
        innerEnumerable.GetEnumerator(),
        converter);
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return new ConvertingEnumerator<TFrom, TTo>(
        innerEnumerable.GetEnumerator(),
        converter);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="innerEnumerable">Enumerable to wrap.</param>
    /// <param name="converter">Item converter to use.</param>
    public ConvertingEnumerable(IEnumerable<TFrom> innerEnumerable, Converter<TFrom,TTo> converter)
    {
      ArgumentValidator.EnsureArgumentNotNull(innerEnumerable, "innerEnumerable");
      ArgumentValidator.EnsureArgumentNotNull(converter, "converter");
      this.innerEnumerable = innerEnumerable;
      this.converter = converter;
    }
  }

}