// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.08

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Compilable provider that makes runtime index over the <see cref="UnaryProvider.Source"/> values using <see cref="Order"/>.
  /// </summary>
  [Serializable]
  public sealed class IndexingProvider : UnaryProvider
  {
    /// <summary>
    /// Sort order of the index.
    /// </summary>
    public DirectionCollection<int> Order { get; private set; }

    /// <summary>
    /// Gets the key extractor transform.
    /// </summary>
    public MapTransform OrderKeyExtractorTransform { get; private set; }

    /// <summary>
    /// Gets the <see cref="Order"/> key comparer.
    /// </summary>
    public AdvancedComparer<Tuple> OrderKeyComparer { get; private set; }

    /// <summary>
    /// Extracts the key part from <paramref name="tuple"/> using <see cref="OrderKeyExtractorTransform"/>.
    /// </summary>
    /// <param name="tuple">The tuple to extract the key from.</param>
    /// <returns>A tuple containing extracted order key.</returns>
    public Tuple OrderKeyExtractor(Tuple tuple)
    {
      return OrderKeyExtractorTransform.Apply(TupleTransformType.Auto, tuple); ;
    }

    /// <inheritdoc/>
    protected override RecordSetHeader BuildHeader()
    {
      return new RecordSetHeader(
        Source.Header.TupleDescriptor, 
        Source.Header.Columns, 
        Source.Header.OrderDescriptor.TupleDescriptor, 
        Source.Header.ColumnGroups, 
        Order);
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      var comparisonRules = new ComparisonRules[Order.Count];
      for (int i = 0; i < Order.Count; i++) {
        var orderItem = Order[i];
        CultureInfo culture = Header.Columns[orderItem.Key].ColumnInfoRef != null
          ? Header.Columns[orderItem.Key].ColumnInfoRef.CultureInfo
          : CultureInfo.InvariantCulture;
        comparisonRules[i] = new ComparisonRule(orderItem.Value, culture);
      }

      var orderKeyDescriptor = TupleDescriptor.Create(
        Order.Select(p => Header.Columns[p.Key].Type));
      OrderKeyExtractorTransform = new MapTransform(true, orderKeyDescriptor, 
        Order.Select(p => p.Key).ToArray());
      OrderKeyComparer = AdvancedComparer<Tuple>.Default.ApplyRules(
        new ComparisonRules(ComparisonRule.Positive, comparisonRules, ComparisonRules.None));
    }

    /// <inheritdoc/>
    public override string ParametersToString()
    {
      return Order
        .Select(pair => Header.Columns[pair.Key].Name + (pair.Value == Direction.Negative ? " desc" : string.Empty))
        .ToCommaDelimitedString();
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="order">The <see cref="Order"/> property value.</param>
    public IndexingProvider(CompilableProvider source, DirectionCollection<int> order)
      : base(source)
    {
      Order = order;
    }
  }
}