// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.12.08

using System.Collections.Generic;

namespace Xtensive.Orm.FullTextSearchCondition.Interfaces
{
  /// <summary>
  /// Specifies a match of words when the included simple terms include variants of the original word for which to search.
  /// </summary>
  public interface IGenerationTerm : IOperand
  {
    /// <summary>
    /// Gets type of generator of term variants
    /// </summary>
    GenerationType GenerationType { get; }

    /// <summary>
    /// Gets words or phrases which are basis for variants' generation.
    /// </summary>
    IReadOnlyList<string> Terms { get; } 
  }
}