// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.13

using System;
using NUnit.Framework;
using Xtensive.Core.Caching;
using Xtensive.Core.Disposing;
using Xtensive.Core.Reflection;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Configuration
{
  [TestFixture]
  public class SessionInitializationTest
  {
    [Test]
    public void TestSessionCache()
    {
      string url = "memory://localhost/DO40-Tests";
      // Default CacheType
      var dc = new DomainConfiguration(url);
      dc.UpgradeMode = DomainUpgradeMode.Recreate;
      TestCacheType(dc, typeof (LruCache<,>));
      // Lru CacheType
      dc = new DomainConfiguration(url);
      dc.Sessions.Add(new SessionConfiguration(WellKnown.Sessions.Default) {CacheType = SessionCacheType.LruWeak});
      TestCacheType(dc, typeof (LruCache<,>));
      // Infinite CacheType
      dc = new DomainConfiguration(url);
      dc.Sessions.Add(new SessionConfiguration(WellKnown.Sessions.Default) { CacheType = SessionCacheType.Infinite });
      TestCacheType(dc, typeof (InfiniteCache<,>));
    }

    public void TestCacheType(DomainConfiguration config, Type expectedType)
    {
      var d = Domain.Build(config);
      using (var s = Session.Open(d)) {
        var cacheType = s.EntityStateCache.GetType();
        Log.Debug("Session CacheType: {0}", cacheType.Name);
        Assert.IsTrue(cacheType.IsOfGenericType(expectedType));
      }
      d.DisposeSafely();
    }

    [Test]
    public void TestNamedConfigurations()
    {
      string url = "memory://localhost/DO40-Tests";
      var config = new DomainConfiguration(url);
      AssertEx.ThrowsArgumentNullException(() => config.Sessions.Add(new SessionConfiguration()));
      config.Sessions.Add(new SessionConfiguration("SomeName"));
      AssertEx.ThrowsInvalidOperationException(() => config.Sessions.Add(new SessionConfiguration("SomeName")));
    }
  }
}