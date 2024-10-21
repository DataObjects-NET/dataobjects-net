// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlDropSequence : SqlStatement, ISqlCompileUnit
  {
    private bool cascade = true;

    public Sequence Sequence { get; }

    public bool Cascade {
      get {
        return cascade;
      }
      set {
        cascade = value;
      }
    }

    internal override SqlDropSequence Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlDropSequence(t.Sequence));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlDropSequence(Sequence sequence)
      : base(SqlNodeType.Drop)
    {
      Sequence = sequence;
    }

    internal SqlDropSequence(Sequence sequence, bool cascade)
      : base(SqlNodeType.Drop)
    {
      Sequence = sequence;
      this.cascade = cascade;
    }
  }
}
