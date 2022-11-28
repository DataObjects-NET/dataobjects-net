// Copyright (C) 2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2022.02.03

using System;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Drivers.MySql.v5_7
{
  internal class Translator : v5_6.Translator
  {
    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlCast node, NodeSection section)
    {
      if (node.Type.Type == SqlType.DateTime) {
        var output = context.Output;
        _ = section switch {
          NodeSection.Entry => output.AppendOpeningPunctuation("CAST("),
          NodeSection.Exit => output.Append("AS ")
            .Append(Translate(node.Type))
            .AppendClosingPunctuation("(6))"),
          _ => throw new ArgumentOutOfRangeException(nameof(section)),
        };
      }
      base.Translate(context, node, section);
    }

    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}