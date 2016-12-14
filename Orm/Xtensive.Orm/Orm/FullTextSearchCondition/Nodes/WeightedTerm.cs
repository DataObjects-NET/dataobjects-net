// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.12.08

using System.Collections.Generic;
using Xtensive.Orm.FullTextSearchCondition.Interfaces;

namespace Xtensive.Orm.FullTextSearchCondition.Nodes
{
  /// <summary>
  /// <see cref="IWeightedTerm"/> implementation.
  /// </summary>
  public sealed class WeightedTerm : Operand, IWeightedTerm
  {
    /// <inheritdoc/>
    public IDictionary<IWeighableTerm, float?> WeighedTerms { get; private set; }

    protected override void AcceptVisitorInternal(ISearchConditionNodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal WeightedTerm(IOperator source, IDictionary<IWeighableTerm, float?> weightedTerms)
      : base(SearchConditionNodeType.WeightedTerm, source)
    {
      WeighedTerms = weightedTerms;
    }
  }
}