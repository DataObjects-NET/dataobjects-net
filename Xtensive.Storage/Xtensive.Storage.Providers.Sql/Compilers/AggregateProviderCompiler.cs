// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.26

using System.Collections.Generic;
using System.Linq;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using SqlFactory = Xtensive.Sql.Dom.Sql;


namespace Xtensive.Storage.Providers.Sql.Compilers
{
  internal class AggregateProviderCompiler : TypeCompiler<AggregateProvider>
  {
    protected override ExecutableProvider Compile(AggregateProvider provider, params ExecutableProvider[] compiledSources)
    {
      var source = compiledSources[0] as SqlProvider;
      if (source == null)
        return null;
      
      var queryRef = SqlFactory.QueryRef(source.Request.Statement as SqlSelect);
      SqlSelect sqlSelect = SqlFactory.Select(queryRef);

      var columns = queryRef.Columns.ToList();
      sqlSelect.Columns.Clear();

      for(int i = 0; i < provider.GroupColumnIndexes.Length; i++) {
        sqlSelect.Columns.Add(columns[provider.GroupColumnIndexes[i]]);
        sqlSelect.GroupBy.Add(columns[provider.GroupColumnIndexes[i]]);
      }

      foreach (var col in provider.AggregateColumns) {
        SqlExpression expr = null;
        switch (col.AggregateType) {
          case AggregateType.Avg:
            expr = SqlFactory.Avg(columns[col.SourceIndex]);
            break;
          case AggregateType.Count:
            expr = SqlFactory.Count(columns[col.SourceIndex]);
            break;
          case AggregateType.Max:
            expr = SqlFactory.Max(columns[col.SourceIndex]);
            break;
          case AggregateType.Min:
            expr = SqlFactory.Min(columns[col.SourceIndex]);
            break;
          case AggregateType.Sum:
            expr = SqlFactory.Sum(columns[col.SourceIndex]);
            break;
        }
        sqlSelect.Columns.Add(expr, col.Name);
      }

      var request = new SqlFetchRequest(sqlSelect, provider.Header, source.Request.ParameterBindings);
      return new SqlProvider(provider, request, Handlers, source);
    }


    // Constructors

    public AggregateProviderCompiler(Rse.Compilation.Compiler provider)
      : base(provider)
    {
    }
  }
}