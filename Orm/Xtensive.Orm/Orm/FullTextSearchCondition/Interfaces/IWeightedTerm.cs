// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.12.08

using System.Collections.Generic;

namespace Xtensive.Orm.FullTextSearchCondition.Interfaces
{
  /// <summary>
  /// Specifies that the matching rows match a list of words and phrases, each optionally given a weighting value.
  /// </summary>
  public interface IWeightedTerm : IOperand
  {
    /// <summary>
    /// Terms mapped to its weights.
    /// </summary>
    IDictionary<IWeighableTerm, float?> WeighedTerms { get; }
  }
}