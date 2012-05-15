// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.18

using Xtensive.Parameters;
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
    private const string CursorParameterNameFormat = "{0}c";
    private const string StatementFormat = "OPEN :{0} FOR {1}";

    public override CommandPart CreateQueryPart(IQueryRequest request, string parameterNamePrefix, ParameterContext parameterContext)
    {
      var part = base.CreateQueryPart(request, parameterNamePrefix, parameterContext);
      var parameterName = string.Format(CursorParameterNameFormat, parameterNamePrefix);
      part.Statement = string.Format(StatementFormat, parameterName, part.Statement);
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