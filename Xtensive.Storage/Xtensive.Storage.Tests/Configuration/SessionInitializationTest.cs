// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.13

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Core.Caching;
using Xtensive.Core.Disposable;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Configuration
{
  [TestFixture]
  public class SessionInitializationTest
  {

    [Test]
    public void TestSessionCache()
    {
      string url = "memory://localhost/";
      //Default CacheType
      var config = new DomainConfiguration(url);
      TestCacheType(config, typeof(LruCache<,>));
      //Lru CacheType
      config = new DomainConfiguration(url);
      config.Sessions.Add(new SessionConfiguration {CacheType = SessionCacheType.LruWeak});
      TestCacheType(config, typeof(LruCache<,>));
      //Infinite CacheType
      config = new DomainConfiguration(url);
      config.Sessions.Add(new SessionConfiguration{CacheType = SessionCacheType.Infinite});
      TestCacheType(config, typeof(InfiniteCache<,>));
    }

    public void TestCacheType(DomainConfiguration config, Type expectedType)
    {
      Domain d = Domain.Build(config);
      using (SessionConsumptionScope s = d.OpenSession())
      {
        Type cacheType = s.Session.EntityStateCache.GetType();
        Log.Debug("Session CacheType: {0}", cacheType.Name);
        Assert.IsTrue(cacheType.IsOfGenericType(expectedType));
      }
      d.DisposeSafely();
    }


  }
}