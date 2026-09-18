// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  public class SqlDropDomain : SqlStatement, ISqlCompileUnit
  {
    public Domain Domain { get; }

    public bool Cascade { get; set; } = true;

    internal override SqlDropDomain Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlDropDomain(t.Domain));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlDropDomain(Domain domain) : base(SqlNodeType.Drop)
    {
      Domain = domain;
    }

    internal SqlDropDomain(Domain domain, bool cascade) : base(SqlNodeType.Drop)
    {
      Domain = domain;
      Cascade = cascade;
    }
  }
}
