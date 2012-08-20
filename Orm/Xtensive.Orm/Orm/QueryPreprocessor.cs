// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.08.20

using System;
using System.Linq.Expressions;

namespace Xtensive.Orm
{
  /// <summary>
  /// Base class for LINQ query preprocessor.
  /// </summary>
  public abstract class QueryPreprocessor : IQueryPreprocessor
  {
    /// <summary>
    ///  Applies the preprocessor to the specified query.
    ///  </summary>
    /// <param name="session">Current session.</param>
    /// <param name="query">The query to apply the preprocessor to.</param>
    /// <returns>Application (preprocessing) result.</returns>
    public virtual Expression Apply(Session session, Expression query)
    {
      return query;
    }

    /// <summary>
    /// Applies the preprocessor to the specified query.
    /// This method is not called by DataObjects.Net
    /// and expected to throw <see cref="NotSupportedException"/>.
    /// </summary>
    /// <param name="query">The query to apply the preprocessor to.</param>
    /// <returns>This method does not return.</returns>
    public Expression Apply(Expression query)
    {
      throw new NotSupportedException(Strings.ExThisMethodShouldNotBeCalledUseApplySessionExpressionInstead);
    }

    /// <summary>
    /// Determines whether this query preprocessor is dependent on the <paramref name="other"/> one.
    /// </summary>
    /// <param name="other">The other query preprocessor.</param>
    /// <returns>
    /// <see langword="true"/> if this query preprocessor 
    /// is dependent on <paramref name="other"/>; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public virtual bool IsDependentOn(IQueryPreprocessor other)
    {
      return false;
    }
  }
}