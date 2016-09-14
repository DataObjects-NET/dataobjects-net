// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.09.09

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.DateTimeOffsetConstructorTestModel;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.DateTimeOffsetConstructorTestModel
{
  [HierarchyRoot]
  public class TestEntity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public DateTime DateTime { get; set; }

    [Field]
    public TimeSpan UtcZone { get; set; }

    [Field]
    public TimeSpan MoscowZone { get; set; }

    [Field]
    public TimeSpan EkaterinburgZone { get; set; }

    [Field]
    public TimeSpan NewYorkZone { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.DateTimeOffsets
{
  public class DateTimeOffsetConstructorTest : AutoBuildTest
  {
    private readonly TimeSpan UtcZone = new TimeSpan(0, 0, 0);
    private readonly TimeSpan MoscowZone = new TimeSpan(3, 0, 0);
    private readonly TimeSpan EkaterinburgZone = new TimeSpan(5, 0, 0); 
    private readonly TimeSpan NewYorkZone = new TimeSpan(-5, 0, 0);

    private DateTime[] dateTimes = new DateTime[12];
    private DateTimeOffset[] utcDateTimeOffsets = new DateTimeOffset[12];
    private DateTimeOffset[] moscowDateTimeOffsets = new DateTimeOffset[12];
    private DateTimeOffset[] ekaterinburgDateTimeOffsets = new DateTimeOffset[12];
    private DateTimeOffset[] newYorkDateTimeOffsets = new DateTimeOffset[12];

    [Test]
    public void Test01()
    {
      //new DateTimeOffset(new DateTime());
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var count = session.Query.All<TestEntity>()
          .Count(el => new DateTimeOffset(el.DateTime).Hour > 15);

        var expectedCount = ekaterinburgDateTimeOffsets
          .Select(TryMoveToLocalTimeZone)
          .Count(el => el.Hour > 15);
        Assert.That(count, Is.Not.EqualTo(0));
        Assert.That(expectedCount, Is.Not.EqualTo(0));
        Assert.That(count, Is.EqualTo(expectedCount));

        var result = session.Query.All<TestEntity>()
          .Where(el => new DateTimeOffset(dateTimes[0]).Hour==new DateTimeOffset(el.DateTime).Hour).ToArray();

        Assert.That(result.Length, Is.EqualTo(1));
        Assert.That(result[0].DateTime.Hour, Is.EqualTo(dateTimes[0].Hour));
      }
    }

    [Test]
    public void Test02()
    {
      //new DateTimeOffset(new DateTime(), new TimeSpan());
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var count = session.Query.All<TestEntity>()
          .Count(el => new DateTimeOffset(el.DateTime, UtcZone).Hour > 15);

        var expectedCount = utcDateTimeOffsets
          .Select(TryMoveToLocalTimeZone)
          .Count(el => el.Hour > 15);
        Assert.That(count, Is.Not.EqualTo(0));
        Assert.That(expectedCount, Is.Not.EqualTo(0));
        Assert.That(count, Is.EqualTo(expectedCount));

        count = session.Query.All<TestEntity>()
          .Count(el => new DateTimeOffset(el.DateTime, EkaterinburgZone).Hour > 15);

        expectedCount = ekaterinburgDateTimeOffsets
          .Select(TryMoveToLocalTimeZone)
          .Count(el => el.Hour > 15);
        Assert.That(count, Is.Not.EqualTo(0));
        Assert.That(expectedCount, Is.Not.EqualTo(0));
        Assert.That(count, Is.EqualTo(expectedCount));

        count = session.Query.All<TestEntity>()
          .Count(el => new DateTimeOffset(el.DateTime, MoscowZone).Hour > 15);

        expectedCount = moscowDateTimeOffsets
          .Select(TryMoveToLocalTimeZone)
          .Count(el => el.Hour > 15);
        Assert.That(count, Is.Not.EqualTo(0));
        Assert.That(expectedCount, Is.Not.EqualTo(0));
        Assert.That(count, Is.EqualTo(expectedCount));

        count = session.Query.All<TestEntity>()
          .Count(el => new DateTimeOffset(el.DateTime, NewYorkZone).Hour > 15);

        expectedCount = newYorkDateTimeOffsets
          .Select(TryMoveToLocalTimeZone)
          .Count(el => el.Hour > 15);
        Assert.That(count, Is.Not.EqualTo(0));
        Assert.That(expectedCount, Is.Not.EqualTo(0));
        Assert.That(count, Is.EqualTo(expectedCount));
      }
    }

    [Test]
    public void Test03()
    {
      //new DateTimeOffset(0, 0, 0, 0, 0, 0, new TimeSpan());
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var count = session.Query.All<TestEntity>()
          .Count(el => new DateTimeOffset(
            el.DateTime.Year,
            el.DateTime.Month,
            el.DateTime.Day,
            el.DateTime.Hour,
            el.DateTime.Minute,
            el.DateTime.Second, UtcZone).Hour > 15);

        var expectedCount = utcDateTimeOffsets
          .Select(TryMoveToLocalTimeZone)
          .Count(el => el.Hour > 15);
        Assert.That(count, Is.Not.EqualTo(0));
        Assert.That(expectedCount, Is.Not.EqualTo(0));
        Assert.That(count, Is.EqualTo(expectedCount));

        count = session.Query.All<TestEntity>()
          .Count(el => new DateTimeOffset(
            el.DateTime.Year,
            el.DateTime.Month,
            el.DateTime.Day,
            el.DateTime.Hour,
            el.DateTime.Minute,
            el.DateTime.Second, MoscowZone).Hour > 15);

        expectedCount = moscowDateTimeOffsets
          .Select(TryMoveToLocalTimeZone)
          .Count(el => el.Hour > 15);
        Assert.That(count, Is.Not.EqualTo(0));
        Assert.That(expectedCount, Is.Not.EqualTo(0));
        Assert.That(count, Is.EqualTo(expectedCount));

        count = session.Query.All<TestEntity>()
          .Count(el => new DateTimeOffset(
            el.DateTime.Year,
            el.DateTime.Month,
            el.DateTime.Day,
            el.DateTime.Hour,
            el.DateTime.Minute,
            el.DateTime.Second, EkaterinburgZone).Hour > 15);

        expectedCount = ekaterinburgDateTimeOffsets
          .Select(TryMoveToLocalTimeZone)
          .Count(el => el.Hour > 15);
        Assert.That(count, Is.Not.EqualTo(0));
        Assert.That(expectedCount, Is.Not.EqualTo(0));
        Assert.That(count, Is.EqualTo(expectedCount));

        count = session.Query.All<TestEntity>()
          .Count(el => new DateTimeOffset(
            el.DateTime.Year,
            el.DateTime.Month,
            el.DateTime.Day,
            el.DateTime.Hour,
            el.DateTime.Minute,
            el.DateTime.Second, NewYorkZone).Hour > 15);

        expectedCount = newYorkDateTimeOffsets
          .Select(TryMoveToLocalTimeZone)
          .Count(el => el.Hour > 15);
        Assert.That(count, Is.Not.EqualTo(0));
        Assert.That(expectedCount, Is.Not.EqualTo(0));
        Assert.That(count, Is.EqualTo(expectedCount));
      }
    }

    [Test]
    public void Test04()
    {
      //new DateTimeOffset(0, 0, 0, 0, 0, 0, 0, new TimeSpan());
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var count = session.Query.All<TestEntity>()
          .Count(el => new DateTimeOffset(
            el.DateTime.Year,
            el.DateTime.Month,
            el.DateTime.Day,
            el.DateTime.Hour,
            el.DateTime.Minute,
            el.DateTime.Second,
            el.DateTime.Millisecond, UtcZone).Hour > 15);

        var expectedCount = utcDateTimeOffsets
          .Select(TryMoveToLocalTimeZone)
          .Count(el => el.Hour > 15);
        Assert.That(count, Is.Not.EqualTo(0));
        Assert.That(expectedCount, Is.Not.EqualTo(0));
        Assert.That(count, Is.EqualTo(expectedCount));

        count = session.Query.All<TestEntity>()
          .Count(el => new DateTimeOffset(
            el.DateTime.Year,
            el.DateTime.Month,
            el.DateTime.Day,
            el.DateTime.Hour,
            el.DateTime.Minute,
            el.DateTime.Second,
            el.DateTime.Millisecond, MoscowZone).Hour > 15);

        expectedCount = moscowDateTimeOffsets
          .Select(TryMoveToLocalTimeZone)
          .Count(el => el.Hour > 15);
        Assert.That(count, Is.Not.EqualTo(0));
        Assert.That(expectedCount, Is.Not.EqualTo(0));
        Assert.That(count, Is.EqualTo(expectedCount));

        count = session.Query.All<TestEntity>()
          .Count(el => new DateTimeOffset(
            el.DateTime.Year,
            el.DateTime.Month,
            el.DateTime.Day,
            el.DateTime.Hour,
            el.DateTime.Minute,
            el.DateTime.Second,
            el.DateTime.Millisecond, EkaterinburgZone).Hour > 15);

        expectedCount = ekaterinburgDateTimeOffsets
          .Select(TryMoveToLocalTimeZone)
          .Count(el => el.Hour > 15);
        Assert.That(count, Is.Not.EqualTo(0));
        Assert.That(expectedCount, Is.Not.EqualTo(0));
        Assert.That(count, Is.EqualTo(expectedCount));

        count = session.Query.All<TestEntity>()
          .Count(el => new DateTimeOffset(
            el.DateTime.Year,
            el.DateTime.Month,
            el.DateTime.Day,
            el.DateTime.Hour,
            el.DateTime.Minute,
            el.DateTime.Second,
            el.DateTime.Millisecond, NewYorkZone).Hour > 15);

        expectedCount = newYorkDateTimeOffsets
          .Select(TryMoveToLocalTimeZone)
          .Count(el => el.Hour > 15);
        Assert.That(count, Is.Not.EqualTo(0));
        Assert.That(expectedCount, Is.Not.EqualTo(0));
        Assert.That(count, Is.EqualTo(expectedCount));
      }
    }

    [Test]
    public void Test05()
    {
      //new DateTimeOffset(new DateTime(), new TimeSpan());
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var count = session.Query.All<TestEntity>().Count(el => new DateTimeOffset(el.DateTime, el.UtcZone).Hour > 15);

        var expectedCount = utcDateTimeOffsets
          .Select(TryMoveToLocalTimeZone)
          .Count(el => el.Hour > 15);
        Assert.That(count, Is.Not.EqualTo(0));
        Assert.That(expectedCount, Is.Not.EqualTo(0));
        Assert.That(count, Is.EqualTo(expectedCount));

        count = session.Query.All<TestEntity>().Count(el => new DateTimeOffset(el.DateTime, el.EkaterinburgZone).Hour > 15);

        expectedCount = ekaterinburgDateTimeOffsets
          .Select(TryMoveToLocalTimeZone)
          .Count(el => el.Hour > 15);
        Assert.That(count, Is.Not.EqualTo(0));
        Assert.That(expectedCount, Is.Not.EqualTo(0));
        Assert.That(count, Is.EqualTo(expectedCount));

        count = session.Query.All<TestEntity>().Count(el => new DateTimeOffset(el.DateTime, el.MoscowZone).Hour > 15);

        expectedCount = moscowDateTimeOffsets
          .Select(TryMoveToLocalTimeZone)
          .Count(el => el.Hour > 15);
        Assert.That(count, Is.Not.EqualTo(0));
        Assert.That(expectedCount, Is.Not.EqualTo(0));
        Assert.That(count, Is.EqualTo(expectedCount));

        count = session.Query.All<TestEntity>().Count(el => new DateTimeOffset(el.DateTime, el.NewYorkZone).Hour > 15);

        expectedCount = newYorkDateTimeOffsets
          .Select(TryMoveToLocalTimeZone)
          .Count(el => el.Hour > 15);
        Assert.That(count, Is.Not.EqualTo(0));
        Assert.That(expectedCount, Is.Not.EqualTo(0));
        Assert.That(count, Is.EqualTo(expectedCount));
      }
    }

    [Test]
    public void Test06()
    {
      //new DateTimeOffset(0, 0, 0, 0, 0, 0, new TimeSpan());
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var count = session.Query.All<TestEntity>()
          .Count(el => new DateTimeOffset(
            el.DateTime.Year,
            el.DateTime.Month,
            el.DateTime.Day,
            el.DateTime.Hour,
            el.DateTime.Minute,
            el.DateTime.Second, el.UtcZone).Hour > 15);

        var expectedCount = utcDateTimeOffsets
          .Select(TryMoveToLocalTimeZone)
          .Count(el => el.Hour > 15);
        Assert.That(count, Is.Not.EqualTo(0));
        Assert.That(expectedCount, Is.Not.EqualTo(0));
        Assert.That(count, Is.EqualTo(expectedCount));

        count = session.Query.All<TestEntity>()
          .Count(el => new DateTimeOffset(
            el.DateTime.Year,
            el.DateTime.Month,
            el.DateTime.Day,
            el.DateTime.Hour,
            el.DateTime.Minute,
            el.DateTime.Second, el.MoscowZone).Hour > 15);

        expectedCount = moscowDateTimeOffsets
          .Select(TryMoveToLocalTimeZone)
          .Count(el => el.Hour > 15);
        Assert.That(count, Is.Not.EqualTo(0));
        Assert.That(expectedCount, Is.Not.EqualTo(0));
        Assert.That(count, Is.EqualTo(expectedCount));

        count = session.Query.All<TestEntity>()
          .Count(el => new DateTimeOffset(
            el.DateTime.Year,
            el.DateTime.Month,
            el.DateTime.Day,
            el.DateTime.Hour,
            el.DateTime.Minute,
            el.DateTime.Second, el.EkaterinburgZone).Hour > 15);

        expectedCount = ekaterinburgDateTimeOffsets
          .Select(TryMoveToLocalTimeZone)
          .Count(el => el.Hour > 15);
        Assert.That(count, Is.Not.EqualTo(0));
        Assert.That(expectedCount, Is.Not.EqualTo(0));
        Assert.That(count, Is.EqualTo(expectedCount));

        count = session.Query.All<TestEntity>()
          .Count(el => new DateTimeOffset(
            el.DateTime.Year,
            el.DateTime.Month,
            el.DateTime.Day,
            el.DateTime.Hour,
            el.DateTime.Minute,
            el.DateTime.Second, el.NewYorkZone).Hour > 15);

        expectedCount = newYorkDateTimeOffsets
          .Select(TryMoveToLocalTimeZone)
          .Count(el => el.Hour > 15);
        Assert.That(count, Is.Not.EqualTo(0));
        Assert.That(expectedCount, Is.Not.EqualTo(0));
        Assert.That(count, Is.EqualTo(expectedCount));
      }
    }

    [Test]
    public void Test07()
    {
      //new DateTimeOffset(0, 0, 0, 0, 0, 0, 0, new TimeSpan());
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var count = session.Query.All<TestEntity>()
          .Count(el => new DateTimeOffset(
            el.DateTime.Year,
            el.DateTime.Month,
            el.DateTime.Day,
            el.DateTime.Hour,
            el.DateTime.Minute,
            el.DateTime.Second,
            el.DateTime.Millisecond, el.UtcZone).Hour > 15);

        var expectedCount = utcDateTimeOffsets
          .Select(TryMoveToLocalTimeZone)
          .Count(el => el.Hour > 15);
        Assert.That(count, Is.Not.EqualTo(0));
        Assert.That(expectedCount, Is.Not.EqualTo(0));
        Assert.That(count, Is.EqualTo(expectedCount));

        count = session.Query.All<TestEntity>()
          .Count(el => new DateTimeOffset(
            el.DateTime.Year,
            el.DateTime.Month,
            el.DateTime.Day,
            el.DateTime.Hour,
            el.DateTime.Minute,
            el.DateTime.Second,
            el.DateTime.Millisecond, el.MoscowZone).Hour > 15);

        expectedCount = moscowDateTimeOffsets
          .Select(TryMoveToLocalTimeZone)
          .Count(el => el.Hour > 15);
        Assert.That(count, Is.Not.EqualTo(0));
        Assert.That(expectedCount, Is.Not.EqualTo(0));
        Assert.That(count, Is.EqualTo(expectedCount));

        count = session.Query.All<TestEntity>()
          .Count(el => new DateTimeOffset(
            el.DateTime.Year,
            el.DateTime.Month,
            el.DateTime.Day,
            el.DateTime.Hour,
            el.DateTime.Minute,
            el.DateTime.Second,
            el.DateTime.Millisecond, el.EkaterinburgZone).Hour > 15);

        expectedCount = ekaterinburgDateTimeOffsets
          .Select(TryMoveToLocalTimeZone)
          .Count(el => el.Hour > 15);
        Assert.That(count, Is.Not.EqualTo(0));
        Assert.That(expectedCount, Is.Not.EqualTo(0));
        Assert.That(count, Is.EqualTo(expectedCount));

        count = session.Query.All<TestEntity>()
          .Count(el => new DateTimeOffset(
            el.DateTime.Year,
            el.DateTime.Month,
            el.DateTime.Day,
            el.DateTime.Hour,
            el.DateTime.Minute,
            el.DateTime.Second,
            el.DateTime.Millisecond, el.NewYorkZone).Hour > 15);

        expectedCount = newYorkDateTimeOffsets
          .Select(TryMoveToLocalTimeZone)
          .Count(el => el.Hour > 15);
        Assert.That(count, Is.Not.EqualTo(0));
        Assert.That(expectedCount, Is.Not.EqualTo(0));
        Assert.That(count, Is.EqualTo(expectedCount));
      }
    }

    protected override void PopulateData()
    {
      dateTimes = new DateTime[12];
      utcDateTimeOffsets = new DateTimeOffset[12];
      moscowDateTimeOffsets = new DateTimeOffset[12];
      ekaterinburgDateTimeOffsets = new DateTimeOffset[12];
      newYorkDateTimeOffsets = new DateTimeOffset[12];

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        for (int i = 1; i < 13; i++) {
          var datetime = new DateTime(2000 + i, i, 12 + i, 11 + i, 4 * i, 3 * i);
          dateTimes[i - 1] = datetime;
          utcDateTimeOffsets[i - 1] = new DateTimeOffset(datetime, UtcZone);
          moscowDateTimeOffsets[i - 1] = new DateTimeOffset(datetime, MoscowZone);
          ekaterinburgDateTimeOffsets[i - 1] = new DateTimeOffset(datetime, EkaterinburgZone);
          newYorkDateTimeOffsets[i - 1] = new DateTimeOffset(datetime, NewYorkZone);

          new TestEntity {
            DateTime = datetime,
            UtcZone = UtcZone,
            EkaterinburgZone = EkaterinburgZone,
            MoscowZone = MoscowZone,
            NewYorkZone = NewYorkZone
          };
        }
        transaction.Complete();
      }
    }

    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.DateTimeOffset);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (TestEntity).Assembly, typeof (TestEntity).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    private DateTimeOffset TryMoveToLocalTimeZone(DateTimeOffset dateTimeOffset)
    {
      if (ProviderInfo.ProviderName==WellKnown.Provider.PostgreSql)
        return dateTimeOffset.ToLocalTime();
      return dateTimeOffset;
    }
  }
}
