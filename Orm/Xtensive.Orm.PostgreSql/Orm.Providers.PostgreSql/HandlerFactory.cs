// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2008.09.15

using Xtensive.Orm;

namespace Xtensive.Orm.Providers.PostgreSql
{
  /// <summary>
  /// Storage provider for PostgreSQL.
  /// </summary>
  [Provider(WellKnown.Provider.PostgreSql, typeof (Xtensive.Sql.Drivers.PostgreSql.DriverFactory))]
  public class HandlerFactory : Providers.HandlerFactory
  {
  }
}