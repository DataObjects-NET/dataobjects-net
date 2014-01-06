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
    public override string Translate(SqlCompilerContext context, SqlCast node, NodeSection section)
    {
      switch (node.Type.Type) {
      case SqlType.DateTime:
        switch (section) {
        case NodeSection.Entry:
          return "CAST(";
        case NodeSection.Exit:
          return "AS " + Translate(node.Type) + "(6))";
        default:
          throw new ArgumentOutOfRangeException("section");
        }
      }
      return base.Translate(context, node, section);
    }

    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}