// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Represents collection of <see cref="SqlColumn"/>s.
  /// </summary>
  [Serializable]
  public class SqlTableColumnCollection : IReadOnlyList<SqlTableColumn>
  {
    private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;
    private readonly List<SqlTableColumn> columnList;
    private Dictionary<string, SqlTableColumn> columnLookup;

    /// <summary>
    /// Gets the number of elements contained in the <see cref="SqlTableColumnCollection"/>.
    /// </summary>
    public int Count => columnList.Count;

    /// <inheritdoc cref="IEnumerable.GetEnumerator"/>>
    IEnumerator IEnumerable.GetEnumerator() => columnList.GetEnumerator();

    /// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>>
    IEnumerator<SqlTableColumn> IEnumerable<SqlTableColumn>.GetEnumerator() => columnList.GetEnumerator();

    /// <summary>
    /// Returns a <see cref="List{T}.Enumerator"/> that iterates through the <see cref="SqlTableColumnCollection"/>.
    /// </summary>
    public List<SqlTableColumn>.Enumerator GetEnumerator() => columnList.GetEnumerator();

    /// <summary>
    /// Gets the column at the specified <paramref name="index"/>.
    /// </summary>
    public SqlTableColumn this[int index] => columnList[index];

    /// <summary>
    /// Gets the column with the specified <paramref name="name"/>
    /// or <see langword="null"/> if collection doesn't contain such a column.
    /// </summary>
    public SqlTableColumn this[string name]
    {
      get {
        if (string.IsNullOrEmpty(name)) {
          return null;
        }

        if (columnLookup != null) {
          return columnLookup.TryGetValue(name, out var column) ? column : null;
        }

        var count = columnList.Count;
        if (count <= 16) {
          foreach (var column in columnList) {
            if (Comparer.Equals(column.Name, name)) {
              return column;
            }
          }

          return null;
        }

        SqlTableColumn result = null;
        columnLookup = new Dictionary<string, SqlTableColumn>(count, Comparer);
        for (var index = count - 1; index >= 0; index--) {
          var column = columnList[index];
          var columnName = column.Name;
          columnLookup[columnName] = column;
          if (Comparer.Equals(columnName, name)) {
            result = column;
          }
        }

        return result;
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlTableColumnCollection"/> class.
    /// </summary>
    /// <param name="columns">A collection of <see cref="SqlTableColumn"/>s to be wrapped.</param>
    public SqlTableColumnCollection(IEnumerable<SqlTableColumn> columns)
    {
      columnList = new List<SqlTableColumn>(columns);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlTableColumnCollection"/> class.
    /// This is special version it uses provided list as is.
    /// </summary>
    internal SqlTableColumnCollection(List<SqlTableColumn> columns)
    {
      columnList = columns;
    }
  }
}