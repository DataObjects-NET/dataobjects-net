// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  public class SqlAlterDomain : SqlStatement, ISqlCompileUnit
  {
    public SqlAction Action { get; }

    public Domain Domain { get; }

    internal override SqlAlterDomain Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlAlterDomain(t.Domain, (SqlAction)t.Action.Clone(c)));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlAlterDomain(Domain domain, SqlAction action)
      : base(SqlNodeType.Alter)
    {
      Action = action;
      Domain = domain;
    }
  }
}
