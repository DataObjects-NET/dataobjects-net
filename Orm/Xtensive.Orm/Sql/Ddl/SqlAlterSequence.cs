// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlAlterSequence : SqlStatement, ISqlCompileUnit
  {
    private readonly Sequence sequence;
    private readonly SequenceDescriptor sequenceDescriptor;
    private SqlAlterIdentityInfoOptions infoOption = SqlAlterIdentityInfoOptions.All;

    public Sequence Sequence => sequence;

    public SequenceDescriptor SequenceDescriptor => sequenceDescriptor;

    public SqlAlterIdentityInfoOptions InfoOption
    {
      get => infoOption;
      set => infoOption = value;
    }

    /// <inheritdoc />
    internal override SqlAlterSequence Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t,c) => new SqlAlterSequence(t.sequence, t.sequenceDescriptor.Clone(), t.infoOption));

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
