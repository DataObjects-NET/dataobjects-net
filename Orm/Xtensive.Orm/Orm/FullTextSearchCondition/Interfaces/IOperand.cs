// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.12.08

namespace Xtensive.Orm.FullTextSearchCondition.Interfaces
{
  /// <summary>
  /// Operand of search condition
  /// </summary>
  public interface IOperand : ISearchConditionNode
  {
    /// <summary>
    /// Source operator
    /// </summary>
    IOperator Source { get; }

    /// <summary>
    /// Creates AND operator and sets this instance as its source.
    /// </summary>
    /// <returns>Operator, ready to continue search condition building.</returns>
    IOperator And();

    /// <summary>
    /// Creates OR operator and sets this instance as its source.
    /// </summary>
    /// <returns>Operator, ready to continue search condition building.</returns>
    IOperator Or();

    /// <summary>
    /// Creates AND NOT operator and sets this instance as its source.
    /// </summary>
    /// <returns>Operator, ready to continue search condition building.</returns>
    IOperator AndNot();
  }
}