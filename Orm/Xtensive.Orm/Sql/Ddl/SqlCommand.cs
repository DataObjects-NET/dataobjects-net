// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.08.04

using System;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlCommand : SqlStatement, ISqlCompileUnit
  {
    public SqlCommandType CommandType { get; private set; }

    internal override SqlCommand Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlCommand(t.CommandType));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    // Constructors

    internal SqlCommand(SqlCommandType commandType)
      : base(SqlNodeType.Command)
    {
      CommandType = commandType;
    }
  }
}