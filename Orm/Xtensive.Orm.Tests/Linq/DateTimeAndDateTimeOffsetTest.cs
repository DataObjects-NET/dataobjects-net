// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Groznov
// Created:    2016.04.26

using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Linq.DateTimeOffsetTestModel;

namespace Xtensive.Orm.Tests.Linq.DateTimeOffsetTestModel
{
  [HierarchyRoot]
  public class TestEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public DateTime Date { get; set; }

    [Field]
    public DateTimeOffset DateWithOffset { get; set; }

    [Field]
    public DateTimeOffset? NullableDateDateWithOffset { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Linq
{
  public class DateTimeAndDateTimeOffsetTest : AutoBuildTest
  {
    private static readonly TimeSpan DefaultOffset1 = TimeSpan.FromHours(2);
    private static readonly TimeSpan DefaultOffset2 = TimeSpan.FromHours(3);
    private static readonly TimeSpan DefaultOffset3 = TimeSpan.FromHours(-3.25);
    private static readonly DateTime DefaultDateTime = new DateTime(2016, 04, 27, 13, 14, 15);
    private static readonly DateTimeOffset DefaultDateTimeOffset = new DateTimeOffset(DefaultDateTime, DefaultOffset1);

    private const string FirstEntityName = "FirstEntity";

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (TestEntity));
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new TestEntity {
          Name = "FirstEntity",
          Date = DefaultDateTime,
          DateWithOffset = DefaultDateTimeOffset
        };
        tx.Complete();
      }
    }

    [Test]
    public void EqualsTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var entity = Query.All<TestEntity>().First(c => c.Name==FirstEntityName);
        Assert.AreEqual(entity.Date, DefaultDateTime);
        Assert.AreEqual(entity.DateWithOffset, DefaultDateTimeOffset);
        Assert.AreEqual(entity.DateWithOffset, DefaultDateTimeOffset.ToOffset(DefaultOffset2));
        Assert.AreEqual(entity.DateWithOffset.ToOffset(DefaultOffset2), DefaultDateTimeOffset);
      }
    }

    [Test]
    public void DifferentCulturesTest()
    {
      var oldCulture = Thread.CurrentThread.CurrentCulture;
      using (new Disposable(c => Thread.CurrentThread.CurrentCulture = oldCulture)) {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        EqualsTest();
      }
      using (new Disposable(c => Thread.CurrentThread.CurrentCulture = oldCulture)) {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");
        EqualsTest();
      }

      using (new Disposable(c => Thread.CurrentThread.CurrentCulture = oldCulture)) {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        EqualsTest();
      }
    }

    [Test]
    public void WhereDateTimeTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        RunTest(c => c.Date==DefaultDateTime);
      }
    }

    [Test]
    public void WhereDateTimeOffsetTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        RunTest(c => c.DateWithOffset==DefaultDateTimeOffset);
        RunTest(c => c.DateWithOffset==DefaultDateTimeOffset.ToOffset(DefaultOffset2));
        RunTest(c => c.DateWithOffset==DefaultDateTimeOffset.ToOffset(DefaultOffset3));
      }
    }

    [Test]
    public void ExtractDateTimeTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        RunTest(c => c.Date.Year==DefaultDateTime.Year);
        RunTest(c => c.Date.Month==DefaultDateTime.Month);
        RunTest(c => c.Date.Day==DefaultDateTime.Day);
        RunTest(c => c.Date.Hour==DefaultDateTime.Hour);
        RunTest(c => c.Date.Minute==DefaultDateTime.Minute);
        RunTest(c => c.Date.Second==DefaultDateTime.Second);
      }
    }

    [Test]
    public void ExtractDateTimeOffsetTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        RunTest(c => true);
        RunTest(c => c.DateWithOffset.Year==DefaultDateTimeOffset.Year);
        RunTest(c => c.DateWithOffset.Month==DefaultDateTimeOffset.Month);
        RunTest(c => c.DateWithOffset.Day==DefaultDateTimeOffset.Day);
        RunTest(c => c.DateWithOffset.Hour==DefaultDateTimeOffset.Hour);
        RunTest(c => c.DateWithOffset.Minute==DefaultDateTimeOffset.Minute);
        RunTest(c => c.DateWithOffset.Second==DefaultDateTimeOffset.Second);
        RunTest(c => c.DateWithOffset.Offset.Hours==DefaultDateTimeOffset.Offset.Hours);
        RunTest(c => c.DateWithOffset.Offset.Minutes == DefaultDateTimeOffset.Offset.Minutes);
        RunTest(c => c.DateWithOffset.Offset == DefaultDateTimeOffset.Offset);
      }
    }

    private void RunTest(Expression<Func<TestEntity, bool>> filter, int rightCount = 1)
    {
      var count = Query.All<TestEntity>().Count(filter);
      Assert.AreEqual(count, rightCount);
    }
  }
}
