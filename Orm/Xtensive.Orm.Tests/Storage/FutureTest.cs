// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.20

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public sealed class FutureTest : ChinookDOModelTest
  {
    private const decimal SearchedCommission = 0.1m;

    [Test]
    public void ExecutionTest()
    {
      using (var session = Domain.OpenSession())
      using (var ts = session.OpenTransaction()) {
        var futureHighComission = session.Query.ExecuteDelayed(
          qe => qe.All<Invoice>().Where(i => i.Commission > 0.1m));
        var futurePaid = session.Query.ExecuteDelayed(
          qe => qe.All<Invoice>().Where(i => i.Status==InvoiceStatus.Paid).Count());
        var futureSequenceTrack = session.Query.ExecuteDelayed(
          qe => qe.All<Track>().Where(p => p.Name.Contains("c")));
        var futureAvgComission = session.Query.ExecuteDelayed(
          qe => qe.All<Invoice>().Average(i => i.Commission));
        Assert.Greater(futureHighComission.Count(), 0); // Count() here is IEnumerable.Count()
        Assert.Greater(futurePaid.Value, 0);
        Assert.Greater(futureSequenceTrack.Count(), 0); // Count() here is IEnumerable.Count()
        Assert.Greater(futureAvgComission.Value, 0);
        ts.Complete();
      }
    }

    [Test]
    public void TransactionChangingTest()
    {
      IEnumerable<Invoice> futureHighComission;
      using (var session = Domain.OpenSession()) {
        using (var ts = session.OpenTransaction()) {
          futureHighComission = session.Query.ExecuteDelayed(qe => qe.All<Invoice>().Where(i => i.Commission > 0.1m));
          ts.Complete();
        }
        AssertEx.Throws<InvalidOperationException>(() => futureHighComission.GetEnumerator());
        using (session.OpenTransaction())
          AssertEx.Throws<InvalidOperationException>(() => futureHighComission.GetEnumerator());
      }
    }

    [Test]
    public void CachingFutureSequenceTest()
    {
      Func<QueryEndpoint,IQueryable<Invoice>> futureQueryDelegate = GetFutureSequenceQuery;
      using (var session = Domain.OpenSession())
      using (var ts = session.OpenTransaction()) {
        var futureSequenceOrder = session.Query.ExecuteDelayed(futureQueryDelegate);
        Assert.Greater(futureSequenceOrder.Count(), 0);
        ts.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var ts = session.OpenTransaction()) {
        var futureSequenceOrder = session.Query.ExecuteDelayed(futureQueryDelegate.Method, GetFutureSequenceQueryFake);
        Assert.Greater(futureSequenceOrder.Count(), 0);
        ts.Complete();
      }
    }

    [Test]
    public void CachingFutureScalarTest()
    {
      var cacheKey = new object();
      var invoiceStatus = InvoiceStatus.Paid;
      using (var session = Domain.OpenSession())
      using (var ts = session.OpenTransaction()) {
        var futurePaid = session.Query.ExecuteDelayed(cacheKey,
          qe => qe.All<Invoice>().Where(i => i.Status==invoiceStatus).Count());
        Assert.Greater(futurePaid.Value, 0);
        ts.Complete();
      }

      var t = 0;
      using (var session = Domain.OpenSession())
      using (var ts = session.OpenTransaction()) {
        var futureScalar = session.Query.ExecuteDelayed(cacheKey, qe => t);
        Assert.Greater(futureScalar.Value, 0);
        ts.Complete();
      }
    }

    private IQueryable<Invoice> GetFutureSequenceQuery(QueryEndpoint queryEndpoint)
    {
      return Session.Demand().Query.All<Invoice>().Where(o => o.Commission > SearchedCommission);
    }

    private IQueryable<Invoice> GetFutureSequenceQueryFake(QueryEndpoint queryEndpoint)
    {
      return null;
    }
  }
}