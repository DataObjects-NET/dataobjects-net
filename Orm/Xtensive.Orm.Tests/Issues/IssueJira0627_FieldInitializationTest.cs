// Copyright (C) 2018 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2018.05.03

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.MemberInitWhereItIsNotSupposeToBeModel;

namespace Xtensive.Orm.Tests.Issues.MemberInitWhereItIsNotSupposeToBeModel
{
  public enum Level { Intern, Junior, Senior, Lead }
  public enum Department { RandD, It, Marketing, Sales }

  [HierarchyRoot]
  public class Employee : Entity
  {
    [Field, Key]
    public long Id { get; private set; }
    [Field]
    public long? FloorNumber { get; set; }
    [Field]
    public long? OfficeNumber { get; set; }
    [Field]
    public long? Salary { get; set; }
    [Field]
    public Level Level { get; set; }
    [Field]
    public Department Department { get; set; }
  }

  public class PersonSimplifiedByOffice : PersonSimplified<PersonSimplifiedByOffice>
  {
    public long OfficeNumber;
    public static Expression<Func<PersonSimplifiedByOffice, bool>> IsBigOffice = e => e.OfficeNumber < 3;
  }

  public class PersonSimplified<T> : PersonSimplified where T : PersonSimplified
  {
    public static Expression<Func<T, bool>> Require360Eval = e => e.Department != Department.Sales && (e.Level == Level.Junior || e.Level == Level.Senior);
    public static Expression<Func<T, bool>> NeedProductTraining = e => e.Department != Department.It && e.Level != Level.Intern;
  }

  public class PersonSimplified
  {
    public Level Level;
    public Department Department;
    public long? Cost;
  }

  public class PersonFlatByOffice
  {
    public long OfficeNumber;
    public static Expression<Func<PersonFlatByOffice, bool>> IsBigOffice = e => e.OfficeNumber < 3;

    public static Expression<Func<PersonFlatByOffice, bool>> Require360Eval = e => e.Department != Department.Sales && (e.Level == Level.Junior || e.Level == Level.Senior);
    public static Expression<Func<PersonFlatByOffice, bool>> NeedProductTraining = e => e.Department != Department.It && e.Level != Level.Intern;

    public Level Level;
    public Department Department;
    public long? Cost;
  }

  public class OfficeStatistics
  {
    public long OfficeNumber;
    public Level Level;
    public long Salary;

    public object this[string fieldName]
    {
      set
      {
        var fieldToSetValue = this.GetType().GetField(fieldName);
        if (fieldToSetValue==null)
          throw new ArgumentException();
        fieldToSetValue.SetValue(this, value);
      }
    }
  }

  public class ExtendedOfficeStatistics : OfficeStatistics
  {
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public sealed class IssueJira0627_FieldInitializationTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var domainConfiguration = base.BuildConfiguration();
      domainConfiguration.Types.Register(typeof(Employee).Assembly, typeof(Employee).Namespace);
      domainConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      return domainConfiguration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var levels = Enum.GetValues(typeof(Level));
        var departments = Enum.GetValues(typeof(Department));
        foreach (Level level in levels) {
          foreach (Department department in departments) {
            new Employee() {Level = level, Department = department, FloorNumber = 1, OfficeNumber = 2};
          }
        }
        tx.Complete();
      }
    }

    [Test]
    public void TestForProblem()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var q = Query.All<Employee>()
          .Where(e => e.FloorNumber==1)
          .Select(
            e => new PersonSimplifiedByOffice() {
              OfficeNumber = e.OfficeNumber.GetValueOrDefault(),
              Cost = e.Salary,
              Department = e.Department,
              Level = e.Level
            })
          .GroupBy(g => g.OfficeNumber)
          .Select(
            g => new {
              OfficeNumber = g.Key,
              TotalCost = g.Sum(e => e.Cost),
              BifOfficeCost = (g as IQueryable<PersonSimplifiedByOffice>).Where(PersonSimplifiedByOffice.IsBigOffice).Sum(e => e.Cost),
              TrainingCount = (g as IQueryable<PersonSimplifiedByOffice>).Where(PersonSimplifiedByOffice.NeedProductTraining).Sum(e => e.Cost),
            });
        q.Run();
      }
    }

    [Test]
    public void FlatStructureTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var q = Query.All<Employee>()
          .Where(e => e.FloorNumber==1)
          .Select(
            e => new PersonFlatByOffice() {
              OfficeNumber = e.OfficeNumber.GetValueOrDefault(),
              Cost = e.Salary,
              Department = e.Department,
              Level = e.Level
            })
          .GroupBy(g => g.OfficeNumber)
          .Select(
            g => new {
              OfficeNumber = g.Key,
              TotalCost = g.Sum(e => e.Cost),
              BifOfficeCost = (g as IQueryable<PersonFlatByOffice>).Where(PersonFlatByOffice.IsBigOffice).Sum(e => e.Cost),
              TrainingCount = (g as IQueryable<PersonFlatByOffice>).Where(PersonFlatByOffice.NeedProductTraining).Sum(e => e.Cost),
            });
        q.Run();
      }
    }

    [Test]
    public void DirectFieldsTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<Employee>()
          .Where(e => e.OfficeNumber==1)
          .Select(
            e => new OfficeStatistics() {
              OfficeNumber = e.OfficeNumber.GetValueOrDefault(),
              Salary = e.Salary.GetValueOrDefault(),
              Level = e.Level
            })
          .GroupBy(g => g.OfficeNumber)
          .Select(
            g => new {
              Floor = g.Key,
              TotalCost = g.Sum(e => e.Salary),
              AverageCost = g.Average(e => e.Salary),
              MinCost = g.Min(e => e.Salary),
              MaxCost = g.Max(e => e.Salary),
            });
        query.Run();
      }
    }

    [Test]
    public void NestedFieldsTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<Employee>()
          .Where(e => e.OfficeNumber==1)
          .Select(
            e => new ExtendedOfficeStatistics() {
              OfficeNumber = e.OfficeNumber.GetValueOrDefault(),
              Salary = e.Salary.GetValueOrDefault(),
              Level = e.Level
            })
          .GroupBy(g => g.OfficeNumber)
          .Select(
            g => new {
              Floor = g.Key,
              TotalCost = g.Sum(e => e.Salary),
              AverageCost = g.Average(e => e.Salary),
              MinCost = g.Min(e => e.Salary),
              MaxCost = g.Max(e => e.Salary),
            });
        query.Run();
      }
    }
  }
}
