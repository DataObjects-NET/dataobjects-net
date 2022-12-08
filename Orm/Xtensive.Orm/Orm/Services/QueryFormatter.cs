// Copyright (C) 2011-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2011.03.02

using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Services
{
  /// <summary>
  /// Transforms LINQ queries into various representations.
  /// </summary>
  [Service(typeof (QueryFormatter), Singleton = true)]
  public sealed class QueryFormatter : SessionBound, ISessionService
  {
    private readonly CommandFactory commandFactory;

    /// <summary>
    /// Formats the specified <paramref name="query"/> in SQL.
    /// </summary>
    /// <typeparam name="T">Type of element.</typeparam>
    /// <param name="query">The query to format.</param>
    /// <returns>A string containing formatted query.</returns>
    public string ToSqlString<T>(IQueryable<T> query)
    {
      ArgumentValidator.EnsureArgumentNotNull(query, "query");

      var part = GetCommandPart(query);
      return part!=null ? part.Statement : string.Empty;
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

      var part = GetCommandPart(query);
      if (part==null)
        return null;

      var command = commandFactory.CreateCommand();
      command.AddPart(part);
      return command.Prepare();
    }

    private CommandPart GetCommandPart<T>(IQueryable<T> query)
    {
      var compilerConfiguration = Session.CompilationService.CreateConfiguration(Session);
      compilerConfiguration.PreferTypeIdAsParameter = false;

      var translatedQuery = Session.Query.Provider.Translate(query.Expression, compilerConfiguration);
      var sqlProvider = translatedQuery.DataSource as SqlProvider;

      if (sqlProvider==null)
        return null;

      return commandFactory.CreateQueryPart(sqlProvider.Request, null);
    }


    // Constructors

    /// <inheritdoc/>
    [ServiceConstructor]
    public QueryFormatter(Session session)
      : base(session)
    {
      var sqlSessionHandler = (SqlSessionHandler) session.Handler.GetRealHandler();
      commandFactory = sqlSessionHandler.CommandFactory;
    }
  }
}