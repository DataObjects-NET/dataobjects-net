// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.12.08

namespace Xtensive.Orm.FullTextSearchCondition.Interfaces
{
  /// <summary>
  /// Specifies a match for an exact word or a phrase
  /// </summary>
  public interface ISimpleTerm : IProximityOperand
  {
    /// <summary>
    /// Word or phrase.
    /// </summary>
    string Term { get; }
  }
}