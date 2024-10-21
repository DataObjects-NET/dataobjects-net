// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.08.10

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlRenameTable : SqlStatement, ISqlCompileUnit
  {
    public Table Table { get; private set; }
    public string NewName { get; private set; }

    internal override SqlRenameTable Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlRenameTable(t.Table, t.NewName));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    // Constructors

    internal SqlRenameTable(Table table, string newName)
      : base(SqlNodeType.Rename)
    {
      Table = table;
      NewName = newName;
    }
  }
}