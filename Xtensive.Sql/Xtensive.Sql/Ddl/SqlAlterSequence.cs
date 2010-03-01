// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlAlterSequence : SqlStatement, ISqlCompileUnit
  {
    private Sequence sequence;
    private SequenceDescriptor sequenceDescriptor;
    private SqlAlterIdentityInfoOptions infoOption = SqlAlterIdentityInfoOptions.All;

    public Sequence Sequence {
      get {
        return sequence;
      }
    }

    public SequenceDescriptor SequenceDescriptor {
      get {
        return sequenceDescriptor;
      }
    }

    public SqlAlterIdentityInfoOptions InfoOption {
      get {
        return infoOption;
      }
      set {
        infoOption = value;
      }
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlAlterSequence clone = new SqlAlterSequence(sequence, (SequenceDescriptor)sequenceDescriptor.Clone(), infoOption);
      context.NodeMapping[this] = clone;

      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlAlterSequence(Sequence sequence, SequenceDescriptor sequenceDescriptor, SqlAlterIdentityInfoOptions infoOption)
      : base(SqlNodeType.Alter)
    {
      this.sequence = sequence;
      this.sequenceDescriptor = sequenceDescriptor;
      this.infoOption = infoOption;
    }
  }
}
