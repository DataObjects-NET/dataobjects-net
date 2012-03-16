// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.03.25

using Xtensive.Orm;

namespace Xtensive.Orm.Providers.SQLite
{
  /// <summary>
  /// Storage provider for Sqlite.
  /// </summary>
  [Provider(WellKnown.Provider.Sqlite, "Storage provider for Sqlite.")]
  public class HandlerFactory : Sql.HandlerFactory
  {
  }
}