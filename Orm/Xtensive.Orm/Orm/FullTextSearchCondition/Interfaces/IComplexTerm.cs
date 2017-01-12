// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.12.09

namespace Xtensive.Orm.FullTextSearchCondition.Interfaces
{
  /// <summary>
  /// Complex term.
  /// </summary>
  public interface IComplexTerm : IOperand
  {
    /// <summary>
    /// Root operand of an operand sequence wrapped by the complex term.
    /// </summary>
    IOperand RootOperand { get; }
  }
}