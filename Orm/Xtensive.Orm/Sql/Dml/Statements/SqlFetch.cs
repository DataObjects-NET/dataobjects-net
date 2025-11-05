// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlFetch: SqlStatement,
    ISqlCompileUnit
  {
    private readonly SqlCursor cursor;
    private readonly SqlFetchOption option = SqlFetchOption.Next;
    private readonly SqlExpression rowCount;
    private readonly IList<ISqlCursorFetchTarget> targets = new Collection<ISqlCursorFetchTarget>();

    /// <summary>
    /// Gets the cursor.
    /// </summary>
    /// <value>The cursor.</value>
    public SqlCursor Cursor
    {
      get { return cursor; }
    }

    /// <summary>
    /// Gets the fetch option.
    /// </summary>
    /// <value>The fetch option.</value>
    public SqlFetchOption Option
    {
      get { return option; }
    }

    /// <summary>
    /// Gets the row count.
    /// </summary>
    /// <value>The row count.</value>
    public SqlExpression RowCount
    {
      get { return rowCount; }
    }

    /// <summary>
    /// Gets the targets.
    /// </summary>
    /// <value>The targets.</value>
    public IList<ISqlCursorFetchTarget> Targets
    {
      get { return targets; }
    }

    internal override SqlFetch Clone(SqlNodeCloneContext context) => throw new NotImplementedException();

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlFetch(SqlFetchOption option, SqlExpression rowCount, SqlCursor cursor,
                    params ISqlCursorFetchTarget[] targets)
      : base(SqlNodeType.Fetch)
    {
      this.option = option;
      if (targets != null)
        for (int i = 0, l = targets.Length; i < l; i++)
          this.targets.Add(targets[i]);
      this.cursor = cursor;
      this.rowCount = rowCount;
    }
  }
}