// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.11.14

using System;

namespace Xtensive.Indexing.Measures
{
  /// <summary>
  /// Defines measure for some set of items - e.g. count of items.
  /// </summary>
  /// <typeparam name="TItem">The type of item in item collection this measure is defined for.</typeparam>
  /// <typeparam name="TResult">The type of measurement result.</typeparam>
  public interface IMeasure<TItem, TResult>: IMeasure<TItem>
  {
    /// <summary>
    /// Gets the delegate measuring <typeparamref name="TItem"/> - i.e. converting it to <typeparamref name="TResult"/>.
    /// </summary>
    /// <value>The measuring delegate.</value>
    Converter<TItem, TResult> ResultExtractor { get; }

    /// <summary>
    /// Gets result of the measurement.
    /// </summary>
    new TResult Result { get; }

    /// <summary>
    /// Creates new instance of current measure and initializes it with supplied result.
    /// </summary>
    IMeasure<TItem,TResult> CreateNew(TResult result);
  }
}