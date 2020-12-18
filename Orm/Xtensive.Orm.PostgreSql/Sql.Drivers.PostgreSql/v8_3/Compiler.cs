// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Xtensive.Sql.Drivers.PostgreSql.Resources;

namespace Xtensive.Sql.Drivers.PostgreSql.v8_3
{
  internal class Compiler : v8_2.Compiler
  {
    public override void Visit(SqlCreateIndex node, IndexColumn item)
    {
      if (!node.Index.IsFullText) {
        base.Visit(node, item);
      }
      // FullText builds expression instead of list of columns in Translate(SqlCompilerContext context, SqlCreateIndex node, CreateIndexSection section)
    }


    public override void Visit(SqlFreeTextTable node)
    {
      if (node.TargetColumns.Count != 1 || node.TargetColumns[0] != node.TargetTable.Asterisk) {
        throw new NotSupportedException(Strings.ExFreeTextSearchOnCustomColumnsNotSupported);
      }

      var fullTextIndex = node.TargetTable.DataTable.Indexes.OfType<FullTextIndex>().Single();
      var alias = context.TableNameProvider.GetName(node);
      var vector = ((Translator) translator).GetFulltextVector(context, fullTextIndex);
      var tableName = translator.QuoteIdentifier(node.TargetTable.Name);
      var internalColumnIndex = 0;
      while (node.Columns["column" + internalColumnIndex] != null) {
        internalColumnIndex++;
      }
      var vectorName = translator.QuoteIdentifier("column" + internalColumnIndex);
      internalColumnIndex++;
      while (node.Columns["column" + internalColumnIndex] != null) {
        internalColumnIndex++;
      }

      var queryName = translator.QuoteIdentifier("column" + internalColumnIndex);

      context.Output.AppendText("(SELECT ");
      for (var columnIndex = 0; columnIndex < node.Columns.Count - 1; columnIndex++) {
        if (columnIndex != 0) {
          context.Output.AppendText(translator.ColumnDelimiter);
        }
        context.Output.AppendText(translator.QuoteIdentifier(node.Columns[columnIndex].Name));
      }
      context.Output.AppendText(translator.ColumnDelimiter);
      context.Output.AppendText("ts_rank_cd(");
      context.Output.AppendText(vectorName);
      context.Output.AppendText(translator.ArgumentDelimiter);
      context.Output.AppendText(queryName);
      context.Output.AppendText(") AS");
      context.Output.AppendText(translator.QuoteIdentifier(node.Columns[node.Columns.Count - 1].Name));
      context.Output.AppendText(" FROM (SELECT ");
      for (var columnIndex = 0; columnIndex < node.Columns.Count - 1; columnIndex++) {
        if (columnIndex != 0) {
          context.Output.AppendText(translator.ColumnDelimiter);
        }
        context.Output.AppendText(translator.QuoteIdentifier(node.Columns[columnIndex].Name));
      }
      context.Output.AppendText(translator.ColumnDelimiter);
      context.Output.AppendText(string.Format("{0} AS {1}", vector, vectorName));
      context.Output.AppendText(translator.ColumnDelimiter);

      var languages = fullTextIndex
        .Columns
        .SelectMany(column => column.Languages)
        .Select(language => language.Name)
        .Distinct();
      context.Output.AppendText("(");

      var isFirst = true;
      foreach(var language in languages) {
        if (!isFirst) {
          context.Output.AppendText(" || ");
        }
        isFirst = false;

        context.Output.AppendText("to_tsquery('");
        context.Output.AppendText(language);
        context.Output.AppendText("'::regconfig, ");
        context.Output.AppendText("replace(trim(regexp_replace(");
        node.FreeText.AcceptVisitor(this);
        context.Output.AppendText(@",'\\W+', ' ', 'g')),' ', '|')");
        context.Output.AppendText(")");
      }
      context.Output.AppendText(")");

      context.Output.AppendText($" AS {queryName}");
      context.Output.AppendText($" FROM {tableName}) AS {alias} WHERE {vectorName} @@ {queryName})");
    }

    // Constructors

    public Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}