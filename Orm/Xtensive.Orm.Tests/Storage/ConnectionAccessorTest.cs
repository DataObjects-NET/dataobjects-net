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

  public class PerfCheckAccessor1 : DbConnectionAccessor { }
  public class PerfCheckAccessor2 : DbConnectionAccessor { }
  public class PerfCheckAccessor3 : DbConnectionAccessor { }
  public class PerfCheckAccessor4 : DbConnectionAccessor { }
  public class PerfCheckAccessor5 : DbConnectionAccessor { }
  public class PerfCheckAccessor6 : DbConnectionAccessor { }
  public class PerfCheckAccessor7 : DbConnectionAccessor { }
  public class PerfCheckAccessor8 : DbConnectionAccessor { }
  public class PerfCheckAccessor9 : DbConnectionAccessor { }
  public class PerfCheckAccessor10 : DbConnectionAccessor { }
  public class PerfCheckAccessor11 : DbConnectionAccessor { }
  public class PerfCheckAccessor12 : DbConnectionAccessor { }
  public class PerfCheckAccessor13 : DbConnectionAccessor { }
  public class PerfCheckAccessor14 : DbConnectionAccessor { }
  public class PerfCheckAccessor15 : DbConnectionAccessor { }
  public class PerfCheckAccessor16 : DbConnectionAccessor { }
  public class PerfCheckAccessor17 : DbConnectionAccessor { }
  public class PerfCheckAccessor18 : DbConnectionAccessor { }
  public class PerfCheckAccessor19 : DbConnectionAccessor { }
  public class PerfCheckAccessor20 : DbConnectionAccessor { }
  public class PerfCheckAccessor21 : DbConnectionAccessor { }
  public class PerfCheckAccessor22 : DbConnectionAccessor { }
  public class PerfCheckAccessor23 : DbConnectionAccessor { }
  public class PerfCheckAccessor24 : DbConnectionAccessor { }
  public class PerfCheckAccessor25 : DbConnectionAccessor { }

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
    public void NonPublicDefaultConstructorTest()
    {
      var domainConfig = DomainConfigurationFactory.Create();
      domainConfig.UpgradeMode = DomainUpgradeMode.Recreate;
      domainConfig.Types.Register(typeof(DummyEntity));
      domainConfig.Types.Register(typeof(NonPublicDefaultConstructorAccessor));

      using var domain = Domain.Build(domainConfig);
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
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void ConnectionExtensionExistanceTest(int amountOfAccessors)
    {
      var domainConfig = DomainConfigurationFactory.Create();
      domainConfig.UpgradeMode = DomainUpgradeMode.Recreate;

      foreach (var accessor in GetAccessors(amountOfAccessors)) {
        domainConfig.Types.Register(accessor);
      }

      using (var domain = Domain.Build(domainConfig))
      using (var session = domain.OpenSession()) {
        var nativeHandler = (SqlSessionHandler) session.Handler;
        var extensions = nativeHandler.Connection.Extensions;
        if (amountOfAccessors > 0) {
          Assert.That(extensions.Count, Is.EqualTo(1));
          var extension = extensions.Get<DbConnectionAccessorExtension>();
          Assert.That(extension, Is.Not.Null);
          Assert.That(extension.Accessors.Count, Is.EqualTo(amountOfAccessors));
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
        typeof(PerfCheckAccessor1), typeof(PerfCheckAccessor2), typeof(PerfCheckAccessor3), typeof(PerfCheckAccessor4),
        typeof(PerfCheckAccessor5), typeof(PerfCheckAccessor6), typeof(PerfCheckAccessor7), typeof(PerfCheckAccessor8),
        typeof(PerfCheckAccessor9), typeof(PerfCheckAccessor10), typeof(PerfCheckAccessor11), typeof(PerfCheckAccessor12),
        typeof(PerfCheckAccessor13), typeof(PerfCheckAccessor14), typeof(PerfCheckAccessor15), typeof(PerfCheckAccessor16),
        typeof(PerfCheckAccessor17), typeof(PerfCheckAccessor18), typeof(PerfCheckAccessor19), typeof(PerfCheckAccessor20),
        typeof(PerfCheckAccessor21), typeof(PerfCheckAccessor22), typeof(PerfCheckAccessor23), typeof(PerfCheckAccessor24),
        typeof(PerfCheckAccessor25)
      };
      for (var i = 0; i < neededCount; i++) {
        yield return all[i];
      }
    }
  }
}
