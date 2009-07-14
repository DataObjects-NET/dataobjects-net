// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.03.20

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Rse;
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
    public void CrossApplyTest()
    {
      using (Session.Open(Domain)) {
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
            .Apply(parameter, subquery, ApplySequenceType.All, JoinType.Inner)
            .Count();
          Assert.AreEqual(total, result);
        }
      }
    }

    [Test]
    public void OuterApplyTest()
    {
      using (Session.Open(Domain)) {
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
            .Apply(parameter, subquery, ApplySequenceType.All, JoinType.LeftOuter)
            .Count();
          Assert.AreEqual(total, result);
        }
      }      
    }

    [Test]
    public void CrossApplyExistenseTest()
    {
      using (Session.Open(Domain)) {
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
            .Apply(parameter, subquery, ApplySequenceType.Single, JoinType.Inner)
            .Count(t => (bool) t.GetValue(t.Count-1));
          Assert.AreEqual(total, result);
        }
      }
    }
  }
}
