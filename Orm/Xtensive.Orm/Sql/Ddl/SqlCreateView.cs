// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlCreateView : SqlStatement, ISqlCompileUnit
  {
    private View node;

    public View View {
      get {
        return node;
      }
    }

    internal override SqlCreateView Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlCreateView(t.node));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlCreateView(View node) : base(SqlNodeType.Create)
    {
      this.node = node;
    }
  }
}
