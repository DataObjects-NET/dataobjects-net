// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlDropView : SqlStatement, ISqlCompileUnit
  {
    private View view;
    private bool cascade = true;

    public View View {
      get {
        return view;
      }
    }

    public bool Cascade {
      get {
        return cascade;
      }
      set {
        cascade = value;
      }
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlDropView clone = new SqlDropView(view);
      context.NodeMapping[this] = clone;

      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlDropView(View view) : base(SqlNodeType.Drop)
    {
      this.view = view;
    }

    internal SqlDropView(View view, bool cascade) : base(SqlNodeType.Drop)
    {
      this.view = view;
      this.cascade = cascade;
    }
  }
}
