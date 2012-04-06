// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.04.02

using System;
using System.Text;
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
      return TranslateSequenceStatement(context, node.Sequence, "DROP");
    }

    public override string Translate(SqlCompilerContext context, SqlAlterSequence node, NodeSection section)
    {
      if (section==NodeSection.Entry)
        return TranslateSequenceStatement(context, node.Sequence, "ALTER");
      return string.Empty;
    }

    public override string Translate(SqlCompilerContext context, SqlCreateSequence node, NodeSection section)
    {
      if (section==NodeSection.Entry)
        return TranslateSequenceStatement(context, node.Sequence, "CREATE");
      return string.Empty;
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

    private string TranslateSequenceStatement(SqlCompilerContext context, Sequence sequence, string action)
    {
      // SQL Server does not support database qualification in create/drop/alter sequence.
      // We add explicit "use" statement before such statements.
      // This changes current database as side effect,
      // but it's OK because we always use database qualified objects in all other statements.

      var result = new StringBuilder();
      if (context.HasOptions(SqlCompilerNamingOptions.DatabaseQualifiedObjects))
        result.AppendFormat("USE {0}; ", QuoteIdentifier(sequence.Schema.Catalog.Name));
      result.AppendFormat("{0} SEQUENCE {1}", action, QuoteIdentifier(sequence.Schema.Name, sequence.Name));
      return result.ToString();
    }

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}