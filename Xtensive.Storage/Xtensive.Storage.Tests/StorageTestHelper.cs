// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.12.17

using System;
using NUnit.Framework;

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
      var indexHandler = handler as Providers.Index.SessionHandler;
      if (indexHandler!=null)
        return indexHandler.StorageView;
      throw new InvalidOperationException();
    }

    public static StorageProvider ParseProvider(string provider)
    {
      switch (provider.ToLowerInvariant()) {
      case WellKnown.Provider.Memory:
        return StorageProvider.Memory;
      case WellKnown.Provider.SqlServer:
        return StorageProvider.SqlServer;
      case WellKnown.Provider.SqlServerCe:
        return StorageProvider.SqlServerCe;
      case WellKnown.Provider.PostgreSql:
        return StorageProvider.PostgreSql;
      case WellKnown.Provider.Oracle:
        return  StorageProvider.Oracle;
      default:
        throw new ArgumentOutOfRangeException("provider");
      }
    }

    public static void EnsureProviderIs(string activeProvider, StorageProvider allowedProviders)
    {
      EnsureProviderIs(ParseProvider(activeProvider), allowedProviders);
    }

    public static void EnsureProviderIs(StorageProvider activeProvider, StorageProvider allowedProviders)
    {
      if ((activeProvider & allowedProviders)==0)
        throw new IgnoreException(
          string.Format("This test is not suitable for '{0}' provider", activeProvider)); 
    }
  }
}