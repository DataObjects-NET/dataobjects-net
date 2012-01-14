// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.02.13

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Comparison;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Indexing.Measures
{
  /// <summary>
  /// Base class for any comparable measure.
  /// </summary>
  /// <typeparam name="TItem">Type of measured item.</typeparam>
  /// <typeparam name="TResult">Type of measure value.</typeparam>
  [Serializable]
  public abstract class ComparableMeasure<TItem, TResult> : MeasureBase<TItem, TResult>, 
    IDeserializationCallback
  {
    [NonSerialized]
    protected AdvancedComparer<TResult> comparer;
    private int count;
   
    /// <summary>
    /// Gets or sets the count of the same items.
    /// </summary>
    public int Count
    {
      [DebuggerStepThrough]
      get { return count; }
      set {
        count = value;
        if (count <= 0)
          Reset();
      }
    }

    /// <inheritdoc/>
    public override void Reset()
    {
      base.Reset();
      count = 0;
    }


    // Constructors

    /// <inheritdoc/>
    protected ComparableMeasure(string name, Converter<TItem, TResult> resultExtractor)
      : base(name, resultExtractor)
    {
      comparer = AdvancedComparer<TResult>.Default;
    }

    /// <inheritdoc/>
    protected ComparableMeasure(string name, Converter<TItem, TResult> resultExtractor, TResult result)
      : base(name, resultExtractor, result)
    {
      comparer = AdvancedComparer<TResult>.Default;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="name"><see cref="MeasureBase{TItem,TResult}.Name"/> property value.</param>
    /// <param name="resultExtractor"><see cref="MeasureBase{TItem,TResult}.ResultExtractor"/> property value.</param>
    /// <param name="result">Initial <see cref="MeasureBase{TItem,TResult}.Result"/> property value.</param>
    /// <param name="count">Initial <see cref="Count"/> property value.</param>
    protected ComparableMeasure(string name, Converter<TItem, TResult> resultExtractor, TResult result, int count)
      : this(name, resultExtractor, result)
    {
      this.count = count;
    }

    /// <see cref="SerializableDocTemplate.OnDeserialization"/>
    void IDeserializationCallback.OnDeserialization(object sender)
    {
      comparer = AdvancedComparer<TResult>.Default;
    }
  }
}