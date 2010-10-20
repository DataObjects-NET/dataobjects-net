// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.30

using System;
using System.Linq.Expressions;
using Xtensive.Core;

namespace Xtensive.Linq
{
  /// <summary>
  /// Extractor of comparison operation from <see cref="Expression"/>.
  /// </summary>
  public class ComparisonExtractor
  {
    private readonly InitialExtractorState intialExtractorState = new InitialExtractorState();
    private readonly KeySearcher keySearcher = new KeySearcher();

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
      if (!IsBoolleanExpression(exp))
        return null;
      ExtractionInfo extractionInfo = intialExtractorState.Extract(exp, keySelector);
      if (extractionInfo == null)
        return null;
      return ComparisonInfo.TryCreate(extractionInfo);
    }

    /// <summary>
    /// Determines whether <paramref name="exp"/> contains key.
    /// </summary>
    /// <param name="exp">The expression.</param>
    /// <param name="keySelector">The key selector.</param>
    /// <returns>
    ///   <see langword="true"/> if <paramref name="exp"/> contains key; otherwise, <see langword="false"/>.
    /// </returns>
    public bool ContainsKey(Expression exp, Func<Expression, bool> keySelector)
    {
      return keySearcher.ContainsKey(exp, keySelector);
    }

    private static bool IsBoolleanExpression(Expression exp)
    {
      return (exp.NodeType==ExpressionType.Lambda && ((LambdaExpression) exp).Body.Type==typeof (bool))
        || (exp.Type==typeof (bool));
    }
  }
}