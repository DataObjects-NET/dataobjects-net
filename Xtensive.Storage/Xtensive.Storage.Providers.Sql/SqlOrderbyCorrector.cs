// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.05.12

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xtensive.Core;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Providers.Sql
{
  internal sealed class SqlOrderbyCorrector : IPostCompiler
  {
    private readonly HandlerAccessor handlers;

    public ExecutableProvider Process(ExecutableProvider rootProvider)
    {
      var sqlProvider = (SqlProvider) rootProvider;
      var query = (SqlSelect) sqlProvider.Request.SelectStatement.Clone();
      if (query.OrderBy.Count > 0)
        return rootProvider;
      if (rootProvider.Origin.Header.Order.Count > 0)
        ApplyOrderingToProvider(sqlProvider, query);
      else {
        var originAsSelect = rootProvider.Origin as SelectProvider;
        if (originAsSelect!=null && originAsSelect.Source.Header.Order.Count > 0)
          query = ApplyOrderingToSelectProvider(rootProvider, originAsSelect);
        else
          return rootProvider;
      }
      return new SqlProvider(rootProvider.Origin, query, handlers, sqlProvider.Request.ParameterBindings,
        (ExecutableProvider[]) rootProvider.Sources);
    }

    private static void ApplyOrderingToProvider(ExecutableProvider sqlProvider, SqlSelect query)
    {
      foreach (KeyValuePair<int, Direction> pair in sqlProvider.Origin.Header.Order)
        query.OrderBy.Add(query.Columns[pair.Key], pair.Value==Direction.Positive);
    }

    private static SqlSelect ApplyOrderingToSelectProvider(Provider rootProvider,
      SelectProvider originAsSelect)
    {
      var queryRef = ((SqlProvider) rootProvider.Sources[0]).PermanentReference;
      var result = SqlDml.Select(queryRef);
      var columnIndexes = originAsSelect.ColumnIndexes;
      result.Columns.AddRange(from index in columnIndexes select (SqlColumn) queryRef.Columns[index]);
      foreach (KeyValuePair<int, Direction> pair in originAsSelect.Source.Header.Order)
        result.OrderBy.Add(queryRef.Columns[pair.Key], pair.Value==Direction.Positive);
      return result;
    }


    // Constructors

    public SqlOrderbyCorrector(HandlerAccessor handlers)
    {
      ArgumentValidator.EnsureArgumentNotNull(handlers, "handlers");
      this.handlers = handlers;
    }
  }
}