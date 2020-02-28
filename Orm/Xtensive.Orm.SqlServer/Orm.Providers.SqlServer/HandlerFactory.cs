// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.07.04

namespace Xtensive.Orm.Providers.SqlServer
{
  /// <summary>
  /// Storage provider for Microsoft SQL Server.
  /// </summary>
  [Provider(WellKnown.Provider.SqlServer, typeof (Xtensive.Sql.Drivers.SqlServer.DriverFactory))]
  public class HandlerFactory : Providers.HandlerFactory
  {
  }
}