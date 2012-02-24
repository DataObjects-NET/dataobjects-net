// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.03.02

using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Xtensive.Aspects;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Orm.Providers.Sql;

namespace Xtensive.Orm.Services
{
  [Service(typeof (QueryFormatter), Singleton = true), Infrastructure]
  public class QueryFormatter : SessionBound, ISessionService
  {
    /// <summary>
    /// Formats the specified <paramref name="query"/> in SQL.
    /// </summary>
    /// <typeparam name="T">Type of element.</typeparam>
    /// <param name="query">The query to format.</param>
    /// <returns>A string containing formatted query.</returns>
    public string ToSqlString<T>(IQueryable<T> query)
    {
      ArgumentValidator.EnsureArgumentNotNull(query, "query");
      var part = ToCommandPart(query);
      if (part==null)
        return string.Empty;
      return part.Query;
    }

    /// <summary>
    /// Formats the specified <paramref name="query"/> in C# notation.
    /// </summary>
    /// <typeparam name="T">Type of element.</typeparam>
    /// <param name="query">The query to format.</param>
    /// <returns>A string containing formatted query.</returns>
    public string ToString<T>(IQueryable<T> query)
    {
      ArgumentValidator.EnsureArgumentNotNull(query, "query");
      return query.Expression.ToString(true);
    }

    /// <summary>
    /// Formats the specified <paramref name="query"/> in <see cref="DbCommand"/>.
    /// </summary>
    /// <typeparam name="T">Type of element.</typeparam>
    /// <param name="query">The query to format.</param>
    /// <returns>A <see cref="DbCommand"/>.</returns>
    public DbCommand ToDbCommand<T>(IQueryable<T> query)
    {
      ArgumentValidator.EnsureArgumentNotNull(query, "query");
      var part = ToCommandPart(query);
      if (part==null)
        return null;
      var domainHandler = (DomainHandler) Session.Handler.Handlers.DomainHandler;
      var sessionHandler = (SessionHandler) Session.Handler;
      var dbCommand = sessionHandler.Connection.CreateCommand();
      var command = new Command(domainHandler.Driver, Session, dbCommand);
      command.AddPart(part);
      dbCommand.CommandText = domainHandler.Driver.BuildBatch(new[] {part.Query});
      return dbCommand;
    }

    private CommandPart ToCommandPart<T>(IQueryable<T> query)
    {
      var translatedQuery = Session.Query.Provider.Translate<IEnumerable<T>>(query.Expression);
      var sqlProvider = translatedQuery.DataSource as SqlProvider;

      if (sqlProvider==null)
        return null;

      var sessionHandler = Session.Handler as SessionHandler;
      if (sessionHandler == null)
        return null;

      var factory = sessionHandler.CommandPartFactory;
      var part = factory.CreateQueryCommandPart(
        new SqlQueryTask(sqlProvider.Request),
        CommandProcessor.DefaultParameterNamePrefix);
      return part;
    }


    // Constructors

    [ServiceConstructor]
    public QueryFormatter(Session session)
      : base(session)
    {
    }
  }
}