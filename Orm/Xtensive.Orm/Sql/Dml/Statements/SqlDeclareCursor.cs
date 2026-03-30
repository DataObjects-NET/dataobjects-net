// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlDeclareCursor : SqlStatement, ISqlCompileUnit
  {
    private SqlCursor cursor;

    public SqlCursor Cursor {
      get {
        return cursor;
      }
    }

    internal override SqlDeclareCursor Clone(SqlNodeCloneContext context) => throw new NotImplementedException();

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlDeclareCursor(SqlCursor cursor)
      : base(SqlNodeType.DeclareCursor)
    {
      this.cursor = cursor;
    }
  }
}
