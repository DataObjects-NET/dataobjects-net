// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlDropSequence : SqlStatement, ISqlCompileUnit
  {
    private Sequence sequence;
    private bool cascade = true;

    public Sequence Sequence {
      get {
        return sequence;
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

      SqlDropSequence clone = new SqlDropSequence(sequence);
      context.NodeMapping[this] = clone;

      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlDropSequence(Sequence sequence)
      : base(SqlNodeType.Drop)
    {
      this.sequence = sequence;
    }

    internal SqlDropSequence(Sequence sequence, bool cascade)
      : base(SqlNodeType.Drop)
    {
      this.sequence = sequence;
      this.cascade = cascade;
    }
  }
}
