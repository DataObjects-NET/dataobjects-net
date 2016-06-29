// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.02.14

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0421_DateTimeAddXxxModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0421_DateTimeAddXxxModel
  {
    [HierarchyRoot]
    public class EntityWithDate : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public DateTime Today { get; set; }
    }
  }

  [TestFixture]
  public class IssueJira0421_DateTimeAddXxx : AutoBuildTest
  {
    //    private DateTime today = new DateTime(2016, 01, 02, 03, 04, 05, 347); todo This Value failed with sqlite provider because of accuracy millisecond error
    private DateTime today = new DateTime(2016, 01, 02, 03, 04, 05, 348);
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (EntityWithDate));
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var e = new EntityWithDate {Today = today};
        tx.Complete();
      }
    }

    private void RunAllTestsInt(Func<int, Expression<Func<EntityWithDate, bool>>> filterProvider)
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        RunTest(session, filterProvider.Invoke(1));
        RunTest(session, filterProvider.Invoke(20));
        RunTest(session, filterProvider.Invoke(-5));
        RunTest(session, filterProvider.Invoke(0));
      }
    }

    private void RunAllTestsDouble(Func<double, Expression<Func<EntityWithDate, bool>>> filterProvider)
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        RunTest(session, filterProvider.Invoke(1));
        RunTest(session, filterProvider.Invoke(20));
        RunTest(session, filterProvider.Invoke(-5));
        RunTest(session, filterProvider.Invoke(0));
      }
    }

    private static void RunTest(Session session, Expression<Func<EntityWithDate,bool>> filter)
    {
      var count = session.Query.All<EntityWithDate>().Count(filter);
      Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public void AddYearsTest()
    {
      RunAllTestsInt(value => e => e.Today.AddYears(value)==today.AddYears(value));
    }

    [Test]
    public void AddMonthsTest()
    {
      RunAllTestsInt(value => e => e.Today.AddMonths(value)==today.AddMonths(value));
    }

    [Test]
    public void AddDaysTest()
    {
      RunAllTestsDouble(value => e => e.Today.AddDays(value)==today.AddDays(value));
    }

    [Test]
    public void AddHoursTest()
    {
      RunAllTestsDouble(value => e => e.Today.AddHours(value)==today.AddHours(value));
    }

    [Test]
    public void AddMinutesTest()
    {
      RunAllTestsDouble(value => e => e.Today.AddMinutes(value)==today.AddMinutes(value));
    }

    [Test]
    public void AddSecondsTest()
    {
      RunAllTestsDouble(value => e => e.Today.AddSeconds(value)==today.AddSeconds(value));
    }

    [Test]
    public void AddMillisecondsTest()
    {
      RunAllTestsDouble(value => e => e.Today.AddMilliseconds(value)==today.AddMilliseconds(value));
    }
  }
}