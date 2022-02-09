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
    public override void Translate(SqlCompilerContext context, SqlDropSequence node)
    {
      TranslateSequenceStatement(context, node.Sequence, "DROP");
    }

    public override void Translate(SqlCompilerContext context, SqlAlterSequence node, NodeSection section)
    {
      if (section == NodeSection.Entry) {
        TranslateSequenceStatement(context, node.Sequence, "ALTER");
      }
    }

    public override void Translate(SqlCompilerContext context, SqlCreateSequence node, NodeSection section)
    {
      if (section == NodeSection.Entry) {
        TranslateSequenceStatement(context, node.Sequence, "CREATE");
      }
    }

    public override void Translate(SqlCompilerContext context, SequenceDescriptor descriptor, SequenceDescriptorSection section)
    {
      switch (descriptor.Owner) {
        case Sequence _:
          TranslateSequenceDescriptorDefault(context, descriptor, section);
          break;
        case TableColumn _:
          TranslateIdentityDescriptor(context, descriptor, section);
          break;
        default:
          throw new NotSupportedException();
      }
    }

    public override void Translate(SqlCompilerContext context, SqlSelect node, SelectSection section)
    {
      var output = context.Output;
      switch (section) {
        case SelectSection.Limit:
          output.Append("FETCH NEXT");
          break;
        case SelectSection.LimitEnd:
          output.Append("ROWS ONLY");
          break;
        case SelectSection.Offset:
          output.Append("OFFSET");
          break;
        case SelectSection.OffsetEnd:
          output.Append("ROWS");
          break;
        default:
          base.Translate(context, node, section);
          break;
      }
    }

    private void TranslateSequenceStatement(SqlCompilerContext context, Sequence sequence, string action)
    {
      // SQL Server does not support database qualification in create/drop/alter sequence.
      // We add explicit "use" statement before such statements.
      // This changes current database as side effect,
      // but it's OK because we always use database qualified objects in all other statements.

      AddUseStatement(context, sequence.Schema.Catalog);
      context.Output.Append(action)
        .Append(" SEQUENCE ");
      TranslateIdentifier(context.Output, sequence.Schema.Name, sequence.Name);
    }

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}