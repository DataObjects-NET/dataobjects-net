﻿using System;
using System.Text;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using System.Linq;

namespace Xtensive.Sql.PostgreSql.v8_3
{
  internal class Translator : v8_2.Translator
  {
    public override string Translate(SqlCompilerContext context, SqlCreateIndex node, CreateIndexSection section)
    {
      var index = node.Index;
      if (!index.IsFullText)
        return base.Translate(context, node, section);
      switch (section) {
      case CreateIndexSection.Entry:
        return string.Format("CREATE INDEX {0} ON {1} USING gin ("
          , QuoteIdentifier(index.Name)
          , Translate(index.DataTable));
      case CreateIndexSection.ColumnsExit:
        // Add actual columns list
        return string.Concat(GetFulltextExpression(context, (FullTextIndex) node.Index), base.Translate(context, node, section));
      default:
        return base.Translate(context, node, section);
      }
    }

    public override string Translate(SqlCompilerContext context, SqlOrder node, NodeSection section)
    {
      switch (section) {
      case NodeSection.Exit:
        return (node.Ascending) ? "ASC NULLS FIRST" : "DESC NULLS LAST";
      }
      return string.Empty;
    }

    protected string GetFulltextExpression(SqlCompilerContext context, FullTextIndex index)
    {
      var sb = new StringBuilder();
      sb.Append("(");
      var languageGroups = index
        .Columns
        .SelectMany(column => column.Languages, (column, language) => new {column, language})
        .GroupBy(pair => pair.language, pair => pair.column)
        .ToList();
      for (int i = 0; i < languageGroups.Count; i++) {
        if (i!=0)
          sb.Append(" || ");
        var group = languageGroups[i];
        var columns = group.ToList();
        sb.Append("to_tsvector('");
        sb.Append(group.Key.Name);
        sb.Append("'::regconfig, ");
        for (int j = 0; j < columns.Count; j++) {
          if (j!=0)
            sb.Append(" || ' '::text");
          IndexColumn column = columns[j];
          sb.AppendFormat("({0})::text", QuoteIdentifier(column.Name));
        }
        sb.Append(")");
      }
      sb.Append(")");
      return sb.ToString();
    }

    // Constuctors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}