// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dml
{
  public class SqlCloseCursor : SqlStatement, ISqlCompileUnit
  {
    /// <summary>
    /// Gets the cursor.
    /// </summary>
    /// <value>The cursor.</value>
    public SqlCursor Cursor { get; }

    internal override SqlCloseCursor Clone(SqlNodeCloneContext context) => throw new NotImplementedException();

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    public SqlCloseCursor(SqlCursor cursor)
      : base(SqlNodeType.CloseCursor)
    {
      Cursor = cursor;
    }
  }
}
