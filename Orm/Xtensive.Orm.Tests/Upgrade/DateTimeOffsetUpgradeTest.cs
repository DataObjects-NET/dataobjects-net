// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Groznov
// Created:    2016.08.31

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Upgrade.DateTimeOffsetUpgradeModel;
using FirstModel = Xtensive.Orm.Tests.Upgrade.DateTimeOffsetUpgradeModel.FirstModel;
using SecondModel = Xtensive.Orm.Tests.Upgrade.DateTimeOffsetUpgradeModel.SecondModel;

namespace Xtensive.Orm.Tests.Upgrade
{
  namespace DateTimeOffsetUpgradeModel
  {
    public interface ITestEntity : IEntity
    {
      DateTimeOffset FirstDateTimeOffset { get; }
    }

    namespace FirstModel
    {
      [HierarchyRoot]
      public class TestEntity : Entity, ITestEntity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public DateTimeOffset FirstDateTimeOffset { get; set; }
      }
    }

    namespace SecondModel
    {
      [HierarchyRoot]
      public class TestEntity : Entity, ITestEntity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public DateTimeOffset FirstDateTimeOffset { get; set; }

        [Field]
        public DateTimeOffset SecondDateTimeOffset { get; set; }
      }
    }
  }

  [TestFixture]
  public class DateTimeOffsetUpgradeTest
  {
    private readonly DateTimeOffset defaultDateTimeOffset = new DateTimeOffset(2016, 01, 02, 13, 14, 15, TimeSpan.FromHours(8)); // TimeSpan.FromMinutes(-170));

    [Test]
    public void ValidateWithoutChangesTest()
    {
      TestWithoutChanges(DomainUpgradeMode.Validate);
    }

    [Test]
    public void PerformSafelyWithoutChangesTest()
    {
      TestWithoutChanges(DomainUpgradeMode.PerformSafely);
    }

    [Test]
    public void PerformWithoutChangesTest()
    {
      TestWithoutChanges(DomainUpgradeMode.Perform);
    }

    [Test]
    public void ValidateWithChanges()
    {
      TestWithChanges(DomainUpgradeMode.Validate, true);
    }

    [Test]
    public void PerformSafelyWithChanges()
    {
      TestWithChanges(DomainUpgradeMode.PerformSafely, false);
    }

    [Test]
    public void PerformWithChanges()
    {
      TestWithChanges(DomainUpgradeMode.Perform, false);
    }

    [TestFixtureSetUp]
    public void FixtureSetUp()
    {
      Require.AllFeaturesSupported(ProviderFeatures.DateTimeOffset | ProviderFeatures.DateTimeOffsetEmulation);
    }

    private void TestWithoutChanges(DomainUpgradeMode secondBuildUpgradeMode)
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, typeof (FirstModel.TestEntity));
      using (var domain = Domain.Build(configuration))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new FirstModel.TestEntity { FirstDateTimeOffset = defaultDateTimeOffset };
        tx.Complete();
      }

      configuration = BuildConfiguration(secondBuildUpgradeMode, typeof (FirstModel.TestEntity));
      using (var domain = Domain.Build(configuration))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        CheckTestEntity<FirstModel.TestEntity>();
        tx.Complete();
      }
    }

    private void TestWithChanges(DomainUpgradeMode secondBuildUpgradeMode, bool isExceptionExpected)
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, typeof (FirstModel.TestEntity));
      using (var domain = Domain.Build(configuration))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new FirstModel.TestEntity { FirstDateTimeOffset = defaultDateTimeOffset };
        tx.Complete();
      }

      configuration = BuildConfiguration(secondBuildUpgradeMode, typeof (SecondModel.TestEntity));

      Action buildAction = () => {
        using (var domain = Domain.Build(configuration))
        using (var session = domain.OpenSession())
        using (var tx = session.OpenTransaction()) {
          CheckTestEntity<SecondModel.TestEntity>();
          tx.Complete();
        }
      };

      if (isExceptionExpected)
        Assert.Throws<SchemaSynchronizationException>(() => buildAction());
      else
        buildAction();
    }

    private DomainConfiguration BuildConfiguration(DomainUpgradeMode upgradeMode, Type type)
    {
      var configruation = DomainConfigurationFactory.Create();
      configruation.UpgradeMode = upgradeMode;
      configruation.Types.Register(type);
      return configruation;
    }

    private void CheckTestEntity<T>()
      where T : Entity, ITestEntity
    {
      var entity = Query.All<T>().FirstOrDefault();
      Assert.IsNotNull(entity);
      var k = entity.FirstDateTimeOffset;
      var r = k.Year;
      Assert.AreEqual(entity.FirstDateTimeOffset, defaultDateTimeOffset);
    }
  }
}
