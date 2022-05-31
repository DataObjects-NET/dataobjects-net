// Copyright (C) 2009-2022 Xtensive LLC.
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
    /// <inheritdoc/>
    public override void Visit(SqlCreateIndex node)
    {
      if (!node.Index.IsFullText) {
        base.Visit(node);
        return;
      }

      AppendTranslatedEntry(node);
      if (node.Index.Columns.Count > 0) {
        AppendSpaceIfNecessary();
        //columns declaration is done in translator
        translator.Translate(context, node, CreateIndexSection.ColumnsEnter);
        translator.Translate(context, node, CreateIndexSection.ColumnsExit);
        AppendSpaceIfNecessary();
      }

      AppendTranslated(node, CreateIndexSection.StorageOptions);

      AppendTranslatedExit(node);
    }

    /// <inheritdoc/>
    public override void Visit(SqlCreateIndex node, IndexColumn item)
    {
      if (!node.Index.IsFullText) {
        base.Visit(node, item);
      }
      // FullText builds expression instead of list of columns in Translate(SqlCompilerContext context, SqlCreateIndex node, CreateIndexSection section)
    }

    /// <inheritdoc/>
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

      var output = context.Output;
      _ = output.Append("(SELECT ");
      for (var columnIndex = 0; columnIndex < node.Columns.Count - 1; columnIndex++) {
        if (columnIndex != 0) {
          _ = output.Append(translator.ColumnDelimiter);
        }
        translator.TranslateIdentifier(context.Output, node.Columns[columnIndex].Name);
      }
      _ = output.Append(translator.ColumnDelimiter)
        .Append("ts_rank_cd(")
        .Append(vectorName)
        .Append(translator.ArgumentDelimiter)
        .Append(queryName)
        .Append(") AS ");
      translator.TranslateIdentifier(output, node.Columns[node.Columns.Count - 1].Name);
      _ = output.Append(" FROM (SELECT ");
      for (var columnIndex = 0; columnIndex < node.Columns.Count - 1; columnIndex++) {
        if (columnIndex != 0) {
          _ = context.Output.Append(translator.ColumnDelimiter);
        }
        translator.TranslateIdentifier(context.Output, node.Columns[columnIndex].Name);
      }
      _ = output.Append(translator.ColumnDelimiter)
        .Append($"{vector} AS {vectorName}")
        .Append(translator.ColumnDelimiter);

      var languages = fullTextIndex
        .Columns
        .SelectMany(column => column.Languages)
        .Select(language => language.Name)
        .Distinct();
      _ = output.Append("(");

      var isFirst = true;
      foreach(var language in languages) {
        if (!isFirst) {
          _ = context.Output.Append(" || ");
        }
        isFirst = false;

        _ = output.Append("to_tsquery('")
          .Append(language)
          .Append("'::regconfig, replace(trim(regexp_replace(");
        node.FreeText.AcceptVisitor(this);
        _ = output.Append(@",'\\W+', ' ', 'g')),' ', '|'))");
      }
      _ = context.Output.Append($") AS {queryName} FROM {tableName}) AS {alias} WHERE {vectorName} @@ {queryName})");
    }

    // Constructors

    public Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}