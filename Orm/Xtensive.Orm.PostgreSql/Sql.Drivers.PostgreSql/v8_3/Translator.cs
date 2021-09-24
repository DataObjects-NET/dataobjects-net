// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Text;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using System.Linq;
using Xtensive.Reflection.PostgreSql;

namespace Xtensive.Sql.Drivers.PostgreSql.v8_3
{
  internal class Translator : v8_2.Translator
  {
    public override void Translate(SqlCompilerContext context, SqlCreateIndex node, CreateIndexSection section)
    {
      var index = node.Index;
      if (!index.IsFullText) {
        base.Translate(context, node, section);
        return;
      }

      var output = context.Output;
      switch (section) {
        case CreateIndexSection.Entry:
          output.Append("CREATE INDEX ");
          TranslateIdentifier(output, index.Name);
          output.Append(" ON ");
          Translate(context, index.DataTable);
          output.Append(" USING gin (");
          break;
        case CreateIndexSection.ColumnsExit:
          // Add actual columns list
          output.Append(GetFulltextVector(context, (FullTextIndex) node.Index));
          base.Translate(context, node, section);
          break;
        default:
          base.Translate(context, node, section);
          break;
      }
    }

    public override void Translate(SqlCompilerContext context, SqlOrder node, NodeSection section)
    {
      if (section == NodeSection.Exit) {
        context.Output.Append(node.Ascending ? "ASC NULLS FIRST" : "DESC NULLS LAST");
      }
    }

    internal protected string GetFulltextVector(SqlCompilerContext context, FullTextIndex index)
    {
      var sb = new StringBuilder("(");
      var languageGroups = index
        .Columns
        .SelectMany(column => column.Languages, (column, language) => new { column, language })
        .GroupBy(pair => pair.language, pair => pair.column);

      var isFirstOuter = true;
      foreach(var languageGroup in languageGroups) {
        if (!isFirstOuter) {
          _ = sb.Append(" || ");
        }
        isFirstOuter = false;

        var columns = languageGroup.ToList();
        var isFirstInner = true;
        _= sb.Append("to_tsvector('")
          .Append(languageGroup.Key.Name)
          .Append("'::regconfig, ");
        foreach (var language in languageGroup) {
          if(!isFirstInner) {
            _ = sb.Append(" || ' '::text");
          }
          isFirstInner = false;
          _ = sb.AppendFormat("({0})::text", QuoteIdentifier(language.Name));
        }
        _ = sb.Append(")");
      }
      return sb.Append(")").ToString();
    }

    protected override string TranslateClrType(Type type) =>
      type == WellKnownTypes.GuidType ? "uuid" : base.TranslateClrType(type);

    // Constuctors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}