// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.30

using System;
using System.Linq.Expressions;

namespace Xtensive.Core.Linq.ComparisonExtraction
{
  /// <summary>
  /// Extractor of comparison operation from <see cref="Expression"/>.
  /// </summary>
  public class ComparisonExtractor
  {
    /// <summary>
    /// Extracts an information about a comparison operation. A comparison is
    /// considered regarding a key selected by <paramref name="keySelector"/>.
    /// </summary>
    /// <param name="exp">The <see cref="Expression"/> containing a comparison
    /// operation.</param>
    /// <param name="keySelector">The key selector.</param>
    /// <returns>An information about a comparison operation or <see langword="null"/>,
    /// if a comparison operation was not found.</returns>
    public ComparisonInfo Extract(Expression exp, Func<Expression, bool> keySelector)
    {
      ArgumentValidator.EnsureArgumentNotNull(exp, "exp");
      ArgumentValidator.EnsureArgumentNotNull(keySelector, "keySelector");
      ExtractionInfo extractionInfo = BaseExtractorState.InitialState.Extract(exp, keySelector);
      if (extractionInfo == null)
        return null;
      return ComparisonInfo.TryCreate(extractionInfo);
    }
  }
}