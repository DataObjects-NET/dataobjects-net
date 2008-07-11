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
using Xtensive.Storage.Providers.Sql.Providers;
using Xtensive.Storage.Providers.Sql.Resources;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Declaration;
using System.Linq;
using SQL = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql.Compilers
{
  public class IndexProviderCompiler : ProviderCompiler<IndexProvider>
  {
    private readonly ExecutionContext executionContext;
    private readonly DomainHandler domainHandler;

    protected override Provider Compile(IndexProvider provider)
    {
      var index = provider.Index;
      SqlSelect query = BuildProviderQuery(index);
      return new SqlProvider(new RecordHeader(index), query);
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
      Table table = domainHandler.Catalog.DefaultSchema.Tables[index.ReflectedType.MappingName];
      SqlTableRef tableRef = Xtensive.Sql.Dom.Sql.TableRef(table);
      SqlSelect query = Xtensive.Sql.Dom.Sql.Select(tableRef);
      query.Columns.AddRange(index.Columns.Select(c => (SqlColumn)tableRef.Columns[c.Name]));
      return query;
    }

    private SqlSelect BuildUnionQuery(IndexInfo index)
    {
      ISqlQueryExpression result = null;

      var baseQueries = index.BaseIndexes.Select(i => BuildProviderQuery(i)).ToList();
      foreach (var baseQuery in baseQueries) {
        SqlSelect select = SQL.Select(SQL.QueryRef(baseQuery));
        foreach (var columnInfo in index.Columns) {
          var column = baseQuery.Columns[columnInfo.Name];
          if (SqlExpression.IsNull(column))
            select.Columns.Add(SQL.Null, columnInfo.Name);
          else
            select.Columns.Add(column);
        }
        if (result == null)
          result = select;
        else
          result = result.Union(select);
      }

      var unionRef = SQL.QueryRef(result);
      SqlSelect query = SQL.Select(unionRef);
      query.Columns.AddRange(unionRef.Columns.Select(column => (SqlColumn)column));
      return query;
    }

    private SqlSelect BuildJoinQuery(IndexInfo index)
    {
      throw new NotImplementedException();
    }

    private SqlSelect BuildFilteredQuery(IndexInfo index)
    {
      var descendants = new List<TypeInfo> {index.ReflectedType};
      descendants.AddRange(index.ReflectedType.GetDescendants(true));
      var typeIds = descendants.Select(t => t.TypeId).ToArray();

      var baseQuery = BuildProviderQuery(index.BaseIndexes[0]);
      SqlColumn typeIdColumn = baseQuery.Columns[executionContext.Domain.NameProvider.TypeId];
      SqlBinary inQuery = SQL.In(typeIdColumn, SQL.Array(typeIds));
      SqlSelect query = SQL.Select(SQL.QueryRef(baseQuery));
      query.Columns.AddRange(index.Columns.Select(c => baseQuery.Columns[c.Name]));
      query.Where = inQuery;
      
      return query;
    }


    // Constructor

    public IndexProviderCompiler(Rse.Compilation.CompilerResolver resolver)
      : base(resolver)
    {
      executionContext = ((CompilerResolver) resolver).ExecutionContext;
      domainHandler = (DomainHandler)executionContext.DomainHandler;
    }
  }
}