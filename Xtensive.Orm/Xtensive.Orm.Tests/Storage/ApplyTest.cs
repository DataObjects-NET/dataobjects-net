// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.03.20

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Rse;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;
using Xtensive.Orm;

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class ApplyTest : NorthwindDOModelTest
  {
    private List<Customer> allCustomers;
    private List<Order> allOrders;
    private RecordQuery customerPrimary;
    private RecordQuery orderPrimary;
    private int customerIdIndex;
    private int orderCustomerIndex;

    private void LoadData()
    {
      var customerType = Domain.Model.Types[typeof(Customer)];
      var orderType = Domain.Model.Types[typeof(Order)];

      var customerIdColumn = customerType.Fields["Id"].Column.Name;
      var orderCustomerColumn = orderType.Fields["Customer"].Fields[0].Column.Name;

      customerPrimary = customerType.Indexes.PrimaryIndex.ToRecordQuery();
      orderPrimary = orderType.Indexes.PrimaryIndex.ToRecordQuery();

      customerIdIndex = customerPrimary.Header.IndexOf(customerIdColumn);
      orderCustomerIndex = orderPrimary.Header.IndexOf(orderCustomerColumn);

      allCustomers = customerPrimary.ToRecordSet(Session.Current).ToEntities<Customer>(0).ToList();
      allOrders = orderPrimary.ToRecordSet(Session.Current).ToEntities<Order>(0).ToList();      
    }

    [Test]
    public void CrossApplyTest()
    {
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {
          LoadData();
          long total = 0;
          foreach (var customer in allCustomers)
            total += allOrders.Count(o => o.Customer==customer);
          var parameter = new ApplyParameter();
          var subquery = orderPrimary
            .Filter(t => t.GetValue(orderCustomerIndex)==parameter.Value.GetValue(customerIdIndex))
            .Alias("XYZ");
          var result = customerPrimary
            .Apply(parameter, subquery)
            .Count(Session.Current);
          Assert.AreEqual(total, result);
        }
      }
    }

    [Test]
    public void OuterApplyTest()
    {
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {
          LoadData();
          long total = 0;
          foreach (var customer in allCustomers)
            total += Math.Max(1, allOrders.Count(o => o.Customer == customer));
          var parameter = new ApplyParameter();
          var subquery = orderPrimary
            .Filter(t => t.GetValue(orderCustomerIndex) == parameter.Value.GetValue(customerIdIndex))
            .Alias("XYZ");
          var result = customerPrimary
            .Apply(parameter, subquery, false, ApplySequenceType.All, JoinType.LeftOuter)
            .Count(Session.Current);
          Assert.AreEqual(total, result);
        }
      }      
    }

    [Test]
    public void CrossApplyExistenseTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {
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
            .Apply(parameter, subquery, false, ApplySequenceType.Single, JoinType.Inner)
            .ToRecordSet(Session.Current)
            .Count(t => (bool) t.GetValue(t.Count-1));
          Assert.AreEqual(total, result);
        }
      }
    }
  }
}
