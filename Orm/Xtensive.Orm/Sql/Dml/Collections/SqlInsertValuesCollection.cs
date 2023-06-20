// Copyright (C) 2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;

namespace Xtensive.Sql.Dml.Collections
{
  /// <summary>
  /// Collection of values of <see cref="SqlInsert"/>.
  /// </summary>
  public sealed class SqlInsertValuesCollection : IReadOnlyList<SqlRow>
  {
    private IReadOnlyList<SqlColumn> columns;
    private List<SqlRow> rows = new();

    /// <summary>
    /// The columns collection has values for.
    /// </summary>
    public IReadOnlyList<SqlColumn> Columns => columns ?? Array.Empty<SqlColumn>();

    /// <summary>
    /// Count of rows.
    /// </summary>
    public int Count => rows.Count;

    /// <summary>
    /// Gets row by index.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public SqlRow this[int index] => rows[index];

    /// <summary>
    /// Adds column-to-value mapped collection of values as row.
    /// </summary>
    /// <param name="row">column-to-value mapped collection of values</param>
    /// <exception cref="ArgumentNullException"><paramref name="row"/> value is null.</exception>
    /// <exception cref="ArgumentException">Count of values between already added rows and <paramref name="row"/>
    /// -or- particular column in <paramref name="row"/> is not presented in already added rows
    /// -or- <paramref name="row"/> is empty.</exception>
    public void Add(Dictionary<SqlColumn, SqlExpression> row)
    {
#if NET6_0_OR_GREATER
      ArgumentNullException.ThrowIfNull(row, nameof(row));
#else
      ArgumentValidator.EnsureArgumentNotNull(row, nameof(row));
#endif
      if (row.Count == 0) {
        throw new ArgumentException("Empty row is not allowed.");
      }

      if (rows.Count == 0) {
        // save columns order as header for further rows to match;
        columns = row.Keys.ToList();
        rows.Add(SqlDml.Row(row.Values.ToList()));
      }
      else {
        if (columns.Count != row.Count)
          throw new ArgumentException("Inconsistent row length.");
        if (row.Keys.SequenceEqual(columns)) {
          //fast addition
          rows.Add(SqlDml.Row(row.Values.ToList()));
        }
        else {
          //re-arrange values to be the same order
          //and also make sure all columns exist
          var rowList = new List<SqlExpression>();
          foreach (var column in columns) {
            if (row.TryGetValue(column, out var value)) {
              rowList.Add(value);
            }
            else {
              throw new ArgumentException(string.Format("There is no mentioning of column '{0}' in previously added rows.", column.Name));
            }
          }

          rows.Add(SqlDml.Row(rowList));
        }
      }
    }

    /// <summary>
    /// Removes row by index.
    /// </summary>
    /// <param name="index">The index of row to remove.</param>
    public void RemoveAt(int index)
    {
      rows.RemoveAt(index);
      if (rows.Count == 0) {
        columns = null;
      }
    }

    /// <summary>
    /// Clears rows and columns.
    /// </summary>
    public void Clear()
    {
      rows = new List<SqlRow>();
      columns = null;
    }

    /// <inheritdoc/>
    public IEnumerator<SqlRow> GetEnumerator() => rows.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    internal SqlInsertValuesCollection Clone(SqlNodeCloneContext context)
    {
      var clone = new SqlInsertValuesCollection();

      if (rows.Count == 0) {
        return clone;
      }

      var clonedList = new List<SqlColumn>(columns.Count);
      foreach (var oldColumn in columns) {
        clonedList.Add((SqlColumn) context.NodeMapping[oldColumn]);
      }
      clone.columns = clonedList;

      clone.rows = new List<SqlRow>(rows.Count);
      foreach(var oldRow in rows) {
        clone.rows.Add((SqlRow) oldRow.Clone());
      }

      return clone;
    }
  }
}
