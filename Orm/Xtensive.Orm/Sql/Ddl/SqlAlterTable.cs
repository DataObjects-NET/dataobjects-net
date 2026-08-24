// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  public class SqlAlterTable : SqlStatement, ISqlCompileUnit
  {
    public SqlAction Action { get; }

    public Table Table { get; }


    internal override SqlAlterTable Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlAlterTable(t.Table, (SqlAction)t.Action.Clone(c)));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlAlterTable(Table table, SqlAction action)
      : base(SqlNodeType.Alter)
    {
      Action = action;
      Table = table;
    }
  }
}
