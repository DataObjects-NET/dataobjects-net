// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.03.20

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class ApplyTest : ChinookDOModelTest
  {
    private List<Customer> allCustomers;
    private List<Invoice> allInvoices;
    private CompilableProvider customerPrimary;
    private CompilableProvider orderPrimary;
    private int customerIdIndex;
    private int invoiceCustomerIndex;

    private void LoadData(Session session)
    {
      var customerType = Domain.Model.Types[typeof(Customer)];
      var invoiceType = Domain.Model.Types[typeof(Invoice)];

      var customerIdColumn = customerType.Fields["CustomerId"].Column.Name;
      var invoiceCustomerColumn = invoiceType.Fields["Customer"].Fields[0].Column.Name;

      customerPrimary = customerType.Indexes.PrimaryIndex.GetQuery();
      orderPrimary = invoiceType.Indexes.PrimaryIndex.GetQuery();

      customerIdIndex = customerPrimary.Header.IndexOf(customerIdColumn);
      invoiceCustomerIndex = orderPrimary.Header.IndexOf(invoiceCustomerColumn);

      allCustomers = customerPrimary.GetRecordSet(session).ToEntities<Customer>(0).ToList();
      allInvoices = orderPrimary.GetRecordSet(session).ToEntities<Invoice>(0).ToList();      
    }

    [Test]
    public void CrossApplyTest()
    {
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {
          LoadData(session);
          long total = 0;
          foreach (var customer in allCustomers)
            total += allInvoices.Count(o => o.Customer==customer);
          var parameter = new ApplyParameter();
          var subquery = orderPrimary
            .Filter(t => t.GetValue(invoiceCustomerIndex)==parameter.Value.GetValue(customerIdIndex))
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
          LoadData(session);
          long total = 0;
          foreach (var customer in allCustomers)
            total += Math.Max(1, allInvoices.Count(o => o.Customer == customer));
          var parameter = new ApplyParameter();
          var subquery = orderPrimary
            .Filter(t => t.GetValue(invoiceCustomerIndex) == parameter.Value.GetValue(customerIdIndex))
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
          LoadData(session);
          long total = 0;
          foreach (var customer in allCustomers)
            if (allInvoices.Any(o => o.Customer == customer))
              total++;
          var parameter = new ApplyParameter();
          var subquery = orderPrimary
            .Filter(t => t.GetValue(invoiceCustomerIndex)==parameter.Value.GetValue(customerIdIndex))
            .Existence("LALALA");
          var result = customerPrimary
            .Apply(parameter, subquery, false, ApplySequenceType.Single, JoinType.Inner)
            .GetRecordSet(Session.Current)
            .Count(t => (bool) t.GetValue(t.Count-1));
          Assert.AreEqual(total, result);
        }
      }
    }
  }
}
