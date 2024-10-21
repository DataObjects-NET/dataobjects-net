// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlCreateDomain : SqlStatement, ISqlCompileUnit
  {
    public Domain Domain { get; }

    internal override SqlCreateDomain Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlCreateDomain(t.Domain));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlCreateDomain(Domain domain) : base(SqlNodeType.Create)
    {
      Domain = domain;
    }
  }
}
