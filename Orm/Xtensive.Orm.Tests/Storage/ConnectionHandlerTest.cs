// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Providers;
using Xtensive.Sql;
using Xtensive.Orm.Tests.Storage.ConnectionHandlersModel;

namespace Xtensive.Orm.Tests.Storage.ConnectionHandlersModel
{
  public class MyConnectionHandler : IConnectionHandler
  {
    private Guid instanceMarker;

    public readonly Guid UniqueInstanceIdentifier;

    public int ConnectionOpeningCounter;
    public int ConnectionInitializationCounter;
    public int ConnectionOpenedCounter;
    public int ConnectionOpeningFailedCounter;

    public void ConnectionOpening(ConnectionEventData eventData)
    {
      instanceMarker = UniqueInstanceIdentifier;
      ConnectionOpeningCounter++;
    }

    public void ConnectionInitialization(ConnectionInitEventData eventData)
    {
      ConnectionInitializationCounter++;
      if (instanceMarker != UniqueInstanceIdentifier) {
        throw new Exception("Not the same instance");
      }
    }

    public void ConnectionOpened(ConnectionEventData eventData)
    {
      ConnectionOpenedCounter++;
      if (instanceMarker != UniqueInstanceIdentifier) {
        throw new Exception("Not the same instance");
      }
    }

    public void ConnectionOpeningFailed(ConnectionErrorEventData eventData)
    {
      ConnectionOpeningFailedCounter++;
      if (instanceMarker != UniqueInstanceIdentifier) {
        throw new Exception("Not the same instance");
      }
    }

    public MyConnectionHandler()
    {
      UniqueInstanceIdentifier = Guid.NewGuid();
    }
  }

  public class NoDefaultConstructorHandler : IConnectionHandler
  {
#pragma warning disable IDE0060 // Remove unused parameter
    public NoDefaultConstructorHandler(int dummyParameter)
#pragma warning restore IDE0060 // Remove unused parameter
    {
    }
  }

  public class NonPublicDefaultConstructorHandler : IConnectionHandler
  {
    private NonPublicDefaultConstructorHandler()
    {
    }
  }

  #region Performance Test handlers

  public class PerfHandler1 : IConnectionHandler { }
  public class PerfHandler2 : IConnectionHandler { }
  public class PerfHandler3 : IConnectionHandler { }
  public class PerfHandler4 : IConnectionHandler { }
  public class PerfHandler5 : IConnectionHandler { }
  public class PerfHandler6 : IConnectionHandler { }
  public class PerfHandler7 : IConnectionHandler { }
  public class PerfHandler8 : IConnectionHandler { }
  public class PerfHandler9 : IConnectionHandler { }
  public class PerfHandler10 : IConnectionHandler { }
  public class PerfHandler11 : IConnectionHandler { }
  public class PerfHandler12 : IConnectionHandler { }
  public class PerfHandler13 : IConnectionHandler { }
  public class PerfHandler14 : IConnectionHandler { }
  public class PerfHandler15 : IConnectionHandler { }
  public class PerfHandler16 : IConnectionHandler { }
  public class PerfHandler17 : IConnectionHandler { }
  public class PerfHandler18 : IConnectionHandler { }
  public class PerfHandler19 : IConnectionHandler { }
  public class PerfHandler20 : IConnectionHandler { }
  public class PerfHandler21 : IConnectionHandler { }
  public class PerfHandler22 : IConnectionHandler { }
  public class PerfHandler23 : IConnectionHandler { }
  public class PerfHandler24 : IConnectionHandler { }
  public class PerfHandler25 : IConnectionHandler { }

  #endregion

  public static class StaticCounter
  {
    public static int OpeningReached;
    public static int OpenedReached;
  }

  public class DummyEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public int Value { get; set; }

    public DummyEntity(Session session)
      : base(session)
    {
    }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  public class ConnectionHandlerTest
  {
    [Test]
    public void DomainRegistryTest()
    {
      var domainConfig = DomainConfigurationFactory.Create();
      domainConfig.Types.Register(typeof(DummyEntity));
      domainConfig.Types.Register(typeof(MyConnectionHandler));

      Assert.That(domainConfig.Types.ConnectionHandlers.Count(), Is.EqualTo(1));
    }

    [Test]
    public void NoDefaultConstructorTest()
    {
      var domainConfig = DomainConfigurationFactory.Create();
      domainConfig.UpgradeMode = DomainUpgradeMode.Recreate;
      domainConfig.Types.Register(typeof(DummyEntity));
      domainConfig.Types.Register(typeof(NoDefaultConstructorHandler));

      Domain domain = null;
      _ = Assert.Throws<NotSupportedException>(() => domain = Domain.Build(domainConfig));
      domain.DisposeSafely();
    }

    [Test]
    public void NonPublicDefaultConstructorTest()
    {
      var domainConfig = DomainConfigurationFactory.Create();
      domainConfig.UpgradeMode = DomainUpgradeMode.Recreate;
      domainConfig.Types.Register(typeof(DummyEntity));
      domainConfig.Types.Register(typeof(NonPublicDefaultConstructorHandler));

      using var domain = Domain.Build(domainConfig);
    }

    [Test]
    public void SessionConnectionHandlersTest()
    {
      var domainConfig = DomainConfigurationFactory.Create();
      domainConfig.UpgradeMode = DomainUpgradeMode.Recreate;
      domainConfig.Types.Register(typeof(DummyEntity));
      domainConfig.Types.Register(typeof(MyConnectionHandler));

      Guid? first = null;
      using (var domain = Domain.Build(domainConfig))
      using (var session = domain.OpenSession()) {
        var nativeHandler = (SqlSessionHandler) session.Handler;
        var extension = nativeHandler.Connection.Extensions.Get<ConnectionHandlersExtension>();
        var handlerInstance = (MyConnectionHandler) extension.Handlers.First();
        Assert.That(handlerInstance.ConnectionOpeningCounter, Is.Not.EqualTo(0));
        Assert.That(handlerInstance.ConnectionOpenedCounter, Is.Not.EqualTo(0));
        first = handlerInstance.UniqueInstanceIdentifier;
      }

      Guid? second = null;
      using (var domain = Domain.Build(domainConfig))
      using (var session = domain.OpenSession()) {
        var nativeHandler = (SqlSessionHandler) session.Handler;
        var extension = nativeHandler.Connection.Extensions.Get<ConnectionHandlersExtension>();
        var handlerInstance = (MyConnectionHandler) extension.Handlers.First();
        Assert.That(handlerInstance.ConnectionOpeningCounter, Is.Not.EqualTo(0));
        Assert.That(handlerInstance.ConnectionOpenedCounter, Is.Not.EqualTo(0));
        second = handlerInstance.UniqueInstanceIdentifier;
      }

      Assert.That(first != null && second != null && first != second, Is.True);
    }

    [Test]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void ConnectionExtensionExistanceTest(int includeHandlersCount)
    {
      var domainConfig = DomainConfigurationFactory.Create();
      domainConfig.UpgradeMode = DomainUpgradeMode.Recreate;

      foreach (var handler in GetHandlers(includeHandlersCount)) {
        domainConfig.Types.Register(handler);
      }

      using (var domain = Domain.Build(domainConfig))
      using (var session = domain.OpenSession()) {
        var nativeHandler = (SqlSessionHandler) session.Handler;
        var extensions = nativeHandler.Connection.Extensions;
        if (includeHandlersCount > 0) {
          Assert.That(extensions.Count, Is.EqualTo(1));
          var extension = extensions.Get<ConnectionHandlersExtension>();
          Assert.That(extension, Is.Not.Null);
          Assert.That(extension.Handlers.Count, Is.EqualTo(includeHandlersCount));
        }
        else {
          Assert.That(extensions.Count, Is.EqualTo(0));
        }
      }
    }

    [Explicit]
    [TestCase(0)]
    [TestCase(5)]
    [TestCase(10)]
    [TestCase(15)]
    [TestCase(20)]
    [TestCase(25)]
    public void SessionOpeningPerformanceTest(int includeHandlersCount)
    {
      var domainConfig = DomainConfigurationFactory.Create();
      domainConfig.UpgradeMode = DomainUpgradeMode.Recreate;

      foreach (var handler in GetHandlers(includeHandlersCount)) {
        domainConfig.Types.Register(handler);
      }

      var watch = new Stopwatch();
      using (var domain = Domain.Build(domainConfig)) {
        watch.Start();
        for (var i = 0; i < 1000000; i++) {
          domain.OpenSession().Dispose();
        }
        watch.Stop();
      }
      Console.WriteLine(watch.ElapsedTicks / 1000000);
    }

    private IEnumerable<Type> GetHandlers(int neededCount)
    {
      if (neededCount > 25) {
        throw new Exception();
      }

      var all = new Type[] {
        typeof(PerfHandler1), typeof(PerfHandler2), typeof(PerfHandler3), typeof(PerfHandler4),
        typeof(PerfHandler5), typeof(PerfHandler6), typeof(PerfHandler7), typeof(PerfHandler8),
        typeof(PerfHandler9), typeof(PerfHandler10), typeof(PerfHandler11), typeof(PerfHandler12),
        typeof(PerfHandler13), typeof(PerfHandler14), typeof(PerfHandler15), typeof(PerfHandler16),
        typeof(PerfHandler17), typeof(PerfHandler18), typeof(PerfHandler19), typeof(PerfHandler20),
        typeof(PerfHandler21), typeof(PerfHandler22), typeof(PerfHandler23), typeof(PerfHandler24),
        typeof(PerfHandler25)
      };
      for (var i = 0; i < neededCount; i++) {
        yield return all[i];
      }
    }
  }
}