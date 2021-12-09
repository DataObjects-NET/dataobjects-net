// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.12.08

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.FullTextSearchCondition.Interfaces;

namespace Xtensive.Orm.FullTextSearchCondition.Nodes
{
  /// <summary>
  /// <see cref="ICustomProximityTerm"/> implementation.
  /// </summary>
  public sealed class CustomProximityTerm : Operand, ICustomProximityTerm
  {
    /// <inheritdoc/>
    public IReadOnlyList<IProximityOperand> Terms { get; private set; }

    /// <inheritdoc/>
    public long? MaxDistance { get; private set; }

    /// <inheritdoc/>
    public bool MatchOrder { get; private set; }

    protected override void AcceptVisitorInternal(ISearchConditionNodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal CustomProximityTerm(IOperator source, ICollection<IProximityOperand> proximityTerms)
      : this(source, proximityTerms, int.MaxValue, false)
    {
      MaxDistance = null;
    }

    internal CustomProximityTerm(IOperator source, ICollection<IProximityOperand> proximityTerms, long maxDistance)
      : this(source, proximityTerms, maxDistance, false)
    {
    }

    internal CustomProximityTerm(IOperator source, ICollection<IProximityOperand> proximityTerms, long maxDistance, bool matchOrder)
      : base(SearchConditionNodeType.CustomProximityTerm, source)
    {
      if (proximityTerms.Count < 2)
        throw new ArgumentException(string.Format(Strings.ExCollectionShouldContainAtLeastXElements, 2));
      ArgumentValidator.EnsureArgumentNotNull(proximityTerms, "proximityTerms");
      ArgumentValidator.EnsureArgumentIsGreaterThanOrEqual(maxDistance, 0, "maxDistance");

      Terms = proximityTerms.ToList().AsReadOnly();
      MaxDistance = maxDistance;
      MatchOrder = matchOrder;
    }
  }
}