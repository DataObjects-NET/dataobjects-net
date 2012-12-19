// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Represents collection of <see cref="SqlColumn"/>s.
  /// </summary>
  [Serializable]
  public class SqlTableColumnCollection
    : ReadOnlyCollection<SqlTableColumn>
  {
    private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

    /// <summary>
    /// An indexer that provides access to collection items by their names.
    /// Returns <see langword="null"/> if there is no such item.
    /// </summary>
    public SqlTableColumn this[string name]
    {
      get
      {
        if (string.IsNullOrEmpty(name))
          return null;
        foreach (SqlTableColumn column in this)
          if (Comparer.Equals(column.Name, name))
            return column;
        return null;
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlTableColumnCollection"/> class.
    /// </summary>
    /// <param name="list">The list to wrap.</param>
    /// <exception cref="T:System.ArgumentNullException">list is null.</exception>
    public SqlTableColumnCollection(IList<SqlTableColumn> list)
      : base(list)
    {
    }
  }
}