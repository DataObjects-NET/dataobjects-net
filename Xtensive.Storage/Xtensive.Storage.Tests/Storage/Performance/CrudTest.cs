// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.09.08

using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Parameters;
using Xtensive.Core.Testing;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Tests.Storage.Performance.CrudModel;
using System.Linq;

namespace Xtensive.Storage.Tests.Storage.Performance
{
  [TestFixture]
  public class CrudTest : AutoBuildTest
  {
    public const int BaseCount = 10000;
    public const int InsertCount = BaseCount;
    private bool warmup  = false;
    private bool profile = false;
    private int instanceCount;

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = DomainConfigurationFactory.Create("mssql2005");
//      var config = DomainConfigurationFactory.Create("memory");
      config.Types.Register(typeof(Simplest).Assembly, typeof(Simplest).Namespace);
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
      warmup = true;
      CombinedTest(10, 10);
      warmup = false;
//      CombinedTest(BaseCount * 10, BaseCount * 10);
      int instanceCount = 100000;
      InsertTest(instanceCount);
      CachedQueryTest(instanceCount / 5);
    }

    private void CombinedTest(int baseCount, int insertCount)
    {
      InsertTest(insertCount);
      BulkFetchTest(baseCount);
      BulkFetchCachedTest(baseCount);
      BulkFetchOnlyTest(baseCount);
      RawBulkFetchTest(baseCount);
      FetchTest(baseCount / 2);
      QueryTest(baseCount / 5);
      CachedQueryExpressionTest(baseCount / 5);
      CachedQueryTest(baseCount / 2);
      ProjectingCachedQueryTest(baseCount / 2);
      RseQueryTest(baseCount / 5);
      CachedRseQueryTest(baseCount / 5);
      RemoveTest();
    }

    private void InsertTest(int insertCount)
    {
      var d = Domain;
      using (var ss = d.OpenSession()) {
        var s = ss.Session;
        long sum = 0;
        TestHelper.CollectGarbage();
        using (warmup ? null : new Measurement("Insert", insertCount)) {
          using (var ts = s.OpenTransaction()) {
            for (int i = 0; i < insertCount; i++) {
              var o = new Simplest(i, i);
              sum += i;
            }
            ts.Complete();
          }
        }
      }
      instanceCount = insertCount;
    }

    private void FetchTest(int count)
    {
      var d = Domain;
      using (var ss = d.OpenSession()) {
        var s = ss.Session;
        long sum = (long)count*(count-1)/2;
        using (var ts = s.OpenTransaction()) {
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Fetch & GetField", count)) {
            for (int i = 0; i < count; i++) {
              var key = Key.Create<Simplest>(Tuple.Create((long) i % instanceCount));
              var o = key.Resolve<Simplest>();
              sum -= o.Id;
            }
            ts.Complete();
          }
        }
        if (count<=instanceCount)
          Assert.AreEqual(0, sum);
      }
    }

    private void RawBulkFetchTest(int count)
    {
      var d = Domain;
      using (var ss = d.OpenSession()) {
        var s = ss.Session;
        long sum = 0;
        int i = 0;
        using (var ts = s.OpenTransaction()) {
          var rs = d.Model.Types[typeof (Simplest)].Indexes.PrimaryIndex.ToRecordSet();
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Raw Bulk Fetch & GetField", count)) {
            while (i < count) {
              foreach (var tuple in rs) {
                var o = new SqlClientCrudModel.Simplest {
                  Id = tuple.GetValueOrDefault<long>(0), 
                  Value = tuple.GetValueOrDefault<long>(2)
                };
                sum += o.Id;
                if (++i >= count)
                  break;
              }
            }
            ts.Complete();
          }
        }
        Assert.AreEqual((long) count * (count - 1) / 2, sum);
      }
    }

    private void BulkFetchTest(int count)
    {
      var d = Domain;
      using (var ss = d.OpenSession()) {
        var s = ss.Session;
        long sum = 0;
        int i = 0;
        using (var ts = s.OpenTransaction()) {
          var rs = d.Model.Types[typeof (Simplest)].Indexes.PrimaryIndex.ToRecordSet();
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Bulk Fetch & GetField", count)) {
            while (i<count) {
              foreach (var o in rs.ToEntities<Simplest>()) {
                sum += o.Id;
                if (++i >= count)
                  break;
              }
            }
            ts.Complete();
          }
        }
        Assert.AreEqual((long)count*(count-1)/2, sum);
      }
    }

    private void BulkFetchCachedTest(int count)
    {
      var d = Domain;
      using (var ss = d.OpenSession()) {
        var s = ss.Session;
        long sum = 0;
        int i = 0;
        var entities = new List<Entity>(count/2);
        var rs = d.Model.Types[typeof(Simplest)].Indexes.PrimaryIndex.ToRecordSet();
        using (var ts = s.OpenTransaction()) {
          while (i<count) {
            foreach (var o in rs.ToEntities<Simplest>()) {
              sum += o.Id;
              if (i % 2 == 0)
                entities.Add(o);
              if (++i >= count)
                break;
            }
//          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Bulk Fetch Cached", count / 2)) {
            foreach (var entity in entities)
              entity.Key.Resolve();
          }
          ts.Complete();
          }
        }
        Assert.AreEqual((long)count*(count-1)/2, sum);
      }
    }

    private void BulkFetchOnlyTest(int count)
    {
      var d = Domain;
      using (var ss = d.OpenSession()) {
        var s = ss.Session;
        long sum = 0;
        int i = 0;
        using (var ts = s.OpenTransaction()) {
          var rs = d.Model.Types[typeof (Simplest)].Indexes.PrimaryIndex.ToRecordSet();
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Bulk Fetch", count)) {
            while (i<count) {
              foreach (var o in rs.ToEntities<Simplest>()) {
                if (++i >= count)
                  break;
              }
            }
            ts.Complete();
          }
        }
      }
    }

    private void QueryTest(int count)
    {
      var d = Domain;
      using (var ss = d.OpenSession()) {
        var s = ss.Session;
        using (var ts = s.OpenTransaction()) {
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Query", count)) {
            for (int i = 0; i < count; i++) {
              var id = i % instanceCount;
              var query = Query<Simplest>.All.Where(o => o.Id == id);
              foreach (var simplest in query) {
                // Doing nothing, just enumerate
              }
            }
            ts.Complete();
          }
        }
      }
    }

    private void CachedQueryExpressionTest(int count)
    {
      var d = Domain;
      using (var ss = d.OpenSession()) {
        var s = ss.Session;
        using (var ts = s.OpenTransaction()) {
          var id = 0;
          var query = Query<Simplest>.All.Where(o => o.Id == id);
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Cached Query Expression", count)) {
            for (int i = 0; i < count; i++) {
              id = i % instanceCount;
              foreach (var simplest in query) {
                // Doing nothing, just enumerate
              }
            }
            ts.Complete();
          }
        }
      }
    }

    private void CachedQueryTest(int count)
    {
      var d = Domain;
      using (var ss = d.OpenSession()) {
        var s = ss.Session;
        using (var ts = s.OpenTransaction()) {
          var id = 0;
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Cached Query", count)) {
            for (int i = 0; i < count; i++) {
              id = i % instanceCount;
              var query = CachedQuery.Execute(() => Query<Simplest>.All.Where(o => o.Id==id));
              foreach (var simplest in query) {
                // Doing nothing, just enumerate
              }
            }
            ts.Complete();
          }
        }
      }
    }

    private void ProjectingCachedQueryTest(int count)
    {
      var d = Domain;
      using (var ss = d.OpenSession()) {
        var s = ss.Session;
        using (var ts = s.OpenTransaction()) {
          var id = 0;
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Projecting Cached Query", count)) {
            for (int i = 0; i < count; i++) {
              id = i % instanceCount;
              var query = CachedQuery.Execute(() => Query<Simplest>.All
                .Where(o => o.Id == id)
                .Select(o => new {o.Id, o.Value}));
              foreach (var simplest in query) {
                // Doing nothing, just enumerate
              }
            }
            ts.Complete();
          }
        }
      }
    }

    private void RseQueryTest(int count)
    {
      var d = Domain;
      using (var ss = d.OpenSession()) {
        var s = ss.Session;
        using (var ts = s.OpenTransaction()) {
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("RSE Query", count)) {
            for (int i = 0; i < count; i++) {
              var pKey = new Parameter<Tuple>();
              var rs = d.Model.Types[typeof (Simplest)].Indexes.PrimaryIndex.ToRecordSet();
              rs = rs.Seek(() => pKey.Value);
              using (new ParameterScope()) {
                pKey.Value = Tuple.Create(i % instanceCount);
                var es = rs.ToEntities<Simplest>();
                foreach (var o in es) {
                  // Doing nothing, just enumerate
                }
              }
            }
            ts.Complete();
          }
        }
      }
    }

    private void CachedRseQueryTest(int count)
    {
      var d = Domain;
      using (var ss = d.OpenSession()) {
        var s = ss.Session;
        using (var ts = s.OpenTransaction()) {
          TestHelper.CollectGarbage();
          var pKey = new Parameter<Tuple>();
          var rs = d.Model.Types[typeof (Simplest)].Indexes.PrimaryIndex.ToRecordSet();
          rs = rs.Seek(() => pKey.Value);
          using (new ParameterScope()) {
            using (warmup ? null : new Measurement("Cached RSE Query", count)) {
              for (int i = 0; i < count; i++) {
                pKey.Value = Tuple.Create(i % instanceCount);
                var es = rs.ToEntities<Simplest>();
                foreach (var o in es) {
                  // Doing nothing, just enumerate
                }
              }
            }
            ts.Complete();
          }
        }
      }
    }

    private void RemoveTest()
    {
      var d = Domain;
      using (var ss = d.OpenSession()) {
        var s = ss.Session;
        TestHelper.CollectGarbage();
        using (warmup ? null : new Measurement("Remove", instanceCount)) {
          using (var ts = s.OpenTransaction()) {
            var rs = d.Model.Types[typeof (Simplest)].Indexes.PrimaryIndex.ToRecordSet();
            var es = rs.ToEntities<Simplest>();
            foreach (var o in es)
              o.Remove();
            ts.Complete();
          }
        }
      }
    }
  }
}
