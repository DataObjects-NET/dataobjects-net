// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlCreateIndex : SqlStatement, ISqlCompileUnit
  {
    private Index index;

    public Index Index {
      get {
        return index;
      }
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlCreateIndex clone = new SqlCreateIndex(index);
      context.NodeMapping[this] = clone;

      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlCreateIndex(Index index)
      : base(SqlNodeType.Create)
    {
      this.index = index;
    }
  }
}
