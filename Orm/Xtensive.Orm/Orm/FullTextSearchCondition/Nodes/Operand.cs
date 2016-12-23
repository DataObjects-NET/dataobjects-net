// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.12.08

using Xtensive.Orm.FullTextSearchCondition.Interfaces;
using Xtensive.Orm.FullTextSearchCondition.Internals;

namespace Xtensive.Orm.FullTextSearchCondition.Nodes
{
  /// <summary>
  /// <see cref="IOperand"/> implementation, base class for other operands.
  /// </summary>
  public abstract class Operand : IOperand
  {
    /// <inheritdoc/>
    public IOperator Source { get; private set; }

    /// <inheritdoc/>
    public SearchConditionNodeType NodeType { get; private set; }

    /// <inheritdoc/>
    public IOperator And()
    {
      return SearchConditionNodeFactory.CreateAnd(this);
    }

    /// <inheritdoc/>
    public IOperator Or()
    {
      return SearchConditionNodeFactory.CreateOr(this);
    }

    /// <inheritdoc/>
    public IOperator AndNot()
    {
      return SearchConditionNodeFactory.CreateAndNot(this);
    }

    public void AcceptVisitor(ISearchConditionNodeVisitor visitor)
    {
      AcceptVisitorInternal(visitor);
    }

    protected abstract void AcceptVisitorInternal(ISearchConditionNodeVisitor visitor);

    internal Operand(SearchConditionNodeType nodeType, IOperator sourceOperator)
    {
      NodeType = nodeType;
      Source = sourceOperator;
    }
  }
}
