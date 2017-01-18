﻿// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.12.08

using Xtensive.Orm.FullTextSearchCondition.Interfaces;

namespace Xtensive.Orm.FullTextSearchCondition.Nodes
{
  /// <summary>
  /// AND operator.
  /// </summary>
  public sealed class And : Operator
  {
    protected override void AcceptVisitorInternal(ISearchConditionNodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal And(IOperand source)
      : base(SearchConditionNodeType.And, source)
    {
    }
  }
}
