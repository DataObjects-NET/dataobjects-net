// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.23

using System.Data;
using System.Data.Objects;
using NUnit.Framework;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Testing;
using Xtensive.Storage.Tests.Storage.Performance.EntityCrudModel;

namespace Xtensive.Storage.Tests.Storage.Performance
{
  [TestFixture]
  public class EntityCrudTest
  {
    public const int BaseCount = 10000;
    public const int InsertCount = BaseCount;
    private bool warmup = false;
    private bool profile = false;
    private int instanceCount;

    [Test]
    public void RegularTest()
    {
      warmup = true;
      CombinedTest(10, 10);
      warmup = false;
      CombinedTest(BaseCount, InsertCount);
    }

    [Test]
    [Explicit]
    [Category("Profile")]
    public void ProfileTest()
    {
      int instanceCount = 10000;
      InsertTest(instanceCount);
      BulkFetchTest(instanceCount);
    }

    private void CombinedTest(int baseCount, int insertCount)
    {
      InsertTest(insertCount);
      BulkFetchTest(baseCount);
      FetchTest(baseCount / 2);
      QueryTest(baseCount / 5);
      CachedQueryTest(baseCount / 5);
      RemoveTest();
    }

    private void InsertTest(int insertCount)
    {
      using (var dataContext = new Entities()) {
        dataContext.Connection.Open();
        using (var transaction = dataContext.Connection.BeginTransaction()) {
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Insert", insertCount)) {
            for (int i = 0; i < insertCount; i++) {
              var s = Simplest.CreateSimplest(i, 0, i);
              dataContext.AddToSimplest(s);
            }
            dataContext.SaveChanges();
            transaction.Commit();
          }
        }
      }
      instanceCount = insertCount;
    }

    private void FetchTest(int count)
    {
      using (var dataContext = new Entities()) {
        dataContext.Connection.Open();
        long sum = (long) count * (count - 1) / 2;
        using (var transaction = dataContext.Connection.BeginTransaction()) {
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Fetch & GetField", count)) {
            for (int i = 0; i < count; i++) {
              var s = (Simplest) dataContext.GetObjectByKey(new EntityKey("Entities.Simplest", "Id", (long) i % instanceCount));
              sum -= s.Id;
            }
            transaction.Commit();
          }
        }
        if (count <= instanceCount)
          Assert.AreEqual(0, sum);
      }
    }

    private void BulkFetchTest(int count)
    {
      using (var dataContext = new Entities()) {
        dataContext.Connection.Open();
        long sum = 0;
        int i = 0;
        using (var transaction = dataContext.Connection.BeginTransaction()) {
          TestHelper.CollectGarbage();
          var simplest = dataContext.Simplest;
          using (warmup ? null : new Measurement("Bulk Fetch & GetField", count)) {
            while (i < count)
              foreach (var o in simplest) {
                sum += o.Id;
                if (++i >= count)
                  break;
              }
            transaction.Commit();
          }
        }
        Assert.AreEqual((long) count * (count - 1) / 2, sum);
      }
    }

    private void QueryTest(int count)
    {
      using (var dataContext = new Entities()) {
        dataContext.Connection.Open();
        using (var transaction = dataContext.Connection.BeginTransaction()) {
          TestHelper.CollectGarbage();
          var simplest = dataContext.Simplest;
          using (warmup ? null : new Measurement("Query", count)) {
            for (int i = 0; i < count; i++) {
              var pId = new ObjectParameter("pId", i % instanceCount);
              var query = simplest.Where("it.Id == @pId -- "+i+"\r\n", pId);
              foreach (var o in query) {
                // Doing nothing, just enumerate
              }
            }
            transaction.Commit();
          }
        }
      }
    }

    private void CachedQueryTest(int count)
    {
      using (var dataContext = new Entities()) {
        dataContext.Connection.Open();
        using (var transaction = dataContext.Connection.BeginTransaction()) {
          TestHelper.CollectGarbage();
          var simplest = dataContext.Simplest;
          var pId = new ObjectParameter("pId", 0);
          var query = simplest.Where("it.Id == @pId", pId);
          using (warmup ? null : new Measurement("Cached Query", count)) {
            for (int i = 0; i < count; i++) {
              pId.Value = i % instanceCount;
              foreach (var o in query) {
                // Doing nothing, just enumerate
              }
            }
            transaction.Commit();
          }
        }
      }
    }

    private void RemoveTest()
    {
      using (var dataContext = new Entities()) {
        dataContext.Connection.Open();
        var simplest = dataContext.Simplest;
        TestHelper.CollectGarbage();
        using (warmup ? null : new Measurement("Remove", instanceCount)) {
          using (var transaction = dataContext.Connection.BeginTransaction()) {
            foreach (var s in simplest) {
              dataContext.DeleteObject(s);
              dataContext.SaveChanges();
            }
            transaction.Commit();
          }
        }
      }
    }
  }
}
