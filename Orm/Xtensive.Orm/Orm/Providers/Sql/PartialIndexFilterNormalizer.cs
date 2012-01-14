// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.10.13

namespace Xtensive.Orm.Providers.Sql
{
  /// <summary>
  /// Normalizer of partial index filter expressions.
  /// Normalization of such expressions are required to compare filter extracted from DBMS
  /// with expression generated inside domain model.
  /// </summary>
  public class PartialIndexFilterNormalizer : HandlerBase
  {
    /// <summary>
    /// Normalizes <paramref name="expression"/>.
    /// </summary>
    /// <param name="expression">Expression to normalize.</param>
    /// <returns>Result of normalization.</returns>
    public virtual string Normalize(string expression)
    {
      return expression;
    }
  }
}
