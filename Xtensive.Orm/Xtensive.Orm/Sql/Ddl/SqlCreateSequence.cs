// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlCreateSequence : SqlStatement, ISqlCompileUnit
  {
    private Sequence sequence;

    public Sequence Sequence {
      get {
        return sequence;
      }
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlCreateSequence clone = new SqlCreateSequence(sequence);
      context.NodeMapping[this] = clone;

      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlCreateSequence(Sequence sequence) : base(SqlNodeType.Create)
    {
      this.sequence = sequence;
    }
  }
}
