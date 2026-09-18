// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dml
{
  public class SqlDeclareCursor : SqlStatement, ISqlCompileUnit
  {
    public SqlCursor Cursor { get; }

    internal override SqlDeclareCursor Clone(SqlNodeCloneContext context) => throw new NotImplementedException();

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlDeclareCursor(SqlCursor cursor)
      : base(SqlNodeType.DeclareCursor)
    {
      Cursor = cursor;
    }
  }
}
