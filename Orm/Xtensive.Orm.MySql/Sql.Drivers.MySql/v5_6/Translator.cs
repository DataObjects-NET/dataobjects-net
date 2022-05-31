// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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
            _ = context.Output.Append("AS ").Append(Translate(node.Type)).Append("(6))");
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