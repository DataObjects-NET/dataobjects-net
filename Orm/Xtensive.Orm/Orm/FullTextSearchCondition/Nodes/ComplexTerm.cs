// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.12.09

using Xtensive.Core;
using Xtensive.Orm.FullTextSearchCondition.Interfaces;

namespace Xtensive.Orm.FullTextSearchCondition.Nodes
{
  /// <summary>
  /// <see cref="IComplexTerm"/> implementation.
  /// </summary>
  public sealed class ComplexTerm : Operand, IComplexTerm
  {
    /// <inheritdoc/>
    public IOperand RootOperand { get; private set; }

    protected override void AcceptVisitorInternal(ISearchConditionNodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal ComplexTerm(IOperator source, IOperand operandsSequenceRoot)
      : base(SearchConditionNodeType.ComplexTerm, source)
    {
      ArgumentValidator.EnsureArgumentNotNull(operandsSequenceRoot, "operandsSequenceRoot");
      RootOperand = operandsSequenceRoot;
    }
  }
}
