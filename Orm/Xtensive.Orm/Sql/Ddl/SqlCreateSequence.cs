// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlCreateSequence : SqlStatement, ISqlCompileUnit
  {
    public Sequence Sequence { get; }

    internal override SqlCreateSequence Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlCreateSequence(t.Sequence));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlCreateSequence(Sequence sequence) : base(SqlNodeType.Create)
    {
      Sequence = sequence;
    }
  }
}
