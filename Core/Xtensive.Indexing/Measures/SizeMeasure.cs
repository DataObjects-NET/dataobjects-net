// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.04.18

using System;
using System.Diagnostics;
using Xtensive.Internals.DocTemplates;
using Xtensive.Indexing.SizeCalculators;

namespace Xtensive.Indexing.Measures
{
  /// <summary>
  /// A measure providing size allocated by the items.
  /// </summary>
  /// <typeparam name="TItem">Type of measured item.</typeparam>
  [Serializable]
  public sealed class SizeMeasure<TItem> : AdditiveMeasure<TItem, long>
  {
    /// <summary>
    /// Common (the only possible) name of this measure.
    /// Value is "Size".
    /// </summary>
    public static readonly string CommonName = "Size";

    private static readonly Converter<TItem, long> resultExtractor = item => sizeCalculator.GetValueSize(item);
    private static readonly SizeCalculator<TItem> sizeCalculator = SizeCalculator<TItem>.Default;

    /// <inheritdoc/>
    public override IMeasure<TItem> CreateNew()
    {
      return new SizeMeasure<TItem>();
    }

    /// <inheritdoc/>
    public override IMeasure<TItem, long> CreateNew(long result)
    {
      return new SizeMeasure<TItem>(result);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    public SizeMeasure() 
      : base(CommonName, resultExtractor)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="result">Initial <see cref="MeasureBase{TItem,TResult}.Result"/> property value.</param>
    private SizeMeasure(long result)
      : base(CommonName, resultExtractor, result)
    {
    }
  }
}