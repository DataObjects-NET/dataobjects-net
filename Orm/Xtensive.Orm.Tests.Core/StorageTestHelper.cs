// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.12.17

using System;
using Xtensive.Orm.Providers;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests
{
  public static class StorageTestHelper
  {
    public static bool IsFetched(Session session, Key key)
    {
      EntityState dummy;
      return session.EntityStateCache.TryGetItem(key, false, out dummy);
    }

    public static object GetNativeTransaction()
    {
      var handler = Session.Demand().Handler;
      var sqlHandler = handler as SqlSessionHandler;
      if (sqlHandler!=null)
        return sqlHandler.Connection.ActiveTransaction;
      throw new NotSupportedException();
    }

    public static Schema GetDefaultSchema(Domain domain)
    {
      return domain.Handler.Mapping[domain.Model.Types[typeof (Metadata.Assembly)]].Schema;
    }
  }
}