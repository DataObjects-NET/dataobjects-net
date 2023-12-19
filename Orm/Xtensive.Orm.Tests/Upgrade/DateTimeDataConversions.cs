// Copyright (C) 2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade;
using Xtensive.Orm.Tests.Upgrade.DateTimeTypesDataConversionModel;


namespace Xtensive.Orm.Tests.Upgrade
{
  [TestFixture]
  public class DateTimeTypesDataConversionTest
  {
    private readonly Dictionary<string, Type> upgradeHandlers = new();
    private readonly Dictionary<string, (Type, object)> initEntities = new();

    private readonly TimeSpan ServerTimeZone = StorageProviderInfo.Instance.TimeZoneProvider.TimeZoneOffset;
    private readonly bool RequreToServerOffset =
      StorageProviderInfo.Instance.Provider is StorageProvider.PostgreSql;

    private readonly DateTimeOffset dateTimeOffsetInitValue =
      new DateTimeOffset(new DateTime(2017, 10, 23, 22, 33, 44, 321), new TimeSpan(3, 45, 0));
    private readonly DateTime dateTimeInitValue = new DateTime(2017, 10, 23, 22,33,44, 321);
    private readonly TimeOnly timeOnlyInitValue = TimeOnly.FromTimeSpan(new TimeSpan(22, 33, 44));
    private readonly DateOnly dateOnlyInitValue = DateOnly.FromDateTime(new DateTime(2017, 10, 23));

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
      upgradeHandlers[nameof(DateTimeToDateTimeOffsetTest)] = typeof(DateTimeToDateTimeOffsetUpgradeHandler);
      upgradeHandlers[nameof(DateTimeOffsetToDateTimeTest)] = typeof(DateTimeOffsetToDateTimeUpgradeHandler);

      upgradeHandlers[nameof(DateTimeToDateOnlyTest)] = typeof(DateTimeToDateOnlyUpgradeHandler);
      upgradeHandlers[nameof(DateTimeToTimeOnlyTest)] = typeof(DateTimeToTimeOnlyUpgradeHandler);

      upgradeHandlers[nameof(DateTimeOffsetToDateOnlyTest)] = typeof(DateTimeOffsetToDateOnlyUpgradeHandler);
      upgradeHandlers[nameof(DateTimeOffsetToTimeOnlyTest)] = typeof(DateTimeOffsetToTimeOnlyUpgradeHandler);

      upgradeHandlers[nameof(DateOnlyToDateTimeTest)] = typeof(DateOnlyToDateTimeUpgradeHandler);
      upgradeHandlers[nameof(DateOnlyToDateTimeOffsetTest)] = typeof(DateOnlyToDateTimeOffsetUpgradeHandler);
      upgradeHandlers[nameof(DateOnlyToTimeOnlyTest)] = typeof(DateOnlyToTimeOnlyUpgradeHandler);

      upgradeHandlers[nameof(TimeOnlyToDateTimeTest)] = typeof(TimeOnlyToDateTimeUpgradeHandler);
      upgradeHandlers[nameof(TimeOnlyToDateTimeOffsetTest)] = typeof(TimeOnlyToDateTimeOffsetUpgradeHandler);
      upgradeHandlers[nameof(TimeOnlyToDateOnlyTest)] = typeof(TimeOnlyToDateOnlyUpgradeHandler);

      initEntities[nameof(DateTimeToDateTimeOffsetTest)] = (typeof(DateTimeEntity), dateTimeInitValue);
      initEntities[nameof(DateTimeOffsetToDateTimeTest)] = (typeof(DateTimeOffsetEntity), dateTimeOffsetInitValue);

      initEntities[nameof(DateTimeToDateOnlyTest)] = (typeof(DateTimeEntity), dateTimeInitValue);
      initEntities[nameof(DateTimeToTimeOnlyTest)] = (typeof(DateTimeEntity), dateTimeInitValue);

      initEntities[nameof(DateTimeOffsetToDateOnlyTest)] = (typeof(DateTimeOffsetEntity), dateTimeOffsetInitValue);
      initEntities[nameof(DateTimeOffsetToTimeOnlyTest)] = (typeof(DateTimeOffsetEntity), dateTimeOffsetInitValue);

      initEntities[nameof(DateOnlyToDateTimeTest)] = (typeof(DateOnlyEntity), dateOnlyInitValue);
      initEntities[nameof(DateOnlyToDateTimeOffsetTest)] = (typeof(DateOnlyEntity), dateOnlyInitValue);
      initEntities[nameof(DateOnlyToTimeOnlyTest)] = (typeof(DateOnlyEntity), dateOnlyInitValue);

      initEntities[nameof(TimeOnlyToDateTimeTest)] = (typeof(TimeOnlyEntity), timeOnlyInitValue);
      initEntities[nameof(TimeOnlyToDateTimeOffsetTest)] = (typeof(TimeOnlyEntity), timeOnlyInitValue);
      initEntities[nameof(TimeOnlyToDateOnlyTest)] = (typeof(TimeOnlyEntity), timeOnlyInitValue);
    }

    [SetUp]
    public void BeforeEveryTestMethodSetup()
    {
      var entityAndValue = GetInitialEntityType();
      if (entityAndValue.type.Name.Contains("Offset", StringComparison.Ordinal)) {
        Require.AnyFeatureSupported(ProviderFeatures.DateTimeOffset | ProviderFeatures.DateTimeOffsetEmulation);
      }

      using (var domain = Domain.Build(BuildConfiguration(DomainUpgradeMode.Recreate, entityAndValue.type)))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        _ = entityAndValue.type
          .GetMethod(nameof(DateTimeEntity.CreateTestInstance), BindingFlags.Public | BindingFlags.Static)
          .Invoke(null,new[] { session, entityAndValue.initValue});

        tx.Complete();
      }
    }

    [Test]
    public void DateTimeToDateTimeOffsetTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.DateTimeOffset | ProviderFeatures.DateTimeOffsetEmulation);

      UpgradeAndTestData<DateTimeOffsetEntity, DateTimeOffset>(new DateTimeOffset(dateTimeInitValue, ServerTimeZone));
    }

    [Test]
    public void DateTimeOffsetToDateTimeTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.DateTimeOffset | ProviderFeatures.DateTimeOffsetEmulation);

      var expectedValue = RequreToServerOffset
        ? dateTimeOffsetInitValue.ToOffset(ServerTimeZone).DateTime
        : dateTimeOffsetInitValue.DateTime;

      UpgradeAndTestData<DateTimeEntity, DateTime>(expectedValue);
    }

    [Test]
    public void DateTimeToDateOnlyTest()
    {
      UpgradeAndTestData<DateOnlyEntity, DateOnly>(DateOnly.FromDateTime(dateTimeInitValue));
    }

    [Test]
    public void DateTimeToTimeOnlyTest()
    {
      if (StorageProviderInfo.Instance.Provider == StorageProvider.MySql) {
        UpgradeAndTestData<TimeOnlyEntity, TimeOnly>(timeOnlyInitValue);// datetime in mysql is without fractions
      }
      else {
        UpgradeAndTestData<TimeOnlyEntity, TimeOnly>(TimeOnly.FromDateTime(dateTimeInitValue));
      }
    }

    [Test]
    public void DateTimeOffsetToDateOnlyTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.DateTimeOffset | ProviderFeatures.DateTimeOffsetEmulation);

      var expectedValue = RequreToServerOffset
        ? DateOnly.FromDateTime(dateTimeOffsetInitValue.ToOffset(ServerTimeZone).DateTime)
        : DateOnly.FromDateTime(dateTimeOffsetInitValue.DateTime);

      UpgradeAndTestData<DateOnlyEntity, DateOnly>(expectedValue);
    }

    [Test]
    public void DateTimeOffsetToTimeOnlyTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.DateTimeOffset | ProviderFeatures.DateTimeOffsetEmulation);

      var expectedValue = RequreToServerOffset
        ? TimeOnly.FromDateTime(dateTimeOffsetInitValue.ToOffset(ServerTimeZone).DateTime)
        : TimeOnly.FromDateTime(dateTimeOffsetInitValue.DateTime);

      UpgradeAndTestData<TimeOnlyEntity, TimeOnly>(expectedValue);
    }

    [Test]
    public void DateOnlyToDateTimeTest()
    {
      UpgradeAndTestData<DateTimeEntity, DateTime>(dateOnlyInitValue.ToDateTime(TimeOnly.MinValue));
    }

    [Test]
    public void DateOnlyToDateTimeOffsetTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.DateTimeOffset | ProviderFeatures.DateTimeOffsetEmulation);

      UpgradeAndTestData<DateTimeOffsetEntity, DateTimeOffset>(new DateTimeOffset(dateOnlyInitValue.ToDateTime(TimeOnly.MinValue), ServerTimeZone));
    }

    [Test]
    public void DateOnlyToTimeOnlyTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "No any error from storage on conversion");

      var provider = StorageProviderInfo.Instance.Provider;

      if (provider == StorageProvider.PostgreSql) {
        UpgradeAndExpectException<TimeOnlyEntity, SyntaxErrorException>();
      }
      else {
        UpgradeAndExpectException<TimeOnlyEntity, StorageException>();
      }
    }

    [Test]
    public void TimeOnlyToDateTimeTest()
    {
      var provider = StorageProviderInfo.Instance.Provider;

      DateTime expectedValue;
      if (provider == StorageProvider.Oracle) {
        expectedValue = DateTime.MinValue.Add(timeOnlyInitValue.ToTimeSpan());
      }
      else if (provider == StorageProvider.Firebird || provider == StorageProvider.MySql) {
        expectedValue = DateTime.Now.Date.Add(timeOnlyInitValue.ToTimeSpan());
      }
      else if(provider == StorageProvider.PostgreSql) {
        expectedValue = new DateTime(1970, 1, 1).Add(timeOnlyInitValue.ToTimeSpan());
      }
      else {
        expectedValue = new DateTime(1900, 1, 1).Add(timeOnlyInitValue.ToTimeSpan());
      }

      UpgradeAndTestData<DateTimeEntity, DateTime>(expectedValue);
    }

    [Test]
    public void TimeOnlyToDateTimeOffsetTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.DateTimeOffset | ProviderFeatures.DateTimeOffsetEmulation);

      var provider = StorageProviderInfo.Instance.Provider;

      DateTimeOffset expectedValue;
      if (provider == StorageProvider.Oracle) {
        expectedValue = new DateTimeOffset(
          DateTime.MinValue.Add(timeOnlyInitValue.ToTimeSpan()),
          TimeSpan.FromMinutes(60 * 4 + 2));
      }
      else if (provider == StorageProvider.PostgreSql) {
        expectedValue = new DateTimeOffset(new DateTime(1970, 1, 1)).Add(timeOnlyInitValue.ToTimeSpan()).ToOffset(ServerTimeZone);
      }
      else {
        expectedValue = new DateTimeOffset(new DateTime(1900, 1, 1).Add(timeOnlyInitValue.ToTimeSpan()), ServerTimeZone);
      }

      UpgradeAndTestData<DateTimeOffsetEntity, DateTimeOffset>(expectedValue);
    }

    [Test]
    public void TimeOnlyToDateOnlyTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "No any error from storage on conversion");

      var provider = StorageProviderInfo.Instance.Provider;

      if (provider == StorageProvider.PostgreSql) {
        UpgradeAndExpectException<DateOnlyEntity, SyntaxErrorException>();
      }
      else {
        UpgradeAndExpectException<DateOnlyEntity, StorageException>();
      }
    }

    private void UpgradeAndTestData<TEntity, TExpectedValue>(TExpectedValue expectedValue)
      where TEntity : Entity
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Perform, typeof(TEntity), GetUpgradeHandler());

      using (var domain = Domain.Build(configuration))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var entity = session.Query.All<TEntity>().FirstOrDefault();
        Assert.That(entity["ConversionField"], Is.EqualTo(expectedValue));
      }
    }

    private void UpgradeSafelyAndTestData<TEntity, TExpectedValue>(TExpectedValue expectedValue)
      where TEntity : Entity
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.Perform, typeof(TEntity), GetUpgradeHandler());

      using (var domain = Domain.Build(configuration))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var entity = session.Query.All<TEntity>().FirstOrDefault();
        Assert.That(entity["ConversionField"], Is.EqualTo(expectedValue));
      }
    }

    private void UpgradeAndExpectException<TEntity, TExpectedException>()
      where TEntity : Entity
      where TExpectedException : Exception
    {
      var configuration = BuildConfiguration(DomainUpgradeMode.PerformSafely, typeof(TEntity), GetUpgradeHandler());
      _ = Assert.Throws<TExpectedException>(() => Domain.Build(configuration));
    }

    private DomainConfiguration BuildConfiguration(DomainUpgradeMode upgradeMode, Type entityType, Type upgraderType = null)
    {
      var domainConfiguration = DomainConfigurationFactory.Create();

      domainConfiguration.UpgradeMode = upgradeMode;
      domainConfiguration.Types.Register(entityType);
      if (upgradeMode is DomainUpgradeMode.Perform or DomainUpgradeMode.PerformSafely
          && upgraderType is not null) {
        domainConfiguration.Types.Register(upgraderType);
      }
      return domainConfiguration;
    }

    private Type GetUpgradeHandler() =>
      upgradeHandlers[TestContext.CurrentContext.Test.MethodName];

    private (Type type, object initValue) GetInitialEntityType() =>
      initEntities[TestContext.CurrentContext.Test.MethodName];
  }
}

namespace Xtensive.Orm.Tests.Upgrade.DateTimeTypesDataConversionModel
{
  [HierarchyRoot]
  [TableMapping("TestEntity")]
  public class DateTimeEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public DateTime ConversionField { get; set; }

    public static object CreateTestInstance(Session session, object value)
    {
      return new DateTimeEntity(session, (DateTime) value);
    }

    public DateTimeEntity(Session session, DateTime value)
      : base(session)
    {
      ConversionField = value;
    }
  }

  [HierarchyRoot]
  [TableMapping("TestEntity")]
  public class DateTimeOffsetEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public DateTimeOffset ConversionField { get; set; }

    public static object CreateTestInstance(Session session, object value)
    {
      return new DateTimeOffsetEntity(session, (DateTimeOffset) value);
    }

    public DateTimeOffsetEntity(Session session, DateTimeOffset value)
      : base(session)
    {
      ConversionField = value;
    }
  }

  [HierarchyRoot]
  [TableMapping("TestEntity")]
  public class DateOnlyEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public DateOnly ConversionField { get; set; }

    public static object CreateTestInstance(Session session, object value)
    {
      return new DateOnlyEntity(session, (DateOnly) value);
    }

    public DateOnlyEntity(Session session, DateOnly value)
      : base(session)
    {
      ConversionField = value;
    }
  }

  [HierarchyRoot]
  [TableMapping("TestEntity")]
  public class TimeOnlyEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public TimeOnly ConversionField { get; set; }

    public static object CreateTestInstance(Session session, object value)
    {
      return new TimeOnlyEntity(session, (TimeOnly) value);
    }

    public TimeOnlyEntity(Session session, TimeOnly value)
      : base(session)
    {
      ConversionField = value;
    }
  }

  public class DateTimeToDateTimeOffsetUpgradeHandler : UpgradeHandler
  {
    public override bool CanUpgradeFrom(string oldVersion) => true;

    protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
    {
      _ = hints.Add(RenameTypeHint.Create<DateTimeOffsetEntity>("Xtensive.Orm.Tests.Upgrade.DateTimeTypesDataConversionModel.DateTimeEntity"));
      _ = hints.Add(ChangeFieldTypeHint.Create<DateTimeOffsetEntity>(e => e.ConversionField));
    }
  }

  public class DateTimeOffsetToDateTimeUpgradeHandler : UpgradeHandler
  {
    public override bool CanUpgradeFrom(string oldVersion) => true;

    protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
    {
      _ = hints.Add(RenameTypeHint.Create<DateTimeEntity>("Xtensive.Orm.Tests.Upgrade.DateTimeTypesDataConversionModel.DateTimeOffsetEntity"));
      _ = hints.Add(ChangeFieldTypeHint.Create<DateTimeEntity>(e => e.ConversionField));
    }
  }

  public class DateTimeToDateOnlyUpgradeHandler : UpgradeHandler
  {
    public override bool CanUpgradeFrom(string oldVersion) => true;

    protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
    {
      _ = hints.Add(RenameTypeHint.Create<DateOnlyEntity>("Xtensive.Orm.Tests.Upgrade.DateTimeTypesDataConversionModel.DateTimeEntity"));
      _ = hints.Add(ChangeFieldTypeHint.Create<DateOnlyEntity>(e => e.ConversionField));
    }
  }

  public class DateTimeToTimeOnlyUpgradeHandler : UpgradeHandler
  {
    public override bool CanUpgradeFrom(string oldVersion) => true;

    protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
    {
      _ = hints.Add(RenameTypeHint.Create<TimeOnlyEntity>("Xtensive.Orm.Tests.Upgrade.DateTimeTypesDataConversionModel.DateTimeEntity"));
      _ = hints.Add(ChangeFieldTypeHint.Create<TimeOnlyEntity>(e => e.ConversionField));
    }
  }

  public class DateTimeOffsetToDateOnlyUpgradeHandler : UpgradeHandler
  {
    public override bool CanUpgradeFrom(string oldVersion) => true;

    protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
    {
      _ = hints.Add(RenameTypeHint.Create<DateOnlyEntity>("Xtensive.Orm.Tests.Upgrade.DateTimeTypesDataConversionModel.DateTimeOffsetEntity"));
      _ = hints.Add(ChangeFieldTypeHint.Create<DateOnlyEntity>(e => e.ConversionField));
    }
  }

  public class DateTimeOffsetToTimeOnlyUpgradeHandler : UpgradeHandler
  {
    public override bool CanUpgradeFrom(string oldVersion) => true;

    protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
    {
      _ = hints.Add(RenameTypeHint.Create<TimeOnlyEntity>("Xtensive.Orm.Tests.Upgrade.DateTimeTypesDataConversionModel.DateTimeOffsetEntity"));
      _ = hints.Add(ChangeFieldTypeHint.Create<TimeOnlyEntity>(e => e.ConversionField));
    }
  }

  public class DateOnlyToDateTimeUpgradeHandler : UpgradeHandler
  {
    public override bool CanUpgradeFrom(string oldVersion) => true;

    protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
    {
      _ = hints.Add(RenameTypeHint.Create<DateTimeEntity>("Xtensive.Orm.Tests.Upgrade.DateTimeTypesDataConversionModel.DateOnlyEntity"));
      _ = hints.Add(ChangeFieldTypeHint.Create<DateTimeEntity>(e => e.ConversionField));
    }
  }

  public class DateOnlyToDateTimeOffsetUpgradeHandler : UpgradeHandler
  {
    public override bool CanUpgradeFrom(string oldVersion) => true;

    protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
    {
      _ = hints.Add(RenameTypeHint.Create<DateTimeOffsetEntity>("Xtensive.Orm.Tests.Upgrade.DateTimeTypesDataConversionModel.DateOnlyEntity"));
      _ = hints.Add(ChangeFieldTypeHint.Create<DateTimeOffsetEntity>(e => e.ConversionField));
    }
  }

  public class DateOnlyToTimeOnlyUpgradeHandler : UpgradeHandler
  {
    public override bool CanUpgradeFrom(string oldVersion) => true;

    protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
    {
      _ = hints.Add(RenameTypeHint.Create<TimeOnlyEntity>("Xtensive.Orm.Tests.Upgrade.DateTimeTypesDataConversionModel.DateOnlyEntity"));
      _ = hints.Add(ChangeFieldTypeHint.Create<TimeOnlyEntity>(e => e.ConversionField));
    }
  }

  public class TimeOnlyToDateTimeUpgradeHandler : UpgradeHandler
  {
    public override bool CanUpgradeFrom(string oldVersion) => true;

    protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
    {
      _ = hints.Add(RenameTypeHint.Create<DateTimeEntity>("Xtensive.Orm.Tests.Upgrade.DateTimeTypesDataConversionModel.TimeOnlyEntity"));
      _ = hints.Add(ChangeFieldTypeHint.Create<DateTimeEntity>(e => e.ConversionField));
    }
  }

  public class TimeOnlyToDateTimeOffsetUpgradeHandler : UpgradeHandler
  {
    public override bool CanUpgradeFrom(string oldVersion) => true;

    protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
    {
      _ = hints.Add(RenameTypeHint.Create<DateTimeOffsetEntity>("Xtensive.Orm.Tests.Upgrade.DateTimeTypesDataConversionModel.TimeOnlyEntity"));
      _ = hints.Add(ChangeFieldTypeHint.Create<DateTimeOffsetEntity>(e => e.ConversionField));
    }
  }

  public class TimeOnlyToDateOnlyUpgradeHandler : UpgradeHandler
  {
    public override bool CanUpgradeFrom(string oldVersion) => true;

    protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
    {
      _ = hints.Add(RenameTypeHint.Create<DateOnlyEntity>("Xtensive.Orm.Tests.Upgrade.DateTimeTypesDataConversionModel.TimeOnlyEntity"));
      _ = hints.Add(ChangeFieldTypeHint.Create<DateOnlyEntity>(e => e.ConversionField));
    }
  }
}