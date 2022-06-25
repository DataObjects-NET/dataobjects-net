// Copyright (C) 2009-2021 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.13

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Model;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Reflection;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using IndexInfo = Xtensive.Orm.Model.IndexInfo;
using TypeMapping = Xtensive.Sql.TypeMapping;

namespace Xtensive.Orm.Providers
{
  public partial class SqlCompiler
  {
    protected readonly struct QueryAndBindings
    {
      public SqlSelect Query { get; }
      public IReadOnlyList<QueryParameterBinding> Bindings { get; }

      public QueryAndBindings(SqlSelect initialQuery)
        : this(initialQuery, new List<QueryParameterBinding>())
      {
      }

      public QueryAndBindings(SqlSelect initialQuery, IReadOnlyList<QueryParameterBinding> bindings)
      {
        Query = initialQuery;
        Bindings = bindings;
      }
    }

    protected override SqlProvider VisitFreeText(FreeTextProvider provider)
    {
      throw new NotSupportedException();
    }

    protected override SqlProvider VisitContainsTable(ContainsTableProvider provider)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitIndex(IndexProvider provider)
    {
      var index = provider.Index.Resolve(Handlers.Domain.Model);
      var queryAndBindings = BuildProviderQuery(index);
      return CreateProvider(queryAndBindings.Query, queryAndBindings.Bindings, provider);
    }

    protected QueryAndBindings BuildProviderQuery(IndexInfo index)
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

    private QueryAndBindings BuildTableQuery(IndexInfo index)
    {
      var domainHandler = Handlers.DomainHandler;
      var table = Mapping[index.ReflectedType];

      var atRootPolicy = false;

      if (table==null) {
        table = Mapping[index.ReflectedType.GetRoot()];
        atRootPolicy = true;
      }

      var indexColumns = index.Columns;
      var tableRef = SqlDml.TableRef(table);
      var query = SqlDml.Select(tableRef);
      var queryColumns = query.Columns;
      queryColumns.Capacity = queryColumns.Count + indexColumns.Count;
      if (!atRootPolicy) {
        foreach (var c in indexColumns) {
          queryColumns.Add(tableRef[c.Name]);
        }
      }
      else {
        var root = index.ReflectedType.GetRoot().AffectedIndexes.First(i => i.IsPrimary);
        var lookup = root.Columns.ToDictionary(c => c.Field, c => c.Name);
        foreach (var c in indexColumns) {
          queryColumns.Add(tableRef[lookup[c.Field]]);
        }
      }
      return new QueryAndBindings(query);
    }

    private QueryAndBindings BuildUnionQuery(IndexInfo index)
    {
      ISqlQueryExpression result = null;
      List<QueryParameterBinding> resultBindings = null;

      var baseQueries = index.UnderlyingIndexes.Select(BuildProviderQuery).ToList();
      foreach (var select in baseQueries) {
        result = result==null
          ? (ISqlQueryExpression) select.Query
          : result.Union(select.Query);
        if (resultBindings == null) {
          resultBindings = select.Bindings.ToList();
        }
        else {
          resultBindings.AddRange(select.Bindings);
        }
      }

      var unionRef = SqlDml.QueryRef(result);
      var query = SqlDml.Select(unionRef);
      query.Columns.AddRange(unionRef.Columns);

      return new QueryAndBindings(query, resultBindings);
    }

    private QueryAndBindings BuildJoinQuery(IndexInfo index)
    {
      SqlTable resultTable = null;
      SqlTable rootTable = null;

      var keyColumnCount = index.KeyColumns.Count;
      var underlyingQueries = new QueryAndBindings[index.UnderlyingIndexes.Count];
      var underlyingIndexes = index.UnderlyingIndexes;
      var haveVirtualUnderlyingIndexes = false;
      for (int i = 0, count = underlyingIndexes.Count; i < count; i++) {
        var underlyingIndex = underlyingIndexes[i];
        underlyingQueries[i] = BuildProviderQuery(underlyingIndex);
        haveVirtualUnderlyingIndexes = haveVirtualUnderlyingIndexes || underlyingIndex.IsVirtual;
      }

      var sourceTables = new SqlTable[underlyingQueries.Length];
      List<QueryParameterBinding> resultBindings = null;

      for (int i = 0, length = underlyingQueries.Length; i < length; i++) {
        var item = underlyingQueries[i];

        sourceTables[i] = haveVirtualUnderlyingIndexes ? SqlDml.QueryRef(item.Query) : CreateSourceTable(item);

        if (resultBindings == null) {
          resultBindings = item.Bindings.ToList();
        }
        else {
          resultBindings.AddRange(item.Bindings);
        }
      }

      foreach (var table in sourceTables) {
        if (resultTable==null)
          resultTable = rootTable = table;
        else {
          SqlExpression joinExpression = null;
          for (int i = 0; i < keyColumnCount; i++) {
            var binary = (table.Columns[i]==rootTable.Columns[i]);
            if (joinExpression is null)
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

      return new QueryAndBindings(query, resultBindings);
    }

    private static SqlTable CreateSourceTable(QueryAndBindings item)
    {
      var columns = item.Query.Columns;
      var columnNames = new string[columns.Count];
      for (int i = 0, count = columns.Count; i < count; i++) {
        columnNames[i] = columns[i].Name;
      }

      return SqlDml.TableRef(((SqlTableRef) item.Query.From).DataTable,
        ((SqlTableRef) item.Query.From).Name, columnNames);
    }

    private QueryAndBindings BuildFilteredQuery(IndexInfo index)
    {
      var underlyingIndex = index.UnderlyingIndexes[0];
      var baseQueryAndBindings = BuildProviderQuery(underlyingIndex);
      var baseQuery = baseQueryAndBindings.Query;
      var bindings = baseQueryAndBindings.Bindings;

      SqlExpression filter = null;
      var type = index.ReflectedType;
      var discriminatorMap = type.Hierarchy.TypeDiscriminatorMap;
      var filterByTypes = index.FilterByTypes.ToList();
      var filterByTypesCount = filterByTypes.Count;
      if (underlyingIndex.IsTyped && discriminatorMap != null) {
        var columnType = discriminatorMap.Column.ValueType;
        var discriminatorColumnIndex = underlyingIndex.Columns
          .Where(c => !c.Field.IsTypeId)
          .Select((c, i) => (c, i))
          .Where(p => p.c == discriminatorMap.Column)
          .Single().i;
        var discriminatorColumn = baseQuery.From.Columns[discriminatorColumnIndex];
        var containsDefault = filterByTypes.Contains(discriminatorMap.Default);
        var values = filterByTypes
          .Select(t => GetDiscriminatorValue(discriminatorMap, t.TypeDiscriminatorValue));
        if (filterByTypesCount == 1) {
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

        filter = filterByTypesCount == 1
          ? typeIdColumn == SqlDml.Placeholder(filterByTypes[0])
          : SqlDml.In(typeIdColumn, SqlDml.Array(filterByTypes.Select(SqlDml.Placeholder)));
      }
      var query = SqlDml.Select(baseQuery.From);
      query.Columns.AddRange(baseQuery.Columns);
      query.Where = filter;

      return new QueryAndBindings(query, bindings);
    }

    private QueryAndBindings BuildViewQuery(IndexInfo index)
    {
      var underlyingIndex = index.UnderlyingIndexes[0];
      var baseQueryAndBindings = BuildProviderQuery(underlyingIndex);
      var baseQuery = baseQueryAndBindings.Query;

      var query = SqlDml.Select(baseQuery.From);
      query.Where = baseQuery.Where;
      query.Columns.AddRange(index.SelectColumns.Select(i => baseQuery.Columns[i]));

      return new QueryAndBindings(query, baseQueryAndBindings.Bindings);
    }

    private QueryAndBindings BuildTypedQuery(IndexInfo index)
    {
      var underlyingIndex = index.UnderlyingIndexes[0];
      var baseQueryAndBindings = BuildProviderQuery(underlyingIndex);
      var baseQuery = baseQueryAndBindings.Query;
      var bindings = baseQueryAndBindings.Bindings;
      var query = SqlDml.Select(baseQuery.From);
      query.Where = baseQuery.Where;

      var baseColumns = baseQuery.Columns.ToList();
      var typeIdColumnIndex = index.Columns
        .Select((c, i) => (c.Field, i))
        .Single(p => p.Field.IsTypeId && p.Field.IsSystem).i;
      var type = index.ReflectedType;
      var typeIdColumn = SqlDml.Column(SqlDml.Placeholder(type));

      var discriminatorMap = type.Hierarchy.TypeDiscriminatorMap;
      if (discriminatorMap != null) {
        var discriminatorColumnIndex = 0;
        var discriminatorColumnInfo = discriminatorMap.Column;
        var underlyingColumns = underlyingIndex.Columns; 
        for (var columnCount = underlyingColumns.Count; discriminatorColumnIndex < columnCount; discriminatorColumnIndex++) {
          var column = underlyingColumns[discriminatorColumnIndex];
          if (column.Equals(discriminatorColumnInfo)) {
            break;
          }
        }
        var discriminatorColumn = baseQuery.From.Columns[discriminatorColumnIndex];
        var sqlCase = SqlDml.Case(discriminatorColumn);
        foreach (var pair in discriminatorMap) {
          var discriminatorValue = GetDiscriminatorValue(discriminatorMap, pair.First);
          _ = sqlCase.Add(SqlDml.Literal(discriminatorValue), SqlDml.Placeholder(pair.Second));
        }
        if (discriminatorMap.Default != null) {
          sqlCase.Else = SqlDml.Placeholder(discriminatorMap.Default);
        }

        typeIdColumn = SqlDml.Column(sqlCase);
        bindings = baseQueryAndBindings.Bindings;
      }

      var typeIdColumnRef = SqlDml.ColumnRef(typeIdColumn, WellKnown.TypeIdFieldName);
      baseColumns.Insert(typeIdColumnIndex, typeIdColumnRef);
      query.Columns.AddRange(baseColumns);

      return new QueryAndBindings(query, bindings);
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