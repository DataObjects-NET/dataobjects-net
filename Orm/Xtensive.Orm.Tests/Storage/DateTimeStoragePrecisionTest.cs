// Copyright (C) 2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.DateTimeStoragePrecisionTestModel;

namespace Xtensive.Orm.Tests.Storage.DateTimeStoragePrecisionTestModel
{
  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class TestEntity : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public DateTime FDateTime { get; set; }
#if NET6_0_OR_GREATER

    [Field]
    public TimeOnly FTimeOnly { get; set; }
#endif

    public TestEntity(Session session, long idValue)
      : base(session, idValue)
    {
      var dateTime = new DateTime(idValue, DateTimeKind.Utc);
      FDateTime = dateTime;
#if NET6_0_OR_GREATER
      FTimeOnly = TimeOnly.FromDateTime(dateTime);
#endif
    }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  // Each storage is capable of storing certain amount of fractions of second.
  // This test writes a value with 7 fractional points to storage and,
  // depeding on storage, expects value of precision the storage have.
  public class DateTimeStoragePrecisionTest : AutoBuildTest
  {
    private const long TestDateTimeTicks = 638130658792224229L;  //2023-02-27 03:37:59.2224229 UTC

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(TestEntity));
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        _ = new TestEntity(session, TestDateTimeTicks);
        tx.Complete();
      }
    }

    [Test]
    public void DateTimeTest()
    {
      var dateTime = new DateTime(TestDateTimeTicks, DateTimeKind.Utc);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var entity = session.Query.All<TestEntity>().First(e => e.Id == TestDateTimeTicks);
        Assert.That(entity.FDateTime, Is.EqualTo(GetExpectedValue(dateTime)));
      }
    }
#if NET6_0_OR_GREATER

    [Test]
    public void TimeOnlyTest()
    {
      var dateTime = new DateTime(TestDateTimeTicks, DateTimeKind.Utc);
      var timeOnly = TimeOnly.FromDateTime(dateTime);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var entity = session.Query.All<TestEntity>().First(e => e.Id == TestDateTimeTicks);
        Assert.That(entity.FTimeOnly, Is.EqualTo(GetExpectedValue(timeOnly)));
      }
    }

    private static TimeOnly GetExpectedValue(in TimeOnly baseTimeOnly) => baseTimeOnly.AdjustTimeOnlyForCurrentProvider();
#endif

    private static DateTime GetExpectedValue(in DateTime baseDateTime)
    {
      var provider = StorageProviderInfo.Instance.Provider;
      return provider == StorageProvider.MySql
        ? baseDateTime.AdjustDateTime(0, true)
        : provider == StorageProvider.Oracle
          ? baseDateTime.AdjustDateTime(6, true)
          : baseDateTime.AdjustDateTimeForCurrentProvider();
    }
  }
}