// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlAlterTable : SqlStatement, ISqlCompileUnit
  {
    private readonly SqlAction action;
    private readonly Table table;

    public SqlAction Action => action;

    public Table Table => table;


    internal override SqlAlterTable Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlAlterTable(t.table, (SqlAction)t.action.Clone(c)));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlAlterTable(Table table, SqlAction action)
      : base(SqlNodeType.Alter)
    {
      this.action = action;
      this.table = table;
    }
  }
}
