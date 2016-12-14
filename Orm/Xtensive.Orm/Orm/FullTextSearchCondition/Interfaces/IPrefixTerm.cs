// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.12.08

namespace Xtensive.Orm.FullTextSearchCondition.Interfaces
{
  /// <summary>
  /// Specifies a match of words or phrases beginning with the specified text.
  /// </summary>
  public interface IPrefixTerm : IProximityOperand
  {
    /// <summary>
    /// Word or phrase which will be automatically followed by asterisk or another replacer of the ending of word or words in a phrase.
    /// </summary>
    string Prefix { get; }
  }
}