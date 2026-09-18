// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  public class SqlAlterSequence : SqlStatement, ISqlCompileUnit
  {
    public Sequence Sequence { get; }

    public SequenceDescriptor SequenceDescriptor { get; }

    public SqlAlterIdentityInfoOptions InfoOption { get; }

    internal override SqlAlterSequence Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlAlterSequence(t.Sequence, (SequenceDescriptor)t.SequenceDescriptor.Clone(), t.InfoOption));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlAlterSequence(Sequence sequence, SequenceDescriptor sequenceDescriptor, SqlAlterIdentityInfoOptions infoOption)
      : base(SqlNodeType.Alter)
    {
      Sequence = sequence;
      SequenceDescriptor = sequenceDescriptor;
      InfoOption = infoOption;
    }
  }
}
