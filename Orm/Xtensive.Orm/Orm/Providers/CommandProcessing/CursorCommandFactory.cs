// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
    public override CommandPart CreateQueryPart(IQueryRequest request, in string parameterNamePrefix, ParameterContext parameterContext)
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