// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.11.22

using System;
using System.Diagnostics;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Indexing.Measures
{
  /// <summary>
  /// A measure providing sum of the items.
  /// </summary>
  /// <typeparam name="TItem">Type of measured item.</typeparam>
  /// <typeparam name="TResult">Type of measure value.</typeparam>
  [Serializable]
  public sealed class SumMeasure<TItem, TResult> : AdditiveMeasure<TItem, TResult>
  {
    /// <inheritdoc/>
    public override IMeasure<TItem> CreateNew()
    {
      return new SumMeasure<TItem, TResult>(Name, ResultExtractor);
    }

    /// <inheritdoc/>
    public override IMeasure<TItem, TResult> CreateNew(TResult result)
    {
      return new SumMeasure<TItem, TResult>(Name, ResultExtractor, result);
    }


    // Constructors

    /// <inheritdoc/>
    public SumMeasure(string name, Converter<TItem, TResult> resultExtractor)
      : base(name, resultExtractor)
    {
    }

    /// <inheritdoc/>
    private SumMeasure(string name, Converter<TItem, TResult> resultExtractor, TResult result)
      : base(name, resultExtractor, result)
    {
    }
  }
}