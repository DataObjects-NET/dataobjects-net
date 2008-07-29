// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.11

using System;
using System.Collections.Generic;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Sql.Resources;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using System.Linq;
using Xtensive.Storage.Rse.Providers.Compilable;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql.Compilers
{
  internal sealed class IndexProviderCompiler : TypeCompiler<IndexProvider>
  {
    protected override ExecutableProvider Compile(IndexProvider provider)
    {
      var index = provider.Index.Resolve(Handlers.Domain.Model);
      SqlSelect query = BuildProviderQuery(index);
      return new SqlProvider(provider, query);
    }

    private SqlSelect BuildProviderQuery(IndexInfo index)
    {
      if (index.IsVirtual) {
        if ((index.Attributes & IndexAttributes.Union) > 0)
          return BuildUnionQuery(index);
        if ((index.Attributes & IndexAttributes.Join) > 0)
          return BuildJoinQuery(index);
        if ((index.Attributes & IndexAttributes.Filtered) > 0)
          return BuildFilteredQuery(index);
        throw new NotSupportedException(String.Format(Strings.ExUnsupportedIndex, index.Name, index.Attributes));
      }
      return BuildTableQuery(index);
    }

    private SqlSelect BuildTableQuery(IndexInfo index)
    {
      Table table = ((DomainHandler)Handlers.DomainHandler).Catalog.DefaultSchema.Tables[index.ReflectedType.MappingName];
      SqlTableRef tableRef = SqlFactory.TableRef(table);
      SqlSelect query = SqlFactory.Select(tableRef);
      query.Columns.AddRange(index.Columns.Select(c => (SqlColumn)tableRef.Columns[c.Name]));
      return query;
    }

    private SqlSelect BuildUnionQuery(IndexInfo index)
    {
      ISqlQueryExpression result = null;

      var baseQueries = index.UnderlyingIndexes.Select(i => BuildProviderQuery(i)).ToList();
      foreach (var baseQuery in baseQueries) {
        SqlSelect select = SqlFactory.Select(SqlFactory.QueryRef(baseQuery));
        foreach (var columnInfo in index.Columns) {
          var column = baseQuery.Columns[columnInfo.Name];
          if (SqlExpression.IsNull(column))
            select.Columns.Add(SqlFactory.Null, columnInfo.Name);
          else
            select.Columns.Add(column);
        }
        if (result == null)
          result = select;
        else
          result = result.Union(select);
      }

      var unionRef = SqlFactory.QueryRef(result);
      SqlSelect query = SqlFactory.Select(unionRef);
      query.Columns.AddRange(unionRef.Columns.Cast<SqlColumn>());
      return query;
    }

    private SqlSelect BuildJoinQuery(IndexInfo index)
    {
      SqlTable result = null;
      SqlTable rootTable = null;
      IEnumerable<SqlColumn> columns = null;
      int keyColumnCount = index.KeyColumns.Count;
      int nonValueColumnsCount = keyColumnCount + index.IncludedColumns.Count;
      var baseQueries = index.UnderlyingIndexes.Select(i => BuildProviderQuery(i)).ToList();
      foreach (var baseQuery in baseQueries) {
        if (result==null) {
          result = SqlFactory.QueryRef(baseQuery);
          rootTable = result;
          columns = rootTable.Columns.Cast<SqlColumn>();
        }
        else {
          var queryRef = SqlFactory.QueryRef(baseQuery);
          SqlExpression joinExpression = null;
          for (int i = 0; i < keyColumnCount; i++) {
            SqlBinary binary = (queryRef.Columns[i] == rootTable.Columns[i]);
            if (SqlExpression.IsNull(joinExpression == null))
              joinExpression = binary;
            else
              joinExpression &= binary;
          }
          result = result.LeftOuterJoin(queryRef, joinExpression);
          columns = columns.Union(queryRef.Columns.Skip(nonValueColumnsCount).Cast<SqlColumn>());
        }
      }

      SqlSelect query = SqlFactory.Select(result);
      query.Columns.AddRange(columns);

      return query;
    }

    private SqlSelect BuildFilteredQuery(IndexInfo index)
    {
      var descendants = new List<TypeInfo> {index.ReflectedType};
      descendants.AddRange(index.ReflectedType.GetDescendants(true));
      var typeIds = descendants.Select(t => t.TypeId).ToArray();

      var baseQuery = BuildProviderQuery(index.UnderlyingIndexes[0]);
      SqlColumn typeIdColumn = baseQuery.Columns[Handlers.NameBuilder.TypeIdFieldName];
      SqlBinary inQuery = SqlFactory.In(typeIdColumn, SqlFactory.Array(typeIds));
      SqlSelect query = SqlFactory.Select(SqlFactory.QueryRef(baseQuery));
      query.Columns.AddRange(index.Columns.Select(c => baseQuery.Columns[c.Name]));
      query.Where = inQuery;

      return query;
    }


    // Constructor

    public IndexProviderCompiler(Rse.Compilation.Compiler provider)
      : base(provider)
    {
    }
  }
}