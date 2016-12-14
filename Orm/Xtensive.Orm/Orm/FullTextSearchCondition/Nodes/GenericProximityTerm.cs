// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.12.08

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Orm.FullTextSearchCondition.Interfaces;

namespace Xtensive.Orm.FullTextSearchCondition.Nodes
{
  /// <summary>
  /// <see cref="IProximityTerm"/> implementation.
  /// </summary>
  public sealed class GenericProximityTerm : Operand, IProximityTerm, IWeighableTerm
  {
    /// <inheritdoc/>
    public IList<IProximityOperand> Terms { get; private set; }

    protected override void AcceptVisitorInternal(ISearchConditionNodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal GenericProximityTerm(IOperator source, ICollection<IProximityOperand> terms)
      : base(SearchConditionNodeType.GenericProximityTerm, source)
    {
      if (terms.Count < 2)
        throw new ArgumentException(string.Format(Strings.ExCollectionShouldContainAtLeastXElements, 2));
      Terms = new ReadOnlyList<IProximityOperand>(terms.ToList());
    }
  }
}