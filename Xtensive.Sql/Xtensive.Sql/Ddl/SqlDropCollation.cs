// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlDropCollation : SqlStatement, ISqlCompileUnit
  {
    private Collation collation;

    public Collation Collation {
      get {
        return collation;
      }
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlDropCollation clone = new SqlDropCollation(collation);
      context.NodeMapping[this] = clone;

      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlDropCollation(Collation collation) : base(SqlNodeType.Drop)
    {
      this.collation = collation;
    }
  }
}
