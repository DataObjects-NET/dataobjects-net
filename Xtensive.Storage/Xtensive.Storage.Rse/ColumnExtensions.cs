// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.12

using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// <see cref="Column"/> related extension methods.
  /// </summary>
  public static class ColumnExtensions
  {
    /// <summary>
    /// Aliases the specified <see cref="Column"/> collection.
    /// </summary>
    /// <param name="source">The source collection.</param>
    /// <param name="alias">The alias to add.</param>
    /// <returns>Aliased sequence of columns.</returns>
    public static IEnumerable<Column> Alias(this IEnumerable<Column> source, string alias)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      foreach (var column in source)
        yield return new Column(column, alias);
    }

    /// <summary>
    /// Aliases the specified <see cref="Column"/> array.
    /// </summary>
    /// <param name="source">The source array.</param>
    /// <param name="alias">The alias to add.</param>
    /// <returns>An array with aliased columns.</returns>
    public static Column[] Alias(this Column[] source, string alias)
    {
      var result = new Column[source.Length];
      for (int i = 0; i < source.Length; i++)
        result[i] = new Column(source[i], alias);
      return result;
    }
  }
}