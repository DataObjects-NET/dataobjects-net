// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.12.08

using Xtensive.Core;
using Xtensive.Orm.FullTextSearchCondition.Interfaces;

namespace Xtensive.Orm.FullTextSearchCondition.Nodes
{
  /// <summary>
  /// <see cref="ISimpleTerm"/> implementation.
  /// </summary>
  public sealed class SimpleTerm : Operand, ISimpleTerm, IWeighableTerm
  {
    /// <inheritdoc/>
    public string Term { get; private set; }

    protected override void AcceptVisitorInternal(ISearchConditionNodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SimpleTerm(string term)
      : this(null, term)
    {
    }

    internal SimpleTerm(IOperator source, string term)
      : base(SearchConditionNodeType.SimpleTerm, source)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmptyOrWhiteSpace(term, "term");
      Term = term;
    }
  }
}
