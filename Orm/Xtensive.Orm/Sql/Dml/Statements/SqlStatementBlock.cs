// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlStatementBlock : SqlStatement,
    IList<SqlStatement>
  {
    private IList<SqlStatement> statements = new Collection<SqlStatement>();

    #region IList<SqlStatement> Members

    /// <inheritdoc/>
    public void Add(SqlStatement item)
    {
      statements.Add(item);
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
    public int Count
    {
      get { return statements.Count; }
    }

    /// <inheritdoc/>
    public bool IsReadOnly
    {
      get { return false; }
    }

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
      get { return statements[index]; }
      set { statements[index] = value; }
    }

    /// <inheritdoc/>
    IEnumerator<SqlStatement> IEnumerable<SqlStatement>.GetEnumerator()
    {
      return statements.GetEnumerator();
    }

    /// <inheritdoc/>
    public IEnumerator GetEnumerator()
    {
      return statements.GetEnumerator();
    }

    #endregion

    internal override SqlStatementBlock Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) => {
        SqlStatementBlock clone = new SqlStatementBlock();
        foreach (SqlStatement s in t.statements)
          clone.Add((SqlStatement) s.Clone(c));
        return clone;
      });

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlStatementBlock()
      : base(SqlNodeType.BeginEndBlock)
    {
    }
  }
}
