using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Parameters;
using Xtensive.Core.Testing;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Tests.Storage.Performance.CrudModel;

namespace Xtensive.Storage.Tests.Storage.Performance
{
  public abstract class DoCrudTest : AutoBuildTest
  {
    private const int BaseCount = 50000;
    private const int InsertCount = BaseCount;
    private const int EntitySetItemCount = 10;

    private bool warmup;
    private int instanceCount;
    private int collectionCount;

    protected abstract DomainConfiguration CreateConfiguration();

    protected sealed override DomainConfiguration BuildConfiguration()
    {
      var config = CreateConfiguration();
      config.Sessions.Add(new SessionConfiguration("Default"));
      // config.Sessions.Default.CacheSize = BaseCount;
      config.Sessions.Default.CacheType = SessionCacheType.Infinite;
      config.Types.Register(typeof(Simplest).Assembly, typeof(Simplest).Namespace);
      return config;
    }

    [Test]
    public void RegularTest()
    {
      warmup = true;
      CombinedTest(10, EntitySetItemCount + 1);
      warmup = false;
      CombinedTest(BaseCount, InsertCount);
    }

    [Test]
    [Explicit]
    [Category("Profile")]
    public void ProfileTest()
    {
      warmup = true;
      CombinedTest(10, EntitySetItemCount + 1);
      warmup = false;
      InsertTest(BaseCount);
//      FetchTest(BaseCount / 2);
//      PrefetchTest(BaseCount / 2);
//      MaterializeTest(BaseCount);
//      UpdateTest();
//      RemoveTest();
      CreateSimplestContainer(BaseCount);
      AccessToPairedEntitySetTest(collectionCount);
      AccessToNonPairedEntitySetTest(collectionCount);
      DeleteSimplestContainer();
    }

    [Test]
    [Explicit]
    [Category("Profile")]
    public void UpdatePerformanceTest()
    {
      warmup = true;
      CombinedTest(10, 10);
      warmup = false;
      InsertTest(1000000);
      SingleStatementLikeUpdateTest();
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
      
      CreateSimplestContainer(insertCount);
      AccessToNonPairedEntitySetTest(collectionCount);
      AccessToPairedEntitySetTest(collectionCount);
      DeleteSimplestContainer();

      FetchTest(baseCount / 2);
      PrefetchTest(baseCount / 2);
      QueryTest(baseCount / 5);
      SameQueryExpressionTest(baseCount / 5);
      CachedQueryTest(baseCount / 2);
      RseQueryTest(baseCount / 5);
      CachedRseQueryTest(baseCount / 5);
      UpdateTest();
      UpdateNoBatchingTest();
      SingleStatementLikeUpdateTest();
      RemoveTest();
    }

    private void InsertTest(int insertCount)
    {
      var d = Domain;
      using (var ss = Session.Open(d)) {
        var s = ss;
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
        var s = ss;
        long sum = (long)count*(count-1)/2;
        using (var ts = Transaction.Open()) {
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Fetch & GetField", count)) {
            for (int i = 0; i < count; i++) {
              var key = Key.Create<Simplest>((long) i % instanceCount);
              var o = Query<Simplest>.SingleOrDefault(key);
              sum -= o.Id;
            }
            ts.Complete();
          }
        }
        if (count<=instanceCount)
          Assert.AreEqual(0, sum);
      }
    }

    private void PrefetchTest(int count)
    {
      var d = Domain;
      using (var ss = Session.Open(d)) {
        var s = ss;
        //long sum = (long)count*(count-1)/2;
        var i = 0;
        using (var ts = Transaction.Open()) {
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Prefetch", count)) {
            var keys = GetKeys(count).Prefetch<Simplest, Key>(key => key);
            foreach (var key in keys) {
              i++;
              //var o = Query<Simplest>.SingleOrDefault(key);
              //sum -= o.Id;
            }
            ts.Complete();
          }
        }
        Assert.AreEqual(count, i);
        //if (count<=instanceCount)
          //Assert.AreEqual(0, sum);
      }
    }

    private IEnumerable<Key> GetKeys(int count)
    {
      for (int i = 0; i < count; i++)
        yield return Key.Create<Simplest>((long) i % instanceCount);
    }

    private void MaterializeTest(int count)
    {
      var d = Domain;
      using (var ss = Session.Open(d)) {
        var s = ss;
        int i = 0;
        using (var ts = Transaction.Open()) {
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Materialize", count)) {
            while (i < count)
              foreach (var o in Query.Execute(() => Query<Simplest>.All)) {
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
        var s = ss;
        int i = 0;
        using (var ts = Transaction.Open()) {
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Materialize anonymous type", count)) {
            while (i < count)
              foreach (var o in Query.Execute(() => Query<Simplest>.All.Select(t => new {t.Id, t.Value}))) {
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
        var s = ss;
        long sum = 0;
        int i = 0;
        using (var ts = Transaction.Open()) {
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Materialize & GetField", count)) {
            while (i < count)
              foreach (var o in Query.Execute(() => Query<Simplest>.All)) {
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
        var s = ss;
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

    private void AccessToNonPairedEntitySetTest(int count)
    {
      var d = Domain;
      using (var ss = Session.Open(d)) {
        var s = ss;
        int i = 0;
        using (var ts = Transaction.Open()) {
          TestHelper.CollectGarbage();
          var message = string.Format("Access to non-paired EntitySet[{0} items]", EntitySetItemCount);
          using (warmup ? null : new Measurement(message, count)) {
            NonPairedSimplestContainerItem t = null;
            while (i < count)
              foreach (var o in Query.Execute(() => Query<SimplestContainer>.All)) {
                t = ((IEnumerable<NonPairedSimplestContainerItem>) o.DistantItems).First();
                if (++i >= count)
                  break;
              }
            Assert.Greater(t.Id, -1);
            ts.Complete();
          }
        }
      }
    }

    private void AccessToPairedEntitySetTest(int count)
    {
      var d = Domain;
      using (var ss = Session.Open(d)) {
        var s = ss;
        int i = 0;
        using (var ts = Transaction.Open()) {
          TestHelper.CollectGarbage();
          var message = string.Format("Access to paired EntitySet[{0} items]", EntitySetItemCount);
          using (warmup ? null : new Measurement(message, count)) {
            PairedSimplestContainerItem t = null;
            while (i < count)
              foreach (var o in Query.Execute(() => Query<SimplestContainer>.All)) {
                t = ((IEnumerable<PairedSimplestContainerItem>) o.Items).First();
                if (++i >= count)
                  break;
              }
            Assert.Greater(t.Id, -1);
            ts.Complete();
          }
        }
      }
    }

    private void CreateSimplestContainer(int insertCount)
    {
      var d = Domain;
      int count = 0;
      using (var ss = Session.Open(d)) {
        var s = ss;
        TestHelper.CollectGarbage();
        using (var ts = Transaction.Open()) {
          SimplestContainer owner = null;
          for (int i = 0; i < insertCount; i++) {
            if (i % EntitySetItemCount == 0) {
              owner = new SimplestContainer();
              count++;
            }
            owner.DistantItems.Add(new NonPairedSimplestContainerItem());
            owner.Items.Add(new PairedSimplestContainerItem());
          }
          ts.Complete();
        }
      }
      collectionCount = count;
    }

    private void DeleteSimplestContainer()
    {
      var d = Domain;
      using (var ss = Session.Open(d)) {
        var s = ss;
        TestHelper.CollectGarbage();
        using (var ts = Transaction.Open()) {
          var query = Query.Execute(() => Query<SimplestContainer>.All);
          foreach (var o in query)
            o.Remove();
          ts.Complete();
        }
      }
    }

    private void QueryTest(int count)
    {
      var d = Domain;
      using (var ss = Session.Open(d)) {
        var s = ss;
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
        var s = ss;
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
        var s = ss;
        using (var ts = Transaction.Open()) {
          var id = 0;
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Cached query", count)) {
            for (int i = 0; i < count; i++) {
              id = i % instanceCount;
              var query = Query.Execute(() => Query<Simplest>.All
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
        var s = ss;
        using (var ts = Transaction.Open()) {
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("RSE query", count)) {
            for (int i = 0; i < count; i++) {
              var pKey = new Parameter<Tuple>();
              var rs = d.Model.Types[typeof (Simplest)].Indexes.PrimaryIndex.ToRecordSet();
              rs = rs.Seek(() => pKey.Value);
              using (new ParameterContext().Activate()) {
                pKey.Value = Tuple.Create((long)(i % instanceCount));
                var es = rs.ToEntities<Simplest>(0);
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
        var s = ss;
        using (var ts = Transaction.Open()) {
          TestHelper.CollectGarbage();
          var pKey = new Parameter<Tuple>();
          var rs = d.Model.Types[typeof (Simplest)].Indexes.PrimaryIndex.ToRecordSet();
          rs = rs.Seek(() => pKey.Value);
          using (new ParameterContext().Activate()) {
            using (warmup ? null : new Measurement("Cached RSE query", count)) {
              for (int i = 0; i < count; i++) {
                pKey.Value = Tuple.Create((long)(i % instanceCount));
                var es = rs.ToEntities<Simplest>(0);
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
        var s = ss;
        TestHelper.CollectGarbage();
        using (warmup ? null : new Measurement("Update", instanceCount)) {
          using (var ts = Transaction.Open()) {
            var query = Query.Execute(() => Query<Simplest>.All);
            foreach (var o in query)
              o.Value++;
            ts.Complete();
          }
        }
      }
    }

    private void UpdateNoBatchingTest()
    {
      var d = Domain;
      using (var ss = Session.Open(d)) {
        var s = ss;
        TestHelper.CollectGarbage();
        using (warmup ? null : new Measurement("Update (no batching)", instanceCount)) {
          using (var ts = Transaction.Open()) {
            var query = Query.Execute(() => Query<Simplest>.All);
            foreach (var o in query) {
              o.Value = o.Value++;
              s.Persist();
            }
            ts.Complete();
          }
        }
      }
    }

    private void SingleStatementLikeUpdateTest()
    {
      var d = Domain;
      using (var ss = Session.Open(d)) {
        var s = ss;
        TestHelper.CollectGarbage();
        using (warmup ? null : new Measurement("Update (like DML query)", instanceCount)) {
          using (var ts = Transaction.Open()) {
            var query = Query.Execute(() => Query<Simplest>.All);
            foreach (var o in query) {
              var value = o.Value;
              if (value>=0)
                o.Value = -value;
            }
            ts.Complete();
          }
        }
      }
    }

    private void RemoveTest()
    {
      var d = Domain;
      using (var ss = Session.Open(d)) {
        var s = ss;
        TestHelper.CollectGarbage();
        using (warmup ? null : new Measurement("Remove", instanceCount)) {
          using (var ts = Transaction.Open()) {
            var query = Query.Execute(() => Query<Simplest>.All);
            foreach (var o in query)
              o.Remove();
            ts.Complete();
          }
        }
      }
    }
  }
}