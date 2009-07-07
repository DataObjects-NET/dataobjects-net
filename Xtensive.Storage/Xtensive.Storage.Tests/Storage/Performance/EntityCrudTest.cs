// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.23

using System.Data;
using System.Linq;
using System.Data.Objects;
using NUnit.Framework;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.Performance.EntityCrudModel;

namespace Xtensive.Storage.Tests.Storage.Performance
{
  [TestFixture]
  [Explicit]
  public class EntityCrudTest : AutoBuildTest
  {
    public const int BaseCount = 10000;
    public const int InsertCount = BaseCount;
    private bool warmup = false;
    private bool profile = false;
    private int instanceCount;

    protected override DomainConfiguration BuildConfiguration()
    {
      // Just to ensure schema is ready
      var config = DomainConfigurationFactory.Create("mssql2005");
      config.Types.Register(
        typeof (CrudModel.Simplest).Assembly, typeof (CrudModel.Simplest).Namespace);
      return config;
    }

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
      MaterializeTest(instanceCount);
    }

    private void CombinedTest(int baseCount, int insertCount)
    {
      InsertTest(insertCount);
      MaterializeTest(baseCount);
      FetchTest(baseCount / 2);
      QueryTest(baseCount / 5);
      CachedQueryTest(baseCount / 5);
      NoMaterializationQueryTest(baseCount / 5);
      CompiledQueryTest(baseCount / 5);
      UpdateTest();
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

    private void MaterializeTest(int count)
    {
      using (var dataContext = new Entities()) {
        dataContext.Connection.Open();
        long sum = 0;
        int i = 0;
        using (var transaction = dataContext.Connection.BeginTransaction()) {
          TestHelper.CollectGarbage();
          var simplest = dataContext.Simplest;
          using (warmup ? null : new Measurement("Materialize & GetField", count)) {
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
          using (warmup ? null : new Measurement("Query", count)) {
            for (int i = 0; i < count; i++) {
              var id = i % instanceCount;
              var result = dataContext.Simplest.Where(o => o.Id == id);
              foreach (var o in result) {
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
          var id = 0;
          var result = dataContext.Simplest.Where(o => o.Id == id);
          using (warmup ? null : new Measurement("Cached Query", count)) {
            for (int i = 0; i < count; i++) {
              id = i % instanceCount;
              foreach (var o in result) {
                // Doing nothing, just enumerate
              }
            }
            transaction.Commit();
          }
        }
      }
    }

    private void NoMaterializationQueryTest(int count)
    {
      using (var dataContext = new Entities()) {
        dataContext.Connection.Open();
        using (var transaction = dataContext.Connection.BeginTransaction()) {
          TestHelper.CollectGarbage();
          var id = 0;
          var result = dataContext.Simplest.Where(o => o.Id == id).Select(o => new { o.Id, o.Value });
          using (warmup ? null : new Measurement("No Materialization Query", count)) {
            for (int i = 0; i < count; i++) {
              id = i % instanceCount;
              foreach (var o in result) {
                // Doing nothing, just enumerate
              }
            }
            transaction.Commit();
          }
        }
      }
    }

    private void CompiledQueryTest(int count)
    {
      using (var dataContext = new Entities()) {
        dataContext.Connection.Open();
        using (var transaction = dataContext.Connection.BeginTransaction()) {
          TestHelper.CollectGarbage();
          var resultQuery = System.Data.Objects.CompiledQuery.Compile((Entities context, long id) => context.Simplest.Where(o => o.Id == id));
          using (warmup ? null : new Measurement("Compiled Query", count)) {
            for (int i = 0; i < count; i++) {
              var id = i % instanceCount;
              foreach (var o in resultQuery(dataContext, id)) {
                // Doing nothing, just enumerate
              }
            }
            transaction.Commit();
          }
        }
      }
    }

    private void UpdateTest()
    {
      using (var dataContext = new Entities()) {
        dataContext.Connection.Open();
        var simplest = dataContext.Simplest;
        TestHelper.CollectGarbage();
        using (warmup ? null : new Measurement("Update", instanceCount)) {
          using (var transaction = dataContext.Connection.BeginTransaction()) {
            foreach (var s in simplest)
              s.Value++;
            dataContext.SaveChanges();
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
            foreach (var s in simplest)
              dataContext.DeleteObject(s);
            dataContext.SaveChanges();
            transaction.Commit();
          }
        }
      }
    }
  }
}
