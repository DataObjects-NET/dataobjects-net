// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.03.25

using Xtensive.Orm;

namespace Xtensive.Orm.Providers.MySql
{
  /// <summary>
  /// Storage provider for MySql.
  /// </summary>
  [Provider(WellKnown.Provider.MySql, typeof (Xtensive.Sql.Drivers.MySql.DriverFactory))]
  public class HandlerFactory : Providers.HandlerFactory
  {
  }
}