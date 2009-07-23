using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Parameters;
using Xtensive.Core.Testing;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Tests.Storage.Performance.CrudModel;

namespace Xtensive.Storage.Tests.Storage.Performance
{
  public abstract class DOCrudTestBase : AutoBuildTest
  {
    private bool warmup;
    private int instanceCount;
    private const int BaseCount = 10000;
    private const int InsertCount = BaseCount;

    protected abstract DomainConfiguration CreateConfiguration();

    protected sealed override DomainConfiguration BuildConfiguration()
    {
      var config = CreateConfiguration();
      config.Sessions.Add(new SessionConfiguration("Default"));
      config.Sessions.Default.CacheSize = BaseCount;
      config.Sessions.Default.CacheType = SessionCacheType.Infinite;
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
      InsertTest(BaseCount);
      MaterializeTest(BaseCount);
    }

    private void CombinedTest(int baseCount, int insertCount)
    {
      if (warmup)
        Log.Info("Warming up...");
      InsertTest(insertCount);
      MaterializeTest(baseCount);
      MaterializeAnonymousTypeTest(baseCount);
      MaterializeGetFieldTest(baseCount);
      ManualMaterializeTest(baseCount);
      FetchTest(baseCount / 2);
      QueryTest(baseCount / 5);
      SameQueryExpressionTest(baseCount / 5);
      CachedQueryTest(baseCount / 2);
      RseQueryTest(baseCount / 5);
      CachedRseQueryTest(baseCount / 5);
      UpdateTest();
      RemoveTest();
    }

    private void InsertTest(int insertCount)
    {
      var d = Domain;
      using (var ss = Session.Open(d)) {
        var s = ss.Session;
        TestHelper.CollectGarbage();
        using (warmup ? null : new Measurement("Insert", insertCount)) {
          using (var ts = Transaction.Open()) {
            for (int i = 0; i < insertCount; i++)
              new Simplest(i, i);
            ts.Complete();
          }
        }
      }
      instanceCount = insertCount;
    }

    private void FetchTest(int count)
    {
      var d = Domain;
      using (var ss = Session.Open(d)) {
        var s = ss.Session;
        long sum = (long)count*(count-1)/2;
        using (var ts = Transaction.Open()) {
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Fetch & GetField", count)) {
            for (int i = 0; i < count; i++) {
              var key = Key.Create<Simplest>((long) i % instanceCount);
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

    private void MaterializeTest(int count)
    {
      var d = Domain;
      using (var ss = Session.Open(d)) {
        var s = ss.Session;
        int i = 0;
        using (var ts = Transaction.Open()) {
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Materialize", count)) {
            while (i < count)
              foreach (var o in CachedQuery.Execute(() => Query<Simplest>.All)) {
                if (++i >= count)
                  break;
              }
            ts.Complete();
          }
        }
      }
    }

    private void MaterializeAnonymousTypeTest(int count)
    {
      var d = Domain;
      using (var ss = Session.Open(d)) {
        var s = ss.Session;
        int i = 0;
        using (var ts = Transaction.Open()) {
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Materialize anonymous type", count)) {
            while (i < count)
              foreach (var o in CachedQuery.Execute(() => Query<Simplest>.All.Select(t => new {t.Id, t.Value}))) {
                if (++i >= count)
                  break;
              }
            ts.Complete();
          }
        }
      }
    }

    private void MaterializeGetFieldTest(int count)
    {
      var d = Domain;
      using (var ss = Session.Open(d)) {
        var s = ss.Session;
        long sum = 0;
        int i = 0;
        using (var ts = Transaction.Open()) {
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Materialize & GetField", count)) {
            while (i < count)
              foreach (var o in CachedQuery.Execute(() => Query<Simplest>.All)) {
                sum += o.Id;
                if (++i >= count)
                  break;
              }
            ts.Complete();
          }
        }
        Assert.AreEqual((long)count*(count-1)/2, sum);
      }
    }

    private void ManualMaterializeTest(int count)
    {
      var d = Domain;
      using (var ss = Session.Open(d)) {
        var s = ss.Session;
        int i = 0;
        using (var ts = Transaction.Open()) {
          var rs = d.Model.Types[typeof (Simplest)].Indexes.PrimaryIndex.ToRecordSet();
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Manual materialize", count)) {
            while (i < count) {
              foreach (var tuple in rs) {
                var o = new SqlClientCrudModel.Simplest 
                {
                  Id = tuple.GetValueOrDefault<long>(0), 
                  Value = tuple.GetValueOrDefault<long>(2)
                };
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
      using (var ss = Session.Open(d)) {
        var s = ss.Session;
        using (var ts = Transaction.Open()) {
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

    private void SameQueryExpressionTest(int count)
    {
      var d = Domain;
      using (var ss = Session.Open(d)) {
        var s = ss.Session;
        using (var ts = Transaction.Open()) {
          var id = 0;
          var query = Query<Simplest>.All.Where(o => o.Id == id);
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Single query expression", count)) {
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
      using (var ss = Session.Open(d)) {
        var s = ss.Session;
        using (var ts = Transaction.Open()) {
          var id = 0;
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Cached query", count)) {
            for (int i = 0; i < count; i++) {
              id = i % instanceCount;
              var query = CachedQuery.Execute(() => Query<Simplest>.All
                .Where(o => o.Id == id));
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
      using (var ss = Session.Open(d)) {
        var s = ss.Session;
        using (var ts = Transaction.Open()) {
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("RSE query", count)) {
            for (int i = 0; i < count; i++) {
              var pKey = new Parameter<Tuple>();
              var rs = d.Model.Types[typeof (Simplest)].Indexes.PrimaryIndex.ToRecordSet();
              rs = rs.Seek(() => pKey.Value);
              using (new ParameterContext().Activate()) {
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
      using (var ss = Session.Open(d)) {
        var s = ss.Session;
        using (var ts = Transaction.Open()) {
          TestHelper.CollectGarbage();
          var pKey = new Parameter<Tuple>();
          var rs = d.Model.Types[typeof (Simplest)].Indexes.PrimaryIndex.ToRecordSet();
          rs = rs.Seek(() => pKey.Value);
          using (new ParameterContext().Activate()) {
            using (warmup ? null : new Measurement("Cached RSE query", count)) {
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

    private void UpdateTest()
    {
      var d = Domain;
      using (var ss = Session.Open(d)) {
        var s = ss.Session;
        TestHelper.CollectGarbage();
        using (warmup ? null : new Measurement("Update", instanceCount)) {
          using (var ts = Transaction.Open()) {
            var query = CachedQuery.Execute(() => Query<Simplest>.All);
            foreach (var o in query)
              o.Value++;
            ts.Complete();
          }
        }
      }
    }

    private void RemoveTest()
    {
      var d = Domain;
      using (var ss = Session.Open(d)) {
        var s = ss.Session;
        TestHelper.CollectGarbage();
        using (warmup ? null : new Measurement("Remove", instanceCount)) {
          using (var ts = Transaction.Open()) {
            var query = CachedQuery.Execute(() => Query<Simplest>.All);
            foreach (var o in query)
              o.Remove();
            ts.Complete();
          }
        }
      }
    }
  }
}