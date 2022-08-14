// Copyright (C) 2009-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Drivers.Oracle.v11
{
  internal class Translator : v10.Translator
  {
    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlOrder node, NodeSection section)
    {
      if (section == NodeSection.Exit) {
        _ = context.Output.Append(node.Ascending ? "ASC NULLS FIRST" : "DESC NULLS LAST");
      }
    }

    /// <inheritdoc/>
    public override string Translate(SqlValueType type)
    {
      // we need to explicitly specify maximum interval precision
      return type.Type == SqlType.Interval
        ? "INTERVAL DAY(6) TO SECOND(6)"
        : base.Translate(type);
    }

    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}