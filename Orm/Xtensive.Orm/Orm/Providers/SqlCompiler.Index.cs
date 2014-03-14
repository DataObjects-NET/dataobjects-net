// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.13

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Orm.Model;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using IndexInfo = Xtensive.Orm.Model.IndexInfo;

namespace Xtensive.Orm.Providers
{
  partial class SqlCompiler 
  {
    protected override SqlProvider VisitFreeText(FreeTextProvider provider)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitIndex(IndexProvider provider)
    {
      var index = provider.Index.Resolve(Handlers.Domain.Model);
      SqlSelect query = BuildProviderQuery(index);
      return CreateProvider(query, provider);
    }

    protected SqlSelect BuildProviderQuery(IndexInfo index)
    {
      if (index.IsVirtual) {
        if ((index.Attributes & IndexAttributes.Union) > 0)
          return BuildUnionQuery(index);
        if ((index.Attributes & IndexAttributes.Join) > 0)
          return BuildJoinQuery(index);
        if ((index.Attributes & IndexAttributes.Filtered) > 0)
          return BuildFilteredQuery(index);
        if ((index.Attributes & IndexAttributes.View) > 0)
          return BuildViewQuery(index);
        if ((index.Attributes & IndexAttributes.Typed) > 0)
          return BuildTypedQuery(index);
        throw new NotSupportedException(String.Format(Strings.ExUnsupportedIndex, index.Name, index.Attributes));
      }
      return BuildTableQuery(index);
    }

    private SqlSelect BuildTableQuery(IndexInfo index)
    {
      var domainHandler = Handlers.DomainHandler;
      var table = Mapping[index.ReflectedType];

      var atRootPolicy = false;

      if (table==null) {
        table = Mapping[index.ReflectedType.GetRoot()];
        atRootPolicy = true;
      }

      SqlSelect query;
      if (!atRootPolicy) {
        var tableRef = SqlDml.TableRef(table);
        query = SqlDml.Select(tableRef);
        query.Columns.AddRange(index.Columns.Select(c => tableRef[c.Name]));
      }
      else {
        var root = index.ReflectedType.GetRoot().AffectedIndexes.First(i => i.IsPrimary);
        var lookup = root.Columns.ToDictionary(c => c.Field, c => c.Name);
        var tableRef = SqlDml.TableRef(table);
        query = SqlDml.Select(tableRef);
        query.Columns.AddRange(index.Columns.Select(c => tableRef[lookup[c.Field]]));
      }
      return query;
    }

    private SqlSelect BuildUnionQuery(IndexInfo index)
    {
      ISqlQueryExpression result = null;

      var baseQueries = index.UnderlyingIndexes.Select(BuildProviderQuery).ToList();
      foreach (var select in baseQueries) {
        result = result==null
          ? (ISqlQueryExpression) select
          : result.Union(select);
      }

      var unionRef = SqlDml.QueryRef(result);
      var query = SqlDml.Select(unionRef);
      query.Columns.AddRange(unionRef.Columns.Cast<SqlColumn>());
      return query;
    }

    private SqlSelect BuildJoinQuery(IndexInfo index)
    {
      SqlTable resultTable = null;
      SqlTable rootTable = null;

      int keyColumnCount = index.KeyColumns.Count;
      var underlyingQueries = index.UnderlyingIndexes.Select(BuildProviderQuery);

      var sourceTables = index.UnderlyingIndexes.Any(i => i.IsVirtual)
        ? underlyingQueries.Select(SqlDml.QueryRef).Cast<SqlTable>().ToList()
        : underlyingQueries.Select(q => {
          var tableRef = (SqlTableRef) q.From;
          return (SqlTable) SqlDml.TableRef(tableRef.DataTable, tableRef.Name, q.Columns.Select(c => c.Name));
        }).ToList();

      foreach (var table in sourceTables) {
        if (resultTable==null)
          resultTable = rootTable = table;
        else {
          SqlExpression joinExpression = null;
          for (int i = 0; i < keyColumnCount; i++) {
            var binary = (table.Columns[i]==rootTable.Columns[i]);
            if (joinExpression.IsNullReference())
              joinExpression = binary;
            else
              joinExpression &= binary;
          }
          resultTable = resultTable.InnerJoin(table, joinExpression);
        }
      }

      var columns = new List<SqlColumn>();
      foreach (var map in index.ValueColumnsMap) {
        var table = sourceTables[map.First];
        if (columns.Count==0) {
          var keyColumns = Enumerable
            .Range(0, keyColumnCount)
            .Select(i => table.Columns[i])
            .Cast<SqlColumn>();
          columns.AddRange(keyColumns);
        }
        var valueColumns = map.Second
          .Select(columnIndex => table.Columns[columnIndex + keyColumnCount])
          .Cast<SqlColumn>();
        columns.AddRange(valueColumns);
      }

      var query = SqlDml.Select(resultTable);
      query.Columns.AddRange(columns);

      return query;
    }

    private SqlSelect BuildFilteredQuery(IndexInfo index)
    {
      var underlyingIndex = index.UnderlyingIndexes[0];
      var baseQuery = BuildProviderQuery(underlyingIndex);

      SqlExpression filter = null;
      var type = index.ReflectedType;
      var discriminatorMap = type.Hierarchy.TypeDiscriminatorMap;
      var filterByTypes = index.FilterByTypes.ToList();
      if (underlyingIndex.IsTyped && discriminatorMap != null) {
        var columnType = discriminatorMap.Column.ValueType;
        var discriminatorColumnIndex = underlyingIndex.Columns
          .Where(c => !c.Field.IsTypeId)
          .Select((c,i) => new {c,i})
          .Where(p => p.c == discriminatorMap.Column)
          .Single().i;
        var discriminatorColumn = baseQuery.From.Columns[discriminatorColumnIndex];
        var containsDefault = filterByTypes.Contains(discriminatorMap.Default);
        var values = filterByTypes
          .Select(t => GetDiscriminatorValue(discriminatorMap, t.TypeDiscriminatorValue));
        if (filterByTypes.Count == 1) {
          var discriminatorValue = GetDiscriminatorValue(discriminatorMap, filterByTypes.First().TypeDiscriminatorValue);
          filter = discriminatorColumn == SqlDml.Literal(discriminatorValue);
        }
        else {
          filter = SqlDml.In(discriminatorColumn, SqlDml.Array(values));
          if (containsDefault) {
            var allValues = discriminatorMap
              .Select(p => GetDiscriminatorValue(discriminatorMap, p.First));
            filter |= SqlDml.NotIn(discriminatorColumn, SqlDml.Array(allValues));
          }
        }
      }
      else {
        var typeIdColumn = baseQuery.Columns[Handlers.Domain.Handlers.NameBuilder.TypeIdColumnName];
        var typeIds = filterByTypes.Select(t => t.TypeId).ToArray();
        filter = filterByTypes.Count == 1
          ? typeIdColumn == filterByTypes.First().TypeId
          : SqlDml.In(typeIdColumn, SqlDml.Array(typeIds));
      }
      var query = SqlDml.Select(baseQuery.From);
      query.Columns.AddRange(baseQuery.Columns);
      query.Where = filter;
      return query;
    }

    private SqlSelect BuildViewQuery(IndexInfo index)
    {
      var underlyingIndex = index.UnderlyingIndexes[0];
      var baseQuery = BuildProviderQuery(underlyingIndex);
      var query = SqlDml.Select(baseQuery.From);
      query.Where = baseQuery.Where;
      query.Columns.AddRange(index.SelectColumns.Select(i => baseQuery.Columns[i]));
      return query;
    }

    private SqlSelect BuildTypedQuery(IndexInfo index)
    {
      var underlyingIndex = index.UnderlyingIndexes[0];
      var baseQuery = BuildProviderQuery(underlyingIndex);
      var query = SqlDml.Select(baseQuery.From);
      query.Where = baseQuery.Where;

      var baseColumns = baseQuery.Columns.ToList();
      var typeIdColumnIndex = index.Columns
        .Select((c, i) => new {c, i})
        .Single(p => p.c.Field.IsTypeId).i;
      var type = index.ReflectedType;
      var typeIdColumn = SqlDml.ColumnRef(
        SqlDml.Column(SqlDml.Literal(type.TypeId)), 
        WellKnown.TypeIdFieldName);
      var discriminatorMap = type.Hierarchy.TypeDiscriminatorMap;
      if (discriminatorMap != null) {
        var discriminatorColumnIndex = underlyingIndex.Columns.IndexOf(discriminatorMap.Column);
        var discriminatorColumn = baseQuery.From.Columns[discriminatorColumnIndex];
        var sqlCase = SqlDml.Case(discriminatorColumn);
        foreach (var pair in discriminatorMap) {
          var discriminatorValue = GetDiscriminatorValue(discriminatorMap, pair.First);
          var typeId = pair.Second.TypeId;
          sqlCase.Add(SqlDml.Literal(discriminatorValue), SqlDml.Literal(typeId));
        }
        if (discriminatorMap.Default != null)
          sqlCase.Else = SqlDml.Literal(discriminatorMap.Default.TypeId);
        typeIdColumn = SqlDml.ColumnRef(
          SqlDml.Column(sqlCase),
          WellKnown.TypeIdFieldName);
      }
      baseColumns.Insert(typeIdColumnIndex, typeIdColumn);
      query.Columns.AddRange(baseColumns);
      return query;
    }

    private object GetDiscriminatorValue(TypeDiscriminatorMap discriminatorMap, object fieldValue)
    {
      var field = discriminatorMap.Field;
      var column = discriminatorMap.Column;
      return field.ValueType!=column.ValueType
        ? Convert.ChangeType(fieldValue, column.ValueType)
        : fieldValue;
    }
  }
}