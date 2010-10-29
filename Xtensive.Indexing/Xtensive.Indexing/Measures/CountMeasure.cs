// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.11.22

using System;
using System.Diagnostics;
using Xtensive.Arithmetic;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Indexing.Measures
{
  /// <summary>
  /// A measure providing count of items.
  /// </summary>
  /// <typeparam name="TItem">Type of measured item.</typeparam>
  /// <typeparam name="TResult">Type of measurement result.</typeparam>
  [Serializable]
  public sealed class CountMeasure<TItem, TResult>: AdditiveMeasure<TItem, TResult>
  {
    /// <summary>
    /// Common (the only possible) name of this measure.
    /// Value is "Count".
    /// </summary>
    public static readonly string CommonName = "Count";

    private static readonly Converter<TItem, TResult> resultExtractor = item => one;
    private static readonly TResult one = Arithmetic<TResult>.Default.One;

    /// <inheritdoc/>
    public override IMeasure<TItem> CreateNew()
    {
      return new CountMeasure<TItem, TResult>();
    }

    /// <inheritdoc/>
    public override IMeasure<TItem, TResult> CreateNew(TResult result)
    {
      return new CountMeasure<TItem, TResult>(result);
    }

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    public CountMeasure() 
      : base(CommonName, resultExtractor)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="result">Initial <see cref="MeasureBase{TItem,TResult}.Result"/> property value.</param>
    private CountMeasure(TResult result)
      : base(CommonName, resultExtractor, result)
    {
    }
  }
}