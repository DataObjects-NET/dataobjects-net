// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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