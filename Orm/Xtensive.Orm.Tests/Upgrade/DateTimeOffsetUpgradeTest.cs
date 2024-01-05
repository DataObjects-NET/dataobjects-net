// Copyright (C) 2016-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Groznov
// Created:    2016.08.31

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

    [OneTimeSetUp]
    public void FixtureSetUp() => Require.AnyFeatureSupported(ProviderFeatures.DateTimeOffset | ProviderFeatures.DateTimeOffsetEmulation);


    [Test]
    public void ValidateWithoutChangesTest() =>
      TestWithoutChanges(DomainUpgradeMode.Validate);

    [Test]
    public async Task ValidateWithoutChangesAsyncTest() =>
      await TestWithoutChangesAsync(DomainUpgradeMode.Validate);

    [Test]
    public void PerformSafelyWithoutChangesTest() =>
      TestWithoutChanges(DomainUpgradeMode.PerformSafely);

    [Test]
    public async Task PerformSafelyWithoutChangesAsyncTest() =>
      await TestWithoutChangesAsync(DomainUpgradeMode.PerformSafely);

    [Test]
    public void PerformWithoutChangesTest() =>
      TestWithoutChanges(DomainUpgradeMode.Perform);

    [Test]
    public async Task PerformWithoutChangesAsyncTest() =>
      await TestWithoutChangesAsync(DomainUpgradeMode.Perform);

    [Test]
    public void ValidateWithChangesTest() =>
      TestWithChanges(DomainUpgradeMode.Validate, true);

    [Test]
    public async Task ValidateWithChangesAsyncTest() =>
      await TestWithChangesAsync(DomainUpgradeMode.Validate, true);

    [Test]
    public void PerformSafelyWithChangesTest() =>
      TestWithChanges(DomainUpgradeMode.PerformSafely, false);

    [Test]
    public async Task PerformSafelyWithChangesAsyncTest() =>
      await TestWithChangesAsync(DomainUpgradeMode.PerformSafely, false);

    [Test]
    public void PerformWithChangesTest() =>
      TestWithChanges(DomainUpgradeMode.Perform, false);

    [Test]
    public async Task PerformWithChangesAsyncTest() =>
      await TestWithChangesAsync(DomainUpgradeMode.Perform, false);

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

    private async Task TestWithoutChangesAsync(DomainUpgradeMode secondBuildUpgradeMode)
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, typeof(FirstModel.TestEntity));
      using (var domain = Domain.Build(configuration))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new FirstModel.TestEntity { FirstDateTimeOffset = defaultDateTimeOffset };
        tx.Complete();
      }

      configuration = BuildConfiguration(secondBuildUpgradeMode, typeof(FirstModel.TestEntity));
      using (var domain = await Domain.BuildAsync(configuration))
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

      configuration = BuildConfiguration(secondBuildUpgradeMode, typeof(SecondModel.TestEntity));
      if (isExceptionExpected) {
        _ = Assert.ThrowsAsync<SchemaSynchronizationException>(async () => await Domain.BuildAsync(configuration));
      }
      else {
        using (var domain = Domain.Build(configuration))
        using (var session = domain.OpenSession())
        using (var tx = session.OpenTransaction()) {
          CheckTestEntity<SecondModel.TestEntity>();
          tx.Complete();
        }
      }
    }

    private async Task TestWithChangesAsync(DomainUpgradeMode secondBuildUpgradeMode, bool isExceptionExpected)
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Recreate, typeof(FirstModel.TestEntity));

      using (var domain = Domain.Build(configuration))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new FirstModel.TestEntity { FirstDateTimeOffset = defaultDateTimeOffset };
        tx.Complete();
      }

      configuration = BuildConfiguration(secondBuildUpgradeMode, typeof(SecondModel.TestEntity));
      if (isExceptionExpected) {
        _ = Assert.ThrowsAsync<SchemaSynchronizationException>(async () => await Domain.BuildAsync(configuration));
      }
      else {
        using (var domain = await Domain.BuildAsync(configuration))
        using (var session = domain.OpenSession())
        using (var tx = session.OpenTransaction()) {
          CheckTestEntity<SecondModel.TestEntity>();
          tx.Complete();
        }
      }
    }

    private DomainConfiguration BuildConfiguration(DomainUpgradeMode upgradeMode, Type type)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(type);
      var providerInfo = StorageProviderInfo.Instance.Info;
      if (providerInfo.ProviderName == WellKnown.Provider.PostgreSql) {
        var localZone = DateTimeOffset.Now.ToLocalTime().Offset;
        var localZoneString = ((localZone < TimeSpan.Zero) ? "-" : "+") + localZone.ToString(@"hh\:mm");
        configuration.ConnectionInitializationSql = string.Format("SET TIME ZONE INTERVAL '{0}' HOUR TO MINUTE", localZoneString);
      }
      return configuration;
    }

    private void CheckTestEntity<T>()
      where T : Entity, ITestEntity
    {
      var entity = Query.All<T>().FirstOrDefault();
      Assert.IsNotNull(entity);
      var k = entity.FirstDateTimeOffset;
      var r = k.Year;
      var localDefaultDateTimeOffset = TryMoveToLocalTimeZone(defaultDateTimeOffset);
      Assert.AreEqual(entity.FirstDateTimeOffset, localDefaultDateTimeOffset);
    }

    private DateTimeOffset TryMoveToLocalTimeZone(DateTimeOffset dateTimeOffset)
    {
      var providerInfo = StorageProviderInfo.Instance.Info;
      if (providerInfo.ProviderName == WellKnown.Provider.PostgreSql) {
        return dateTimeOffset.ToLocalTime();
      }
      return dateTimeOffset;
    }
  }
}
