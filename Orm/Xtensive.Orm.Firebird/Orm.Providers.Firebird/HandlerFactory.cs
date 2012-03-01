// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.01.27

using Xtensive.Orm;

namespace Xtensive.Orm.Providers.Firebird
{
  /// <summary>
  /// Storage provider for Firebird.
  /// </summary>
  [Provider(WellKnown.Provider.Firebird, typeof (Xtensive.Sql.Drivers.Firebird.DriverFactory))]
  public class HandlerFactory : Sql.HandlerFactory
  {
  }
}