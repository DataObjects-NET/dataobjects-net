// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.04.02

using System;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Drivers.SqlServer.v11
{
  internal class Translator : v10.Translator
  {
    public override string Translate(SqlCompilerContext context, SqlDropSequence node)
    {
      return "DROP SEQUENCE " + Translate(context, node.Sequence);
    }

    public override string Translate(SqlCompilerContext context, SequenceDescriptor descriptor, SequenceDescriptorSection section)
    {
      if (descriptor.Owner is Sequence)
        return TranslateSequenceDescriptorDefault(context, descriptor, section);
      if (descriptor.Owner is TableColumn)
        return TranslateIdentityDescriptor(context, descriptor, section);
      throw new NotSupportedException();
    }

    public override string Translate(SqlCompilerContext context, SqlSelect node, SelectSection section)
    {
      switch (section) {
        case SelectSection.Limit:
          return "FETCH NEXT";
        case SelectSection.LimitEnd:
          return "ROWS ONLY";
        case SelectSection.Offset:
          return "OFFSET";
        case SelectSection.OffsetEnd:
          return "ROWS";
        default:
          return base.Translate(context, node, section);
      }
    }

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}