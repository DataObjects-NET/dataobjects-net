// Copyright (C) 2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlTruncateTable : SqlStatement, ISqlCompileUnit
  {
    public Table Table { get; }

    internal override SqlTruncateTable Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlTruncateTable(t.Table));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlTruncateTable(Table table) : base(SqlNodeType.Truncate)
    {
      Table = table;
    }
  }
}
