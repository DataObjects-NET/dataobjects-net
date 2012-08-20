// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.12.15

using System.Linq.Expressions;

namespace Xtensive.Orm
{
  /// <summary>
  /// LINQ query preprocessor contract.
  /// Consider inheriting from <see cref="QueryPreprocessor"/> instead.
  /// </summary>
  public interface IQueryPreprocessor : IDomainService
  {
    /// <summary>
    /// Applies the preprocessor to the specified query.
    /// </summary>
    /// <param name="query">The query to apply the preprocessor to.</param>
    /// <returns>Application (preprocessing) result.</returns>
    Expression Apply(Expression query);

    /// <summary>
    /// Determines whether this query preprocessor is dependent on the <paramref name="other"/> one.
    /// </summary>
    /// <param name="other">The other query preprocessor.</param>
    /// <returns>
    /// <see langword="true"/> if this query preprocessor 
    /// is dependent on <paramref name="other"/>; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsDependentOn(IQueryPreprocessor other);
  }
}