// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Vakhtina Elena
// Created:    2009.02.13

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Providers.Sql;
using SqlFactory = Xtensive.Sql.Dom.Sql;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Providers.MsSql
{
  /// <inheritdoc/>
  [Serializable]
  public class MsSqlCompiler : SqlCompiler
  {
    protected override ExecutableProvider VisitSkip(SkipProvider provider, ExecutableProvider[] sources)
    {
      const string rowNumber = "RowNumber";

      var source = sources[0] as SqlProvider;
      if (source == null)
        return null;

      var sourceQuery = (SqlSelect)source.Request.Statement.Clone();
      var orderClause = ((IEnumerable) provider.Header.Order.Select(pair => sourceQuery[pair.Key].Name + (pair.Value == Direction.Positive ? " ASC" : " DESC"))).ToCommaDelimitedString();
      sourceQuery.Columns.Add(SqlFactory.Native("ROW_NUMBER() OVER (ORDER BY " + orderClause + ")"), rowNumber);
      sourceQuery.OrderBy.Clear();
      var queryRef = SqlFactory.QueryRef(sourceQuery);
      var query = SqlFactory.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns.Where(column => column.Name != rowNumber).Cast<SqlColumn>());
      query.Where = sourceQuery[rowNumber] > provider.CompiledCount();
      foreach (KeyValuePair<int, Direction> sortOrder in provider.Header.Order)
        query.OrderBy.Add(sortOrder.Key + 1, sortOrder.Value == Direction.Positive);
      var request = new SqlFetchRequest(query, provider.Header, source.Request.ParameterBindings);
      return new SqlProvider(provider, request, Handlers, source);
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public MsSqlCompiler(HandlerAccessor handlers)
      : base(handlers)
    {
    }
  }
}