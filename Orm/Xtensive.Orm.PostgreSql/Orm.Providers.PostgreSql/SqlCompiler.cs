// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.04.27

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Compilation;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Reflection.PostgreSql;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers.PostgreSql
{
  internal class SqlCompiler : Providers.SqlCompiler
  {
    private const int MaxDotnetDecimalPrecision = 28;

    private readonly bool canRemoveInsignificantZerosInDecimals;

    protected override SqlProvider VisitFreeText(FreeTextProvider provider)
    {
      var rankColumnName = provider.Header.Columns[provider.Header.Columns.Count - 1].Name;
      var stringTypeMapping = Driver.GetTypeMapping(WellKnownTypes.StringType);
      var binding = new QueryParameterBinding(stringTypeMapping,
        provider.SearchCriteria.Invoke, QueryParameterBindingType.Regular);

      var select = SqlDml.Select();
      var realPrimaryIndex = provider.PrimaryIndex.Resolve(Handlers.Domain.Model);
      var index = realPrimaryIndex.ReflectedType.Indexes.PrimaryIndex;
      var query = BuildProviderQuery(index);
      var table = Mapping[realPrimaryIndex.ReflectedType];
      var fromTable = SqlDml.FreeTextTable(table, binding.ParameterReference,
        table.Columns.Select(column => column.Name).AddOne(rankColumnName).ToArray(table.Columns.Count + 1));
      var fromTableRef = SqlDml.QueryRef(fromTable);
      foreach (var column in query.Columns) {
        select.Columns.Add(fromTableRef.Columns[column.Name] ?? column);
      }
      select.Columns.Add(SqlDml.Cast(fromTableRef.Columns[rankColumnName], SqlType.Double), rankColumnName);
      select.From = fromTableRef;
      if (provider.TopN != null) {
        select.Limit = provider.TopN.Invoke();
        select.OrderBy.Add(select.Columns[rankColumnName], false);  
      }
      return CreateProvider(select, binding, provider);
    }

    //protected override SqlExpression ProcessAggregate(SqlProvider source, List<SqlExpression> sourceColumns, AggregateColumn aggregateColumn)
    //{
    //  var result = base.ProcessAggregate(source, sourceColumns, aggregateColumn);
    //  if (aggregateColumn.AggregateType == AggregateType.Sum || aggregateColumn.AggregateType == AggregateType.Avg) {
    //    result = SqlDml.Cast(result, Driver.MapValueType(aggregateColumn.Type));
    //  }

    //  return result;
    //}

    protected override SqlExpression ProcessAggregate(SqlProvider source, List<SqlExpression> sourceColumns, AggregateColumn aggregateColumn)
    {
      var result = base.ProcessAggregate(source, sourceColumns, aggregateColumn);
      var aggregateType = aggregateColumn.AggregateType;
      var originCalculateColumn = source.Origin.Header.Columns[aggregateColumn.SourceIndex];
      if (AggregateRequiresDecimalAdjustments(aggregateColumn)) {
        if (canRemoveInsignificantZerosInDecimals) {
          return (IsCalculatedColumn(originCalculateColumn))
            ? PostgresqlSqlDml.DecimalTrimScale(SqlDml.Cast(result, Driver.MapValueType(aggregateColumn.Type)))
            : PostgresqlSqlDml.DecimalTrimScale(result);
        }
        if (!IsCalculatedColumn(originCalculateColumn)) {
          // this is aggregate by one column, result will be defined by the precision and scale of the column
          return result;
        }

        // For expressions we had to try to guess result type parameters to avoid overflow exception
        // on reading something like 12.000000000000000000000000000000 (30 zeros) which in practice can be reduced
        // to 12.0 on reading but Npgsql does not allow us neither to turn on such conversion inside Npgsql (as it was in v3.x.x) nor
        // to get raw data and make conversion by ourselves (because nothing similar to SqlDecimal has provided by the library).

        // Official answer of the Npgsql team is to either cast to DECIMAL with proper parameters or read all parameters as
        // strings and then convert :-)
        // Reading strings is not an option so we try to tell fortunes in a teacup :-(
        var resultType = (!TryAdjustPrecisionScale(aggregateColumn.Descriptor.DecimalParametersHint, aggregateType, out var newPrecision, out var newScale))
          ? Driver.MapValueType(aggregateColumn.Type)
          : Driver.MapValueType(aggregateColumn.Type, null, newPrecision, newScale);
        return SqlDml.Cast(result, resultType);
      }
      else if (aggregateType != AggregateType.Count) {
        result = SqlDml.Cast(result, Driver.MapValueType(aggregateColumn.Type));
      }
      return result;
    }

    private bool AggregateRequiresDecimalAdjustments(AggregateColumn aggregateColumn)
    {
      var aggregateType = aggregateColumn.AggregateType;
      return (aggregateType == AggregateType.Sum || aggregateType == AggregateType.Avg
          || aggregateType == AggregateType.Min || aggregateType == AggregateType.Min)
        && aggregateColumn.Type == typeof(decimal);
    }

    private bool TryAdjustPrecisionScale(
      in (int precision, int scale)? typeHint,
      in AggregateType aggregateType,
      out int precision, out int scale)
    {
      if (!typeHint.HasValue) {
        precision = -1;
        scale = -1;
        return false;
      }
      var typeHintValue = typeHint.Value;

      if (typeHintValue.precision == MaxDotnetDecimalPrecision) {
        // No room for adjust, otherwise we'll lose floor part data
        precision = typeHintValue.precision;
        scale = typeHintValue.scale;
        return true;
      }

      // choose max available precision for .net or let it be the one user declared
      precision = (typeHintValue.precision < MaxDotnetDecimalPrecision) ? MaxDotnetDecimalPrecision : typeHintValue.precision;

      // It is benefitial to increase scale but for how much? It is open question,
      // sometimes we need bigger floor part, and sometimes bigger fractional part.
      // This algorithm is a trade-off.
      scale = aggregateType switch {
        AggregateType.Avg =>
          (typeHintValue.precision < MaxDotnetDecimalPrecision)
            ? typeHintValue.scale + Math.Max((precision - typeHintValue.precision) / 2, 1)
            : typeHintValue.scale + 1,
        AggregateType.Sum =>
            (typeHintValue.precision < MaxDotnetDecimalPrecision - 1)
              ? typeHintValue.scale + 2
              : typeHintValue.scale + 1,
        AggregateType.Min =>
            (typeHintValue.precision < MaxDotnetDecimalPrecision - 1)
              ? typeHintValue.scale + 2
              : typeHintValue.scale + 1,
        AggregateType.Max =>
            (typeHintValue.precision < MaxDotnetDecimalPrecision - 1)
              ? typeHintValue.scale + 2
              : typeHintValue.scale + 1,
        _ => typeHintValue.scale,
      };
      return true;
    }

    private bool IsCalculatedColumn(Column column) => column is CalculatedColumn;

    public SqlCompiler(HandlerAccessor handlers, CompilerConfiguration configuration)
      : base(handlers, configuration)
    {

    }
  }
}