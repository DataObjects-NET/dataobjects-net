// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.18

using Xtensive.Core;
using Xtensive.Sql;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// A command part factory that wraps queries into "open cursor" statements
  /// to return multiple query results from single batch.
  /// Currently this is very Oracle specific.
  /// </summary>
  public class CursorCommandFactory : CommandFactory
  {
    public override CommandPart CreateQueryPart(IQueryRequest request, string parameterNamePrefix, ParameterContext parameterContext)
    {
      var part = base.CreateQueryPart(request, parameterNamePrefix, parameterContext);
      var parameterName = $"{parameterNamePrefix}c";
      part.Statement = $"OPEN :{parameterName} FOR {part.Statement}";
      var parameter = Connection.CreateCursorParameter();
      parameter.ParameterName = parameterName;
      part.Parameters.Add(parameter);
      return part;
    }


    // Constructors

    public CursorCommandFactory(StorageDriver driver, Session session, SqlConnection connection)
      : base(driver, session, connection)
    {
    }
  }
}