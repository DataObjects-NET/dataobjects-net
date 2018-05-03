using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
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

  // Reports
  // 1) How many people are on X floor
  // 2) How many people are on X floor and have Y Level
  // 3) How many people are on X floor and have salary more than Y
  // 4) How many people are on X floor and from Y department;

  // 5) How many people are in X office
  // 6) How many people are in X office and have Y Level
  // 7) How many people are in X office and what is their salary (expences per office)
  // 8) How many people are in X office are from Y department

  // 9) Average salary for office
  //10) Average salary for level
  //11) Average salary for department
  //12) Min and Max salary for office
  //13) Min and Max salary for level
  //14) Min and Max salary for department

  //15) Average floor number of X level
  //16) Min and Max floor number of X level
  //17) 




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
    //PersonSimplifiedByOffice
    public long OfficeNumber;
    public static Expression<Func<PersonFlatByOffice, bool>> IsBigOffice = e => e.OfficeNumber < 3;

    //PersonSimplified<PersonFlatByOffice>
    public static Expression<Func<PersonFlatByOffice, bool>> Require360Eval = e => e.Department != Department.Sales && (e.Level == Level.Junior || e.Level == Level.Senior);
    public static Expression<Func<PersonFlatByOffice, bool>> NeedProductTraining = e => e.Department != Department.It && e.Level != Level.Intern;

    //PersonSimplified
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
  public class IssueJira0627_FieldInitializationTest : AutoBuildTest
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
