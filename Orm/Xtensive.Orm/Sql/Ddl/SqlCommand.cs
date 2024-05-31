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

    internal override object Clone(SqlNodeCloneContext context) =>
      context.NodeMapping.TryGetValue(this, out var clone)
        ? clone
        : context.NodeMapping[this] = new SqlCommand(CommandType);

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