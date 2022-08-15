// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System.Collections;
using System.Linq;
using NUnit.Framework;
using System.Reflection;
using Xtensive.Collections;

using Xtensive.Core;
using Xtensive.Orm.Tests.Upgrade.Sample3.Model.Version2;
using M1 = Xtensive.Orm.Tests.Upgrade.Sample3.Model.Version1;
using M2 = Xtensive.Orm.Tests.Upgrade.Sample3.Model.Version2;
using System;

namespace Xtensive.Orm.Tests.Upgrade.Sample3
{
  [TestFixture, Category("Upgrade")]
  public class Sample3UpgradeTest
  {
    [SetUp]
    public void SetUp()
    {
      using (var domain = BuildDomain("1", DomainUpgradeMode.Recreate)) {
        FillData(domain);
      }
    }
    
    [Test]
    public void UpgradeToVersion2Test()
    {
      using (var domain = BuildDomain("2", DomainUpgradeMode.Perform))
      using (var session = domain.OpenSession())
      using (session.OpenTransaction()) {
        Assert.AreEqual(1, session.Query.All<Employee>().Count());
        var emp = session.Query.All<Employee>().FirstOrDefault();
        Assert.AreEqual("Sales", emp.DepartmentName);
        var order = session.Query.All<Order>().FirstOrDefault();
        var productNames = order.Items.Select(item => item.ProductName).ToCommaDelimitedString();
        Console.WriteLine($"Order {{\tSeller = {order.Seller.FullName}\n\tProducts = {productNames}\n}}");
        // Console.WriteLine(order);
        Assert.AreEqual(1, order.Items.Count);
      }
    }

    private Domain BuildDomain(string version, DomainUpgradeMode upgradeMode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(Assembly.GetExecutingAssembly(),
        "Xtensive.Orm.Tests.Upgrade.Sample3.Model.Version" + version);
      configuration.Types.Register(typeof(Upgrader));
      using (Upgrader.Enable(version)) {
        return Domain.Build(configuration);
      }
    }

    #region Data filler

    private void FillData(Domain domain)
    {
      using (var session = domain.OpenSession())
      using (var transactionScope = session.OpenTransaction()) {
        var e1 = new Model.Version1.Employee {
          FirstName = "Andrew",
          LastName = "Fuller",
          Department = "Sales",
          IsHead = false
        };
        _ = new Model.Version1.Order {
          Amount = 1,
          ProductName = "P1",
          Seller = e1
        };
        transactionScope.Complete();
      }
    }

    #endregion
  }
}