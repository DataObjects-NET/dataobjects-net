// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.03

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Orm.Tests.ObjectModel
{
  [TestFixture]
  public abstract class NorthwindDOModelTest : AutoBuildTest
  {
    private DisposableSet disposables;
    protected Session Session;

    private List<Customer> customers;
    private List<Order> orders;
    private List<Employee> employees;
    private List<Product> products;
    private List<Supplier> suppliers;

    protected IEnumerable<Customer> Customers {
      get {
        if (customers==null)
          customers = Session.Query.All<Customer>().ToList();
        return customers;
      }
    }

    protected IEnumerable<Order> Orders {
      get {
        if (orders==null)
          orders = Session.Query.All<Order>().ToList();
        return orders;
      }
    }

    protected IEnumerable<Employee> Employees {
      get {
        if (employees==null)
          employees = Session.Query.All<Employee>().ToList();
        return employees;
      }
    }

    protected IEnumerable<Product> Products {
      get {
        if (products==null)
          products = Session.Query.All<Product>().ToList();
        return products;
      }
    }

    protected IEnumerable<Supplier> Suppliers {
      get {
        if (suppliers==null)
          suppliers = Session.Query.All<Supplier>().ToList();
        return suppliers;
      }
    }

    [SetUp]
    public virtual void SetUp()
    {
      disposables = new DisposableSet();
      Session = Domain.OpenSession();
      disposables.Add(Session);
      disposables.Add(Session.OpenTransaction());
    }

    [TearDown]
    public virtual void TearDown()
    {
      disposables.DisposeSafely();
      customers = null;
      orders = null;
      employees = null;
      products = null;
      suppliers = null;
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof(Supplier).Assembly, typeof(Supplier).Namespace);
      return config;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      Domain domain;
      try {
        // throw new ApplicationException("Don't validate, just recreate ;)");
        var validateConfig = configuration.Clone();
        validateConfig.UpgradeMode = DomainUpgradeMode.Validate;
        domain = Domain.Build(validateConfig);
      } catch {
        var recreateConfig = configuration.Clone();
        recreateConfig.UpgradeMode = DomainUpgradeMode.Recreate;
        domain = base.BuildDomain(recreateConfig);
      }
      bool shouldFill = false;
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var count = session.Query.All<Customer>().Count();
        if (count == 0)
          shouldFill = true;
      }
      if (shouldFill)
        DataBaseFiller.Fill(domain);
      return domain;
    }
  }
}