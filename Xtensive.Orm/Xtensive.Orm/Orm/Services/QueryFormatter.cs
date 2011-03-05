// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.03.02

using System.Collections.Generic;
using System.Linq;
using Xtensive.Aspects;
using Xtensive.Core;
using Xtensive.IoC;

namespace Xtensive.Orm.Services
{
  [Service(typeof(QueryFormatter), Singleton = true)]
  [Infrastructure]
  public class QueryFormatter : SessionBound, ISessionService
  {
    /// <summary>
    /// Formats the specified <paramref name="query"/> in SQL.
    /// </summary>
    /// <param name="query">The query to format.</param>
    /// <returns>A string containing formatted query.</returns>
    public string ToSqlString<T>(IQueryable<T> query)
    {
      ArgumentValidator.EnsureArgumentNotNull(query, "query");

      var recordset = Session.Query.Provider.Translate<IEnumerable<T>>(query.Expression).RecordQuery;
      var executableProvider = recordset.Compile(Session);
      var sqlProvider = executableProvider as Storage.Providers.Sql.SqlProvider;

      if (sqlProvider == null)
        return string.Empty;

      var domainHandler = (Storage.Providers.Sql.DomainHandler)Session.Handler.Handlers.DomainHandler;
      var compiled = sqlProvider.Request.GetCompiledStatement(domainHandler);
      return compiled.ToString();
    }

    /// <summary>
    /// Formats the specified <paramref name="query"/> in C# notation.
    /// </summary>
    /// <param name="query">The query to format.</param>
    /// <returns>A string containing formatted query.</returns>
    public string ToString<T>(IQueryable<T> query)
    {
      ArgumentValidator.EnsureArgumentNotNull(query, "query");

      return query.Expression.ToString(true);
    }


    // Constructors

    [ServiceConstructor]
    public QueryFormatter(Session session) 
      : base(session)
    {
    }
  }
}