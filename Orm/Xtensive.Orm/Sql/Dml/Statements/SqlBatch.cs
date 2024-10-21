// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlBatch: SqlStatement,
    IList<SqlStatement>,
    ISqlCompileUnit
  {
    private readonly IList<SqlStatement> statements = new Collection<SqlStatement>();

    #region IList<SqlStatement> Members

    /// <inheritdoc/>
    public void Add(SqlStatement item)
    {
      statements.Add(item);
    }

    public void AddRange(IEnumerable<SqlStatement> items)
    {
      foreach (SqlStatement item in items)
        Add(item);
    }

    /// <inheritdoc/>
    public void Clear()
    {
      statements.Clear();
    }

    /// <inheritdoc/>
    public bool Contains(SqlStatement item)
    {
      return statements.Contains(item);
    }

    /// <inheritdoc/>
    public void CopyTo(SqlStatement[] array, int arrayIndex)
    {
      statements.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc/>
    public bool Remove(SqlStatement item)
    {
      return statements.Remove(item);
    }

    /// <inheritdoc/>
    public int Count => statements.Count;

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    public int IndexOf(SqlStatement item)
    {
      return statements.IndexOf(item);
    }

    /// <inheritdoc/>
    public void Insert(int index, SqlStatement item)
    {
      statements.Insert(index, item);
    }

    /// <inheritdoc/>
    public void RemoveAt(int index)
    {
      statements.RemoveAt(index);
    }

    /// <inheritdoc/>
    public SqlStatement this[int index]
    {
      get => statements[index];
      set => statements[index] = value;
    }

    /// <inheritdoc/>
    IEnumerator<SqlStatement> IEnumerable<SqlStatement>.GetEnumerator()
    {
      return statements.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return statements.GetEnumerator();
    }

    #endregion

    internal override SqlBatch Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) => {
        var clone = new SqlBatch();
        foreach (SqlStatement s in t.statements)
          clone.Add((SqlStatement) s.Clone(c));
        return clone;
      });

    /// <inheritdoc/>
    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }


    // Constructors

    internal SqlBatch()
      : base(SqlNodeType.Batch)
    {
    }
  }
}