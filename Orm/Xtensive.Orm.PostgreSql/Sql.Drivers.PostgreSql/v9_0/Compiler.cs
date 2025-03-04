// Copyright (C) 2012-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.06.06

using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Drivers.PostgreSql.v9_0
{
  internal class Compiler : v8_4.Compiler
  {
    // Constructors

    protected override void VisitIntervalToMilliseconds(SqlFunctionCall node)
    {
      AppendSpaceIfNecessary();
      _ = context.Output.Append("(TRUNC(EXTRACT(EPOCH FROM (");
      node.Arguments[0].AcceptVisitor(this);
      _ = context.Output.Append(")) * 1000))");
      
    }

    public Compiler(PostgreSql.Driver driver)
      : base(driver)
    {
    } 
  }
}