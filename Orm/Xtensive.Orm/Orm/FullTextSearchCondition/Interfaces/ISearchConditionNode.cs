// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.12.08

namespace Xtensive.Orm.FullTextSearchCondition.Interfaces
{
  /// <summary>
  /// Node used in search condition.
  /// </summary>
  public interface ISearchConditionNode
  {
    /// <summary>
    /// Type of node.
    /// </summary>
    SearchConditionNodeType NodeType { get; }

    /// <summary>
    /// Accepts visitor to the node
    /// </summary>
    /// <param name="visitor">A visitor accepted to the node.</param>
    void AcceptVisitor(ISearchConditionNodeVisitor visitor);
  }
}