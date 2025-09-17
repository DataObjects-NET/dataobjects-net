// Copyright (C) 2013-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alena Mikshina
// Created:    2013.12.30

using System;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Drivers.MySql.v5_6
{
  internal class Translator : v5_5.Translator
  {
    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlCast node, NodeSection section)
    {
      if (node.Type.Type == SqlType.DateTime) {
        switch (section) {
          case NodeSection.Entry:
            _ = context.Output.AppendOpeningPunctuation("CAST(");
            break;
          case NodeSection.Exit:
            _ = context.Output.Append("AS ").Append(Translate(node.Type)).AppendClosingPunctuation(")");
            break;
          default:
            throw new ArgumentOutOfRangeException(nameof(section));
        }
      }
      else {
        base.Translate(context, node, section);
      }
    }

    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}