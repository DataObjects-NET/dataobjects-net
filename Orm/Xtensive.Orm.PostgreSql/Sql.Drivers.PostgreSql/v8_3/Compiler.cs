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

      context.Output.Append("(SELECT ");
      for (var columnIndex = 0; columnIndex < node.Columns.Count - 1; columnIndex++) {
        if (columnIndex != 0) {
          context.Output.Append(translator.ColumnDelimiter);
        }
        translator.TranslateIdentifier(context.Output, node.Columns[columnIndex].Name);
      }
      context.Output.Append(translator.ColumnDelimiter);
      context.Output.Append("ts_rank_cd(");
      context.Output.Append(vectorName);
      context.Output.Append(translator.ArgumentDelimiter);
      context.Output.Append(queryName);
      context.Output.Append(") AS");
      context.Output.Append(translator.QuoteIdentifier(node.Columns[node.Columns.Count - 1].Name));
      context.Output.Append(" FROM (SELECT ");
      for (var columnIndex = 0; columnIndex < node.Columns.Count - 1; columnIndex++) {
        if (columnIndex != 0) {
          context.Output.Append(translator.ColumnDelimiter);
        }
        context.Output.Append(translator.QuoteIdentifier(node.Columns[columnIndex].Name));
      }
      context.Output.Append(translator.ColumnDelimiter);
      context.Output.Append($"{vector} AS {vectorName}");
      context.Output.Append(translator.ColumnDelimiter);

      var languages = fullTextIndex
        .Columns
        .SelectMany(column => column.Languages)
        .Select(language => language.Name)
        .Distinct();
      context.Output.Append("(");

      var isFirst = true;
      foreach(var language in languages) {
        if (!isFirst) {
          context.Output.Append(" || ");
        }
        isFirst = false;

        context.Output.Append("to_tsquery('");
        context.Output.Append(language);
        context.Output.Append("'::regconfig, ");
        context.Output.Append("replace(trim(regexp_replace(");
        node.FreeText.AcceptVisitor(this);
        context.Output.Append(@",'\\W+', ' ', 'g')),' ', '|')");
        context.Output.Append(")");
      }
      context.Output.Append(")");

      context.Output.Append($" AS {queryName}");
      context.Output.Append($" FROM {tableName}) AS {alias} WHERE {vectorName} @@ {queryName})");
    }

    // Constructors

    public Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}