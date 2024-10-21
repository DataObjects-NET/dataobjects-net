// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlDropCollation : SqlStatement, ISqlCompileUnit
  {
    public Collation Collation { get; }

    internal override SqlDropCollation Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlDropCollation(t.Collation));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlDropCollation(Collation collation) : base(SqlNodeType.Drop)
    {
      Collation = collation;
    }
  }
}
