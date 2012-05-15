// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.04

using Xtensive.Orm;

namespace Xtensive.Orm.Providers.Oracle
{
  /// <summary>
  /// Storage provider for Oracle.
  /// </summary>
  [Provider(WellKnown.Provider.Oracle, typeof (Xtensive.Sql.Drivers.Oracle.DriverFactory))]
  public class HandlerFactory : Providers.HandlerFactory
  {
  }
}