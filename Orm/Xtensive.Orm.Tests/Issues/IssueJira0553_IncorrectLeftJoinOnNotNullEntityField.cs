// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.09.08

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0553_IncorrectLeftJoinOnNotNullEntityFieldModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0553_IncorrectLeftJoinOnNotNullEntityFieldModel
{
  [Serializable]
  [HierarchyRoot]
  public class Employee : Entity
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; private set; }
  }

  [Serializable]
  public class EmployeeWithCar : Employee
  {
    [Field(Nullable = false)]
    public Car Car { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Car : Entity
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class IssueJira0553_IncorrectLeftJoinOnNotNullEntityField : AutoBuildTest
  {
    SessionConfiguration configuration = new SessionConfiguration(SessionOptions.AutoActivation | SessionOptions.ServerProfile);
    protected override DomainConfiguration BuildConfiguration()
    {
      var domainConfiguration = base.BuildConfiguration();
      domainConfiguration.Types.Register(typeof(Car).Assembly, typeof(Car).Namespace);

      domainConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      return domainConfiguration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession(configuration))
      using (var t = session.OpenTransaction()) {
        var car = new Car();
        new EmployeeWithCar { Car = car };
        new Employee();
        new Employee();
        t.Complete();
      }
    }

    [Test]
    public void BadWorkTest()
    {
      using (var session = Domain.OpenSession(configuration))
      using (var t = session.OpenTransaction()) {
        var q = Session.Current.Query;

        var badResult = q.All<Employee>()
          .LeftJoin(
            q.All<EmployeeWithCar>(),
            e => e.Id,
            ewc => ewc.Id,
            (e, ewc) => new
            {
              e.Id,
              CarObject = ewc.Car
            });

        Assert.AreEqual(3, badResult.Count());
      }
    }

    [Test]
    public void GoodWorkTest()
    {
      using (var session = Domain.OpenSession(configuration))
      using (var transaction = session.OpenTransaction()) {
        var q = Session.Current.Query;

        var goodResult = q.All<Employee>()
                    .LeftJoin(
                        q.All<EmployeeWithCar>(),
                        e => e.Id,
                        ewc => ewc.Id,
                        (e, ewc) => new
                        {
                          e.Id,
                          Car = ewc.Car.Id
                        });
        Assert.AreEqual(3, goodResult.Count());
      }
    }

    [Test]
    public void WorkaroundTest()
    {
      using (var session = Domain.OpenSession(configuration))
      using (var transaction = session.OpenTransaction()) {
        var q = Session.Current.Query;

        var wordaround = q.All<Employee>()
                    .LeftJoin(
                        q.All<EmployeeWithCar>(),
                        e => e.Id,
                        ewc => ewc.Id,
                        (e, ewc) => new
                        {
                          e.Id,
                          CarId = ewc.Car.Id
                        })
                        .LeftJoin(
                            q.All<Car>(),
                            e => e.CarId,
                            c => c.Id,
                            (e, c) => new
                            {
                              e.Id,
                              Car = c
                            });
        Assert.AreEqual(3, wordaround.Count());
      }
    }
  }
}
