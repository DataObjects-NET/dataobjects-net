// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.03

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Disposing;
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
          customers = Query<Customer>.All.ToList();
        return customers;
      }
    }

    protected IEnumerable<Order> Orders {
      get {
        if (orders==null)
          orders = Query<Order>.All.ToList();
        return orders;
      }
    }

    protected IEnumerable<Employee> Employees {
      get {
        if (employees==null)
          employees = Query<Employee>.All.ToList();
        return employees;
      }
    }

    protected IEnumerable<Product> Products {
      get {
        if (products==null)
          products = Query<Product>.All.ToList();
        return products;
      }
    }

    protected IEnumerable<Supplier> Suppliers {
      get {
        if (suppliers==null)
          suppliers = Query<Supplier>.All.ToList();
        return suppliers;
      }
    }

    [SetUp]
    public virtual void SetUp()
    {
      disposables = new DisposableSet();
      disposables.Add(Domain.OpenSession());
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
      Domain domain = base.BuildDomain(configuration);
      DataBaseFiller.Fill(domain);
      return domain;
    }
    
  }
}