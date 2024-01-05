// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Vakhtina Elena
// Created:    2009.02.13

using Xtensive.Collections;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers.SqlServer
{
  internal class SqlCompiler : Providers.SqlCompiler
  {
    private static readonly Type ByteType = typeof(byte);
    private static readonly Type Int16Type = typeof(short);
    private static readonly Type Int32Type = typeof(int);
    private static readonly Type FloatType = typeof(float);
    private static readonly Type DecimalType = typeof(decimal);
    private static readonly Type StringType = typeof(string);

    protected override SqlProvider VisitFreeText(FreeTextProvider provider)
    {
      SqlFreeTextTable fromTable;
      QueryParameterBinding[] bindings;

      var stringTypeMapping = Driver.GetTypeMapping(StringType);
      var criteriaBinding = new QueryParameterBinding(
       stringTypeMapping, provider.SearchCriteria.Invoke, QueryParameterBindingType.Regular);

      var index = provider.PrimaryIndex.Resolve(Handlers.Domain.Model);
      var table = Mapping[index.ReflectedType];
      var columns = provider.Header.Columns.Select(column => column.Name).ToList();

      if (provider.TopN == null) {
        fromTable = SqlDml.FreeTextTable(table, criteriaBinding.ParameterReference, columns);
        bindings = new[] {criteriaBinding};
      }
      else {
        var intTypeMapping = Driver.GetTypeMapping(Int32Type);
        var topNBinding = new QueryParameterBinding(intTypeMapping, context => provider.TopN.Invoke(context), QueryParameterBindingType.Regular);
        fromTable = SqlDml.FreeTextTable(table, criteriaBinding.ParameterReference, columns, topNBinding.ParameterReference);
        bindings = new[] { criteriaBinding, topNBinding };
      }
      var fromTableRef = SqlDml.QueryRef(fromTable);
      var select = SqlDml.Select(fromTableRef);
      select.Columns.Add(fromTableRef.Columns[0]);
      select.Columns.Add(SqlDml.Cast(fromTableRef.Columns[1], SqlType.Double), "RANK");

      return CreateProvider(select, bindings, provider);
    }

    protected override SqlProvider VisitContainsTable(ContainsTableProvider provider)
    {
      SqlContainsTable fromTable;
      QueryParameterBinding[] bindings;

      var stringTypeMapping = Driver.GetTypeMapping(StringType);
      var criteriaBinding = new QueryParameterBinding(
       stringTypeMapping, provider.SearchCriteria.Invoke, QueryParameterBindingType.Regular);

      var index = provider.PrimaryIndex.Resolve(Handlers.Domain.Model);
      var table = Mapping[index.ReflectedType];
      var columns = provider.Header.Columns.Select(column => column.Name).ToList();

      var targetColumnNames = provider.TargetColumns.Select(c => c.Name).ToArray();
      if (provider.TopN == null) {
        fromTable = SqlDml.ContainsTable(table, criteriaBinding.ParameterReference, columns, targetColumnNames);
        bindings = new[] { criteriaBinding };
      }
      else {
        var intTypeMapping = Driver.GetTypeMapping(Int32Type);
        var topNBinding = new QueryParameterBinding(intTypeMapping, context => provider.TopN.Invoke(context), QueryParameterBindingType.Regular);
        fromTable = SqlDml.ContainsTable(table, criteriaBinding.ParameterReference, columns, targetColumnNames, topNBinding.ParameterReference);
        bindings = new[] { criteriaBinding, topNBinding };
      }
      var fromTableRef = SqlDml.QueryRef(fromTable);
      var select = SqlDml.Select(fromTableRef);
      select.Columns.Add(fromTableRef.Columns[0]);
      select.Columns.Add(SqlDml.Cast(fromTableRef.Columns[1], SqlType.Double), "RANK");

      return CreateProvider(select, bindings, provider);
    }

    protected override SqlExpression ProcessAggregate(
      SqlProvider source, List<SqlExpression> sourceColumns, AggregateColumn aggregateColumn)
    {
      var result = base.ProcessAggregate(source, sourceColumns, aggregateColumn);
      var aggregateReturnType = aggregateColumn.Type;
      var originCalculateColumn = source.Origin.Header.Columns[aggregateColumn.SourceIndex];
      var sqlType = Driver.MapValueType(aggregateReturnType);

      if (aggregateColumn.AggregateType == AggregateType.Min
        || aggregateColumn.AggregateType == AggregateType.Max
        || aggregateColumn.AggregateType == AggregateType.Sum) {
        if (!IsCalculatedColumn(originCalculateColumn)) {
          if (aggregateReturnType == DecimalType) {
            return result;
          }
          else if (ShouldCastDueType(aggregateReturnType)) {
            return SqlDml.Cast(result, Driver.MapValueType(aggregateReturnType));
          }
        }
        else if (ShouldCastDueType(aggregateReturnType)) {
          return SqlDml.Cast(result, Driver.MapValueType(aggregateReturnType));
        }
        return result;
      }
      if (aggregateColumn.AggregateType == AggregateType.Avg) {
        //var sqlType = Driver.MapValueType(aggregateReturnType);
        if (aggregateReturnType != originCalculateColumn.Type) {
          return SqlDml.Cast(SqlDml.Avg(SqlDml.Cast(sourceColumns[aggregateColumn.SourceIndex], sqlType)), sqlType);
        }
        if (!IsCalculatedColumn(originCalculateColumn)) {
          if (aggregateReturnType == DecimalType) {
            return result;
          }
          else if (ShouldCastDueType(aggregateReturnType)) {
            return SqlDml.Cast(SqlDml.Avg(SqlDml.Cast(sourceColumns[aggregateColumn.SourceIndex], sqlType)), sqlType);
          }
          else if (aggregateReturnType != originCalculateColumn.Type) {
            return SqlDml.Cast(SqlDml.Avg(SqlDml.Cast(sourceColumns[aggregateColumn.SourceIndex], sqlType)), sqlType);
          }
        }
        else {
          if (ShouldCastDueType(aggregateReturnType)) {
            return SqlDml.Cast(SqlDml.Avg(SqlDml.Cast(sourceColumns[aggregateColumn.SourceIndex], sqlType)), sqlType);
          }
          else if (aggregateReturnType != originCalculateColumn.Type) {
            return SqlDml.Cast(SqlDml.Avg(SqlDml.Cast(sourceColumns[aggregateColumn.SourceIndex], sqlType)), sqlType);
          }
          return result;
        }
      }
      return result;
    }

    private bool IsCalculatedColumn(Column column) => column is CalculatedColumn;

    private bool ShouldCastDueType(Type type)
    {
      return type == ByteType
        || type == Int16Type
        || type == DecimalType
        || type == FloatType;
    }

    // Constructors

    public SqlCompiler(HandlerAccessor handlers, CompilerConfiguration configuration)
      : base(handlers, configuration)
    {
    }
  }
}
