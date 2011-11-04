// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlOpenCursor : SqlStatement, ISqlCompileUnit
  {
    private SqlCursor cursor;

    /// <summary>
    /// Gets the cursor.
    /// </summary>
    /// <value>The cursor.</value>
    public SqlCursor Cursor {
      get {
        return cursor;
      }
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      throw new NotImplementedException();
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    public SqlOpenCursor(SqlCursor cursor) : base(SqlNodeType.OpenCursor)
    {
      this.cursor = cursor;
    }
  }
}
