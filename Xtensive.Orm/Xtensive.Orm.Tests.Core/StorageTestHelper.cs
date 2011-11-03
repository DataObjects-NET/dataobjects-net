// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.12.17

using System;
using Xtensive.Storage.Providers.Sql;

namespace Xtensive.Orm.Tests
{
  public static class StorageTestHelper
  {
    public const string MemoryProviderName = "memory";

    public static object GetNativeTransaction()
    {
      var handler = Session.Demand().Handler;
      var sqlHandler = handler as SessionHandler;
      if (sqlHandler!=null)
        return sqlHandler.Connection.ActiveTransaction;
      var indexHandler = handler as Storage.Providers.Indexing.SessionHandler;
      if (indexHandler!=null)
        return indexHandler.StorageView;
      throw new InvalidOperationException();
    }
  }
}