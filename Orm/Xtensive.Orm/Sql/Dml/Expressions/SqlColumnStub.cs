// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.09.08

using System;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlColumnStub : SqlColumn
  {
    public SqlColumn Column { get; set; }

    internal override SqlColumnStub Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlColumnStub(
            t.SqlTable?.Clone(c), 
            t.Column));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {}

    // Constructors

    internal SqlColumnStub(SqlColumn column)
      : base(column.Name ?? string.Empty)
    {
      Column = column;
    }

    private SqlColumnStub(SqlTable sqlTable, SqlColumn column)
      : base(sqlTable, column.Name ?? string.Empty)
    {
      Column = column;
    }
  }
}