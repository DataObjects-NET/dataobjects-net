// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.13

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Providers.Compilable;

namespace Xtensive.Orm.Providers.Sql
{
  partial class SqlCompiler 
  {
    
    protected override SqlProvider VisitAggregate(AggregateProvider provider)
    {
      var source = Compile(provider.Source);

      var sqlSelect = ExtractSqlSelect(provider, source);

      var columns = ExtractColumnExpressions(sqlSelect, provider);
      var columnNames = columns.Select((c, i) =>
        i >= sqlSelect.Columns.Count
          ? sqlSelect.From.Columns[i].Name
          : sqlSelect.Columns[i].Name).ToList();
      sqlSelect.Columns.Clear();

      for (int i = 0; i < provider.GroupColumnIndexes.Length; i++) {
        var columnIndex = provider.GroupColumnIndexes[i];
        var column = columns[columnIndex];
        sqlSelect.GroupBy.Add(column);
        if (!(column is SqlColumn)) {
          var columnName = ProcessAliasedName(columnNames[columnIndex]);
          column = SqlDml.ColumnRef(SqlDml.Column(column), columnName);
        }
        sqlSelect.Columns.Add(column);
      }

      foreach (var column in provider.AggregateColumns) {
        var expression = ProcessAggregate(source, columns, column);
        sqlSelect.Columns.Add(expression, column.Name);
      }

      return CreateProvider(sqlSelect, provider, source);
    }

    /// <summary>
    /// Translates <see cref="AggregateColumn"/> to corresponding <see cref="SqlExpression"/>.
    /// </summary>
    /// <param name="source">The source <see cref="SqlProvider"/>.</param>
    /// <param name="sourceColumns">The source columns.</param>
    /// <param name="aggregateColumn">The aggregate column.</param>
    /// <returns>Aggregate processing result (expression).</returns>
    protected virtual SqlExpression ProcessAggregate(SqlProvider source, List<SqlExpression> sourceColumns, AggregateColumn aggregateColumn)
    {
      switch (aggregateColumn.AggregateType) {
      case AggregateType.Avg:
        return SqlDml.Avg(sourceColumns[aggregateColumn.SourceIndex]);
      case AggregateType.Count:
        return SqlDml.Count(SqlDml.Asterisk);
      case AggregateType.Max:
        return SqlDml.Max(sourceColumns[aggregateColumn.SourceIndex]);
      case AggregateType.Min:
        return SqlDml.Min(sourceColumns[aggregateColumn.SourceIndex]);
      case AggregateType.Sum:
        return SqlDml.Sum(sourceColumns[aggregateColumn.SourceIndex]);
      default:
        throw new ArgumentException();
      }
    }
  }
}