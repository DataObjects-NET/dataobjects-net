// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.09.09

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
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
    
    [Test]
    public void Test01()
    {
      //new DateTimeOffset(new DateTime());
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Where(el => new DateTimeOffset(el.DateTime).Hour > 12).Select(el => new {LocalDateTimeOffset = new DateTimeOffset(el.DateTime)}).Run();
      }
    }

    [Test]
    public void Test02()
    {
      //new DateTimeOffset(new DateTime(), new TimeSpan());
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Where(el=> new DateTimeOffset(el.DateTime, UtcZone).Hour > 12)
          .Run();

        session.Query.All<TestEntity>()
          .Where(el => new DateTimeOffset(el.DateTime, EkaterinburgZone).Hour > 12)
          .Run();

        session.Query.All<TestEntity>()
          .Where(el => new DateTimeOffset(el.DateTime, MoscowZone).Hour > 12)
          .Run();

        session.Query.All<TestEntity>()
          .Where(el => new DateTimeOffset(el.DateTime, NewYorkZone).Hour > 12)
          .Run();
      }
    }

    [Test]
    public void Test03()
    {
      //new DateTimeOffset(0, 0, 0, 0, 0, 0, new TimeSpan());
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Where(el =>
            new DateTimeOffset(
              el.DateTime.Year,
              el.DateTime.Month,
              el.DateTime.Day,
              el.DateTime.Hour,
              el.DateTime.Minute,
              el.DateTime.Second, UtcZone).Hour > 12)
          .Run();

        session.Query.All<TestEntity>()
          .Where(el =>
            new DateTimeOffset(
              el.DateTime.Year,
              el.DateTime.Month,
              el.DateTime.Day,
              el.DateTime.Hour,
              el.DateTime.Minute,
              el.DateTime.Second, MoscowZone).Hour > 12)
          .Run();

        session.Query.All<TestEntity>()
          .Where(el =>
            new DateTimeOffset(
              el.DateTime.Year,
              el.DateTime.Month,
              el.DateTime.Day,
              el.DateTime.Hour,
              el.DateTime.Minute,
              el.DateTime.Second, EkaterinburgZone).Hour > 12)
          .Run();

        session.Query.All<TestEntity>()
          .Where(el =>
            new DateTimeOffset(
              el.DateTime.Year,
              el.DateTime.Month,
              el.DateTime.Day,
              el.DateTime.Hour,
              el.DateTime.Minute,
              el.DateTime.Second, NewYorkZone).Hour > 12)
          .Run();
      }
    }

    [Test]
    public void Test04()
    {
      //new DateTimeOffset(0, 0, 0, 0, 0, 0, 0, new TimeSpan());
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Where(el =>
            new DateTimeOffset(
              el.DateTime.Year,
              el.DateTime.Month,
              el.DateTime.Day,
              el.DateTime.Hour,
              el.DateTime.Minute,
              el.DateTime.Second,
              el.DateTime.Millisecond, UtcZone).Hour > 12)
          .Run();

        session.Query.All<TestEntity>()
          .Where(el =>
            new DateTimeOffset(
              el.DateTime.Year,
              el.DateTime.Month,
              el.DateTime.Day,
              el.DateTime.Hour,
              el.DateTime.Minute,
              el.DateTime.Second,
              el.DateTime.Millisecond, MoscowZone).Hour > 12)
          .Run();

        session.Query.All<TestEntity>()
          .Where(el =>
            new DateTimeOffset(
              el.DateTime.Year,
              el.DateTime.Month,
              el.DateTime.Day,
              el.DateTime.Hour,
              el.DateTime.Minute,
              el.DateTime.Second,
              el.DateTime.Millisecond, EkaterinburgZone).Hour > 12)
          .Run();

        session.Query.All<TestEntity>()
          .Where(el =>
            new DateTimeOffset(
              el.DateTime.Year,
              el.DateTime.Month,
              el.DateTime.Day,
              el.DateTime.Hour,
              el.DateTime.Minute,
              el.DateTime.Second,
              el.DateTime.Millisecond, NewYorkZone).Hour > 12)
          .Run();
      }
    }

    [Test]
    public void Test05()
    {
      //new DateTimeOffset(new DateTime(), new TimeSpan());
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Where(el => new DateTimeOffset(el.DateTime, el.UtcZone).Hour > 12)
          .Run();

        session.Query.All<TestEntity>()
          .Where(el => new DateTimeOffset(el.DateTime, el.EkaterinburgZone).Hour > 12)
          .Run();

        session.Query.All<TestEntity>()
          .Where(el => new DateTimeOffset(el.DateTime, el.MoscowZone).Hour > 12)
          .Run();

        session.Query.All<TestEntity>()
          .Where(el => new DateTimeOffset(el.DateTime, el.NewYorkZone).Hour > 12)
          .Run();
      }
    }

    [Test]
    public void Test06()
    {
      //new DateTimeOffset(0, 0, 0, 0, 0, 0, new TimeSpan());
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Where(el =>
            new DateTimeOffset(
              el.DateTime.Year,
              el.DateTime.Month,
              el.DateTime.Day,
              el.DateTime.Hour,
              el.DateTime.Minute,
              el.DateTime.Second, el.UtcZone).Hour > 12)
          .Run();

        session.Query.All<TestEntity>()
          .Where(el =>
            new DateTimeOffset(
              el.DateTime.Year,
              el.DateTime.Month,
              el.DateTime.Day,
              el.DateTime.Hour,
              el.DateTime.Minute,
              el.DateTime.Second, el.MoscowZone).Hour > 12)
          .Run();

        session.Query.All<TestEntity>()
          .Where(el =>
            new DateTimeOffset(
              el.DateTime.Year,
              el.DateTime.Month,
              el.DateTime.Day,
              el.DateTime.Hour,
              el.DateTime.Minute,
              el.DateTime.Second, el.EkaterinburgZone).Hour > 12)
          .Run();

        session.Query.All<TestEntity>()
          .Where(el =>
            new DateTimeOffset(
              el.DateTime.Year,
              el.DateTime.Month,
              el.DateTime.Day,
              el.DateTime.Hour,
              el.DateTime.Minute,
              el.DateTime.Second, el.NewYorkZone).Hour > 12)
          .Run();
      }
    }

    [Test]
    public void Test07()
    {
      //new DateTimeOffset(0, 0, 0, 0, 0, 0, 0, new TimeSpan());
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Where(el =>
            new DateTimeOffset(
              el.DateTime.Year,
              el.DateTime.Month,
              el.DateTime.Day,
              el.DateTime.Hour,
              el.DateTime.Minute,
              el.DateTime.Second,
              el.DateTime.Millisecond, el.UtcZone).Hour > 12)
          .Run();

        session.Query.All<TestEntity>()
          .Where(el =>
            new DateTimeOffset(
              el.DateTime.Year,
              el.DateTime.Month,
              el.DateTime.Day,
              el.DateTime.Hour,
              el.DateTime.Minute,
              el.DateTime.Second,
              el.DateTime.Millisecond, el.MoscowZone).Hour > 12)
          .Run();

        session.Query.All<TestEntity>()
          .Where(el =>
            new DateTimeOffset(
              el.DateTime.Year,
              el.DateTime.Month,
              el.DateTime.Day,
              el.DateTime.Hour,
              el.DateTime.Minute,
              el.DateTime.Second,
              el.DateTime.Millisecond, el.EkaterinburgZone).Hour > 12)
          .Run();

        session.Query.All<TestEntity>()
          .Where(el =>
            new DateTimeOffset(
              el.DateTime.Year,
              el.DateTime.Month,
              el.DateTime.Day,
              el.DateTime.Hour,
              el.DateTime.Minute,
              el.DateTime.Second,
              el.DateTime.Millisecond, el.NewYorkZone).Hour > 12)
          .Run();
      }
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        for (int i = 1; i < 13; i++) {
          new TestEntity {
            DateTime = new DateTime(2000 + i, i, 12 + i, 11 + i, 4 * i, 3 * i),
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
  }
}
