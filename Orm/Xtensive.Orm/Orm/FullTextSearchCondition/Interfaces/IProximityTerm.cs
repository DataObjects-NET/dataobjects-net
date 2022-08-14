// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.12.08

using System.Collections.Generic;

namespace Xtensive.Orm.FullTextSearchCondition.Interfaces
{
  /// <summary>
  /// Generic proximity term. Allows to set several terms as located near each other.
  /// </summary>
  public interface IProximityTerm : IOperand
  {
    /// <summary>
    /// Gets list of near terms.
    /// </summary>
    IReadOnlyList<IProximityOperand> Terms { get; }
  }
}