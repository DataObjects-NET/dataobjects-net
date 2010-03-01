// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.18

using Xtensive.Sql;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// A command part factory that wraps queries into "open cursor" statements
  /// to return multiple query results from single batch.
  /// Currently this is very Oracle specific.
  /// </summary>
  public class CursorCommandPartFactory : CommandPartFactory
  {
    private const string CursorParameterNameFormat = "{0}c";
    private const string StatementFormat = "OPEN :{0} FOR {1}";

    public override CommandPart CreateQueryCommandPart(SqlQueryTask task, string parameterNamePrefix)
    {
      var parameterName = string.Format(CursorParameterNameFormat, parameterNamePrefix);
      var part = base.CreateQueryCommandPart(task, parameterNamePrefix);
      part.Query = string.Format(StatementFormat, parameterName, part.Query);
      var parameter = connection.CreateCursorParameter();
      parameter.ParameterName = parameterName;
      part.Parameters.Add(parameter);
      return part;
    }


    // Constructors

    public CursorCommandPartFactory(DomainHandler domainHandler, SqlConnection connection)
      : base(domainHandler, connection)
    {
    }
  }
}