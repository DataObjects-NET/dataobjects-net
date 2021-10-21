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
using Xtensive.Orm.Tests.Storage.ConnectionAccessorsModel;
using System.Threading.Tasks;

namespace Xtensive.Orm.Tests.Storage.ConnectionAccessorsModel
{
  public class MyConnectionAccessor : DbConnectionAccessor
  {
    private Guid instanceMarker;

    public readonly Guid UniqueInstanceIdentifier;

    public int ConnectionOpeningCounter;
    public int ConnectionInitializationCounter;
    public int ConnectionOpenedCounter;
    public int ConnectionOpeningFailedCounter;

    public override void ConnectionOpening(ConnectionEventData eventData)
    {
      instanceMarker = UniqueInstanceIdentifier;
      ConnectionOpeningCounter++;
    }

    public override void ConnectionInitialization(ConnectionInitEventData eventData)
    {
      ConnectionInitializationCounter++;
      if (instanceMarker != UniqueInstanceIdentifier) {
        throw new Exception("Not the same instance");
      }
    }

    public override void ConnectionOpened(ConnectionEventData eventData)
    {
      ConnectionOpenedCounter++;
      if (instanceMarker != UniqueInstanceIdentifier) {
        throw new Exception("Not the same instance");
      }
    }

    public override void ConnectionOpeningFailed(ConnectionErrorEventData eventData)
    {
      ConnectionOpeningFailedCounter++;
      if (instanceMarker != UniqueInstanceIdentifier) {
        throw new Exception("Not the same instance");
      }
    }

    public MyConnectionAccessor()
    {
      UniqueInstanceIdentifier = Guid.NewGuid();
    }
  }

  public class NoDefaultConstructorAccessor : DbConnectionAccessor
  {
#pragma warning disable IDE0060 // Remove unused parameter
    public NoDefaultConstructorAccessor(int dummyParameter)
#pragma warning restore IDE0060 // Remove unused parameter
    {
    }
  }

  public class NonPublicDefaultConstructorAccessor : DbConnectionAccessor
  {
    private NonPublicDefaultConstructorAccessor()
    {
    }
  }

  #region Performance Test accessors

  public class PerfAccessor1 : DbConnectionAccessor { }
  public class PerfAccessor2 : DbConnectionAccessor { }
  public class PerfAccessor3 : DbConnectionAccessor { }
  public class PerfAccessor4 : DbConnectionAccessor { }
  public class PerfAccessor5 : DbConnectionAccessor { }
  public class PerfAccessor6 : DbConnectionAccessor { }
  public class PerfAccessor7 : DbConnectionAccessor { }
  public class PerfAccessor8 : DbConnectionAccessor { }
  public class PerfAccessor9 : DbConnectionAccessor { }
  public class PerfAccessor10 : DbConnectionAccessor { }
  public class PerfAccessor11 : DbConnectionAccessor { }
  public class PerfAccessor12 : DbConnectionAccessor { }
  public class PerfAccessor13 : DbConnectionAccessor { }
  public class PerfAccessor14 : DbConnectionAccessor { }
  public class PerfAccessor15 : DbConnectionAccessor { }
  public class PerfAccessor16 : DbConnectionAccessor { }
  public class PerfAccessor17 : DbConnectionAccessor { }
  public class PerfAccessor18 : DbConnectionAccessor { }
  public class PerfAccessor19 : DbConnectionAccessor { }
  public class PerfAccessor20 : DbConnectionAccessor { }
  public class PerfAccessor21 : DbConnectionAccessor { }
  public class PerfAccessor22 : DbConnectionAccessor { }
  public class PerfAccessor23 : DbConnectionAccessor { }
  public class PerfAccessor24 : DbConnectionAccessor { }
  public class PerfAccessor25 : DbConnectionAccessor { }

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
  [TestFixture]
  public sealed class ConnectionAccessorTest
  {
    [Test]
    public void DomainRegistryTest()
    {
      var domainConfig = DomainConfigurationFactory.Create();
      domainConfig.Types.Register(typeof(DummyEntity));
      domainConfig.Types.Register(typeof(MyConnectionAccessor));

      Assert.That(domainConfig.Types.DbConnectionAccessors.Count(), Is.EqualTo(1));
    }

    [Test]
    public void NoDefaultConstructorTest()
    {
      var domainConfig = DomainConfigurationFactory.Create();
      domainConfig.UpgradeMode = DomainUpgradeMode.Recreate;
      domainConfig.Types.Register(typeof(DummyEntity));
      domainConfig.Types.Register(typeof(NoDefaultConstructorAccessor));

      Domain domain = null;
      _ = Assert.Throws<NotSupportedException>(() => domain = Domain.Build(domainConfig));
      domain.DisposeSafely();
    }

    [Test]
    public void NoDefaultConstructorAsyncTest()
    {
      var domainConfig = DomainConfigurationFactory.Create();
      domainConfig.UpgradeMode = DomainUpgradeMode.Recreate;
      domainConfig.Types.Register(typeof(DummyEntity));
      domainConfig.Types.Register(typeof(NoDefaultConstructorAccessor));

      Domain domain = null;
      _ = Assert.ThrowsAsync<NotSupportedException>(async () => domain = await Domain.BuildAsync(domainConfig));
      domain.DisposeSafely();
    }

    [Test]
    public void NonPublicDefaultConstructorTest()
    {
      var domainConfig = DomainConfigurationFactory.Create();
      domainConfig.UpgradeMode = DomainUpgradeMode.Recreate;
      domainConfig.Types.Register(typeof(DummyEntity));
      domainConfig.Types.Register(typeof(NonPublicDefaultConstructorAccessor));

      using var domain = Domain.Build(domainConfig);
    }

    [Test]
    public async Task NonPublicDefaultConstructorAsyncTest()
    {
      var domainConfig = DomainConfigurationFactory.Create();
      domainConfig.UpgradeMode = DomainUpgradeMode.Recreate;
      domainConfig.Types.Register(typeof(DummyEntity));
      domainConfig.Types.Register(typeof(NonPublicDefaultConstructorAccessor));

      await using var domain = await Domain.BuildAsync(domainConfig);
    }

    [Test]
    public void SessionConnectionAccessorsTest()
    {
      var domainConfig = DomainConfigurationFactory.Create();
      domainConfig.UpgradeMode = DomainUpgradeMode.Recreate;
      domainConfig.Types.Register(typeof(DummyEntity));
      domainConfig.Types.Register(typeof(MyConnectionAccessor));

      Guid? first = null;
      using (var domain = Domain.Build(domainConfig))
      using (var session = domain.OpenSession()) {
        var nativeHandler = (SqlSessionHandler) session.Handler;
        var extension = nativeHandler.Connection.Extensions.Get<DbConnectionAccessorExtension>();
        var accessorInstance = (MyConnectionAccessor) extension.Accessors.First();
        Assert.That(accessorInstance.ConnectionOpeningCounter, Is.Not.EqualTo(0));
        Assert.That(accessorInstance.ConnectionOpenedCounter, Is.Not.EqualTo(0));
        first = accessorInstance.UniqueInstanceIdentifier;
      }

      Guid? second = null;
      using (var domain = Domain.Build(domainConfig))
      using (var session = domain.OpenSession()) {
        var nativeHandler = (SqlSessionHandler) session.Handler;
        var extension = nativeHandler.Connection.Extensions.Get<DbConnectionAccessorExtension>();
        var accessorInstance = (MyConnectionAccessor) extension.Accessors.First();
        Assert.That(accessorInstance.ConnectionOpeningCounter, Is.Not.EqualTo(0));
        Assert.That(accessorInstance.ConnectionOpenedCounter, Is.Not.EqualTo(0));
        second = accessorInstance.UniqueInstanceIdentifier;
      }

      Assert.That(first != null && second != null && first != second, Is.True);
    }

    [Test]
    public async Task SessionConnectionAccessorsAsyncTest()
    {
      var domainConfig = DomainConfigurationFactory.Create();
      domainConfig.UpgradeMode = DomainUpgradeMode.Recreate;
      domainConfig.Types.Register(typeof(DummyEntity));
      domainConfig.Types.Register(typeof(MyConnectionAccessor));

      Guid? first = null;
      await using (var domain = await Domain.BuildAsync(domainConfig))
      await using (var session = await domain.OpenSessionAsync()) {
        var nativeHandler = (SqlSessionHandler) session.Handler;
        var extension = nativeHandler.Connection.Extensions.Get<DbConnectionAccessorExtension>();
        var accessorInstance = (MyConnectionAccessor) extension.Accessors.First();
        Assert.That(accessorInstance.ConnectionOpeningCounter, Is.Not.EqualTo(0));
        Assert.That(accessorInstance.ConnectionOpenedCounter, Is.Not.EqualTo(0));
        first = accessorInstance.UniqueInstanceIdentifier;
      }

      Guid? second = null;
      await using (var domain = await Domain.BuildAsync(domainConfig))
      await using (var session = await domain.OpenSessionAsync()) {
        var nativeHandler = (SqlSessionHandler) session.Handler;
        var extension = nativeHandler.Connection.Extensions.Get<DbConnectionAccessorExtension>();
        var accessorInstance = (MyConnectionAccessor) extension.Accessors.First();
        Assert.That(accessorInstance.ConnectionOpeningCounter, Is.Not.EqualTo(0));
        Assert.That(accessorInstance.ConnectionOpenedCounter, Is.Not.EqualTo(0));
        second = accessorInstance.UniqueInstanceIdentifier;
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

      foreach (var accessor in GetAccessors(includeHandlersCount)) {
        domainConfig.Types.Register(accessor);
      }

      using (var domain = Domain.Build(domainConfig))
      using (var session = domain.OpenSession()) {
        var nativeHandler = (SqlSessionHandler) session.Handler;
        var extensions = nativeHandler.Connection.Extensions;
        if (includeHandlersCount > 0) {
          Assert.That(extensions.Count, Is.EqualTo(1));
          var extension = extensions.Get<DbConnectionAccessorExtension>();
          Assert.That(extension, Is.Not.Null);
          Assert.That(extension.Accessors.Count, Is.EqualTo(includeHandlersCount));
        }
        else {
          Assert.That(extensions.Count, Is.EqualTo(0));
        }
      }
    }

    [Test]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public async Task ConnectionExtensionExistanceAsyncTest(int amoundOtAccessors)
    {
      var domainConfig = DomainConfigurationFactory.Create();
      domainConfig.UpgradeMode = DomainUpgradeMode.Recreate;

      foreach (var accessor in GetAccessors(amoundOtAccessors)) {
        domainConfig.Types.Register(accessor);
      }

      await using (var domain = await Domain.BuildAsync(domainConfig))
      await using (var session = await domain.OpenSessionAsync()) {
        var nativeHandler = (SqlSessionHandler) session.Handler;
        var extensions = nativeHandler.Connection.Extensions;
        if (amoundOtAccessors > 0) {
          Assert.That(extensions.Count, Is.EqualTo(1));
          var extension = extensions.Get<DbConnectionAccessorExtension>();
          Assert.That(extension, Is.Not.Null);
          Assert.That(extension.Accessors.Count, Is.EqualTo(amoundOtAccessors));
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
    public void SessionOpeningPerformanceTest(int amountOfAccessors)
    {
      var domainConfig = DomainConfigurationFactory.Create();
      domainConfig.UpgradeMode = DomainUpgradeMode.Recreate;

      foreach (var accessor in GetAccessors(amountOfAccessors)) {
        domainConfig.Types.Register(accessor);
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

    private IEnumerable<Type> GetAccessors(int neededCount)
    {
      if (neededCount > 25) {
        throw new Exception();
      }

      var all = new Type[] {
        typeof(PerfAccessor1), typeof(PerfAccessor2), typeof(PerfAccessor3), typeof(PerfAccessor4),
        typeof(PerfAccessor5), typeof(PerfAccessor6), typeof(PerfAccessor7), typeof(PerfAccessor8),
        typeof(PerfAccessor9), typeof(PerfAccessor10), typeof(PerfAccessor11), typeof(PerfAccessor12),
        typeof(PerfAccessor13), typeof(PerfAccessor14), typeof(PerfAccessor15), typeof(PerfAccessor16),
        typeof(PerfAccessor17), typeof(PerfAccessor18), typeof(PerfAccessor19), typeof(PerfAccessor20),
        typeof(PerfAccessor21), typeof(PerfAccessor22), typeof(PerfAccessor23), typeof(PerfAccessor24),
        typeof(PerfAccessor25)
      };
      for (var i = 0; i < neededCount; i++) {
        yield return all[i];
      }
    }
  }
}
