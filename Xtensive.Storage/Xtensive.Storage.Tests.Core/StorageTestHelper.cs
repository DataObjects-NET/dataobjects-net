// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.12.17

using System;

namespace Xtensive.Storage.Tests
{
  public static class StorageTestHelper
  {
    public static object GetNativeTransaction()
    {
      var handler = Session.Demand().Handler;
      var sqlHandler = handler as Providers.Sql.SessionHandler;
      if (sqlHandler!=null)
        return sqlHandler.Connection.ActiveTransaction;
      var indexHandler = handler as Providers.Indexing.SessionHandler;
      if (indexHandler!=null)
        return indexHandler.StorageView;
      throw new InvalidOperationException();
    }
  }
}