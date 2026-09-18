// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Xtensive.Sql.Dml
{
  public class SqlFetch: SqlStatement,
    ISqlCompileUnit
  {

    /// <summary>
    /// Gets the cursor.
    /// </summary>
    /// <value>The cursor.</value>
    public SqlCursor Cursor { get; }

    /// <summary>
    /// Gets the fetch option.
    /// </summary>
    /// <value>The fetch option.</value>
    public SqlFetchOption Option { get; } = SqlFetchOption.Next;

    /// <summary>
    /// Gets the row count.
    /// </summary>
    /// <value>The row count.</value>
    public SqlExpression RowCount { get; }

    /// <summary>
    /// Gets the targets.
    /// </summary>
    /// <value>The targets.</value>
    public IList<ISqlCursorFetchTarget> Targets { get; } = new Collection<ISqlCursorFetchTarget>();

    internal override SqlFetch Clone(SqlNodeCloneContext context) => throw new NotImplementedException();

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlFetch(SqlFetchOption option, SqlExpression rowCount, SqlCursor cursor,
                    params ISqlCursorFetchTarget[] targets)
      : base(SqlNodeType.Fetch)
    {
      Option = option;
      if (targets != null)
        for (int i = 0, l = targets.Length; i < l; i++)
          Targets.Add(targets[i]);
      Cursor = cursor;
      RowCount = rowCount;
    }
  }
}