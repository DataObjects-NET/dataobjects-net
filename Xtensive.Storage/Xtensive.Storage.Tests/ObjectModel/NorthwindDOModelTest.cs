// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.03

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Disposing;
using Xtensive.Core;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.ObjectModel
{
  [TestFixture]
  public abstract class NorthwindDOModelTest : AutoBuildTest
  {
    private DisposableSet disposables;

    private List<Customer> customers;
    private List<Order> orders;
    private List<Employee> employees;
    private List<Product> products;
    private List<Supplier> suppliers;

    protected IEnumerable<Customer> Customers {
      get {
        if (customers==null)
          customers = Query.All<Customer>().ToList();
        return customers;
      }
    }

    protected IEnumerable<Order> Orders {
      get {
        if (orders==null)
          orders = Query.All<Order>().ToList();
        return orders;
      }
    }

    protected IEnumerable<Employee> Employees {
      get {
        if (employees==null)
          employees = Query.All<Employee>().ToList();
        return employees;
      }
    }

    protected IEnumerable<Product> Products {
      get {
        if (products==null)
          products = Query.All<Product>().ToList();
        return products;
      }
    }

    protected IEnumerable<Supplier> Suppliers {
      get {
        if (suppliers==null)
          suppliers = Query.All<Supplier>().ToList();
        return suppliers;
      }
    }

    [SetUp]
    public virtual void SetUp()
    {
      disposables = new DisposableSet();
      disposables.Add(Session.Open(Domain));
      disposables.Add(Transaction.Open());
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
      using (Session.Open(domain))
      using (var t = Transaction.Open()) {
        var count = Query.All<Customer>().Count();
        if (count == 0)
          shouldFill = true;
      }
      if (shouldFill)
        DataBaseFiller.Fill(domain);
      return domain;
    }
  }
}