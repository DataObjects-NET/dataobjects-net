// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.03.20

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Storage
{
  [TestFixture]
  public class ApplyTest : NorthwindDOModelTest
  {
    private List<Customer> allCustomers;
    private List<Order> allOrders;
    private RecordSet customerPrimary;
    private RecordSet orderPrimary;
    private int customerIdIndex;
    private int orderCustomerIndex;

    private void LoadData()
    {
      var customerType = Domain.Model.Types[typeof(Customer)];
      var orderType = Domain.Model.Types[typeof(Order)];

      var customerIdColumn = customerType.Fields["Id"].Column.Name;
      var orderCustomerColumn = orderType.Fields["Customer"].Fields[0].Column.Name;

      customerPrimary = customerType.Indexes.PrimaryIndex.ToRecordSet();
      orderPrimary = orderType.Indexes.PrimaryIndex.ToRecordSet();

      customerIdIndex = customerPrimary.Header.IndexOf(customerIdColumn);
      orderCustomerIndex = orderPrimary.Header.IndexOf(orderCustomerColumn);

      allCustomers = customerPrimary.ToEntities<Customer>().ToList();
      allOrders = orderPrimary.ToEntities<Order>().ToList();      
    }

    [Test]
    public void ApplyExistingTest()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          LoadData();
          long total = 0;
          foreach (var customer in allCustomers)
            if (allOrders.Any(o => o.Customer==customer))
              total++;
          var parameter = new ApplyParameter();
          var subquery = orderPrimary
            .Filter(t => t.GetValue(orderCustomerIndex)==parameter.Value.GetValue(customerIdIndex));
          var result = customerPrimary
            .Apply(parameter, subquery, ApplyType.Existing)
            .Count();
          Assert.AreEqual(total, result);
        }
      }
    }

    [Test]
    public void ApplyNotExistingTest()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          LoadData();
          long total = 0;
          foreach (var customer in allCustomers)
            if (!allOrders.Any(o => o.Customer == customer))
              total++;
          var parameter = new ApplyParameter();
          var subquery = orderPrimary
            .Filter(t => t.GetValue(orderCustomerIndex) == parameter.Value.GetValue(customerIdIndex));
          var result = customerPrimary
            .Apply(parameter, subquery, ApplyType.NotExisting)
            .Count();
          Assert.AreEqual(total, result);
        }
      }      
    }

    [Test]
    public void CrossApplyTest()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          LoadData();
          long total = 0;
          foreach (var customer in allCustomers)
            total += allOrders.Count(o => o.Customer==customer);
          var parameter = new ApplyParameter();
          var subquery = orderPrimary
            .Filter(t => t.GetValue(orderCustomerIndex)==parameter.Value.GetValue(customerIdIndex))
            .Alias("XYZ");
          var result = customerPrimary
            .Apply(parameter, subquery, ApplyType.Cross)
            .Count();
          Assert.AreEqual(total, result);
        }
      }
    }

    [Test]
    public void OuterApplyTest()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          LoadData();
          long total = 0;
          foreach (var customer in allCustomers)
            total += Math.Max(1, allOrders.Count(o => o.Customer == customer));
          var parameter = new ApplyParameter();
          var subquery = orderPrimary
            .Filter(t => t.GetValue(orderCustomerIndex) == parameter.Value.GetValue(customerIdIndex))
            .Alias("XYZ");
          var result = customerPrimary
            .Apply(parameter, subquery, ApplyType.Outer)
            .Count();
          Assert.AreEqual(total, result);
        }
      }      
    }

    [Test]
    public void CrossApplyExistenseTest()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          LoadData();
          long total = 0;
          foreach (var customer in allCustomers)
            if (allOrders.Any(o => o.Customer == customer))
              total++;
          var parameter = new ApplyParameter();
          var subquery = orderPrimary
            .Filter(t => t.GetValue(orderCustomerIndex)==parameter.Value.GetValue(customerIdIndex))
            .Existence("LALALA");
          var result = customerPrimary
            .Apply(parameter, subquery, ApplyType.Cross)
            .Count(t => (bool) t.GetValue(t.Count-1));
          Assert.AreEqual(total, result);
        }
      }
    }
  }
}
