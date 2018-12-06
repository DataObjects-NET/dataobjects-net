using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Tests.Storage.Performance.CrudModel;
using Xtensive.Orm.Tests;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Tests.Storage.Performance
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

    protected override sealed DomainConfiguration BuildConfiguration()
    {
      var config = CreateConfiguration();
      config.Sessions.Add(new SessionConfiguration("Default"));
      // config.Sessions.Default.CacheSize = BaseCount;
      config.Sessions.Default.CacheType = SessionCacheType.Infinite;
      config.Types.Register(typeof (Simplest).Assembly, typeof (Simplest).Namespace);
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
        TestLog.Info("Warming up...");
      InsertTest(insertCount);
      MaterializeTest(baseCount);
      MaterializeAnonymousTypeTest(baseCount);
      MaterializeGetFieldTest(baseCount);
      ManualMaterializeTest(baseCount);

      CreateSimplestContainer(insertCount);
      AccessToNonPairedEntitySetTest(collectionCount);
      AccessToPairedEntitySetTest(collectionCount);
      DeleteSimplestContainer();

      FetchTest(baseCount/2);
      PrefetchTest(baseCount/2);
      QueryTest(baseCount/5);
      SameQueryExpressionTest(baseCount/5);
      CachedQueryTest(baseCount/2);
      RseQueryTest(baseCount/5);
      CachedRseQueryTest(baseCount/5);
      UpdateTest();
      UpdateNoBatchingTest();
      SingleStatementLikeUpdateTest();
      RemoveTest();
    }

    private void InsertTest(int insertCount)
    {
      using (var session = Domain.OpenSession())
      using (session.Activate()) {
        TestHelper.CollectGarbage();
        using (warmup ? null : new Measurement("Insert", insertCount)) {
          using (var ts = session.OpenTransaction()) {
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
      using (var session = Domain.OpenSession())
      using (session.Activate()) {
        long sum = (long) count*(count - 1)/2;
        using (var ts = session.OpenTransaction()) {
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Fetch & GetField", count)) {
            for (int i = 0; i < count; i++) {
              var key = Key.Create<Simplest>(Domain, (long) i%instanceCount);
              var o = session.Query.SingleOrDefault<Simplest>(key);
              sum -= o.Id;
            }
            ts.Complete();
          }
        }
        if (count <= instanceCount)
          Assert.AreEqual(0, sum);
      }
    }

    private void PrefetchTest(int count)
    {
      using (var session = Domain.OpenSession())
      using (session.Activate()) {
        //long sum = (long)count*(count-1)/2;
        var i = 0;
        using (var ts = session.OpenTransaction()) {
          var keys = GetKeys(count).ToList();
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Prefetch", count)) {
            foreach (var key in session.Query.Many<Simplest>(keys)) {
              i++;
              //var o = session.Query.SingleOrDefault<Simplest>(key);
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
        yield return Key.Create<Simplest>(Domain, (long) i%instanceCount);
    }

    private void MaterializeTest(int count)
    {
      using (var session = Domain.OpenSession())
      using (session.Activate()) {
        int i = 0;
        using (var ts = session.OpenTransaction()) {
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Materialize", count)) {
            while (i < count)
              foreach (var o in session.Query.Execute(qe => qe.All<Simplest>())) {
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
      using (var session = Domain.OpenSession())
      using (session.Activate()) {
        int i = 0;
        using (var ts = session.OpenTransaction()) {
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Materialize anonymous type", count)) {
            while (i < count)
              foreach (var o in session.Query.Execute(qe => qe.All<Simplest>().Select(t => new {t.Id, t.Value}))) {
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
      using (var session = Domain.OpenSession())
      using (session.Activate()) {
        long sum = 0;
        int i = 0;
        using (var ts = session.OpenTransaction()) {
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Materialize & GetField", count)) {
            while (i < count)
              foreach (var o in session.Query.Execute(qe => qe.All<Simplest>())) {
                sum += o.Id;
                if (++i >= count)
                  break;
              }
            ts.Complete();
          }
        }
        Assert.AreEqual((long) count*(count - 1)/2, sum);
      }
    }

    private void ManualMaterializeTest(int count)
    {
      using (var session = Domain.OpenSession())
      using (session.Activate()) {
        int i = 0;
        using (var ts = session.OpenTransaction()) {
          var rs = Domain.Model.Types[typeof (Simplest)].Indexes.PrimaryIndex.GetQuery();
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Manual materialize", count)) {
            while (i < count) {
              foreach (var tuple in rs.GetRecordSet(session)) {
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
      using (var session = Domain.OpenSession())
      using (session.Activate()) {
        int i = 0;
        using (var ts = session.OpenTransaction()) {
          TestHelper.CollectGarbage();
          var message = string.Format("Access to non-paired EntitySet[{0} items]", EntitySetItemCount);
          using (warmup ? null : new Measurement(message, count)) {
            NonPairedSimplestContainerItem t = null;
            while (i < count)
              foreach (var o in session.Query.Execute(qe => qe.All<SimplestContainer>())) {
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
      using (var session = Domain.OpenSession())
      using (session.Activate()) {
        int i = 0;
        using (var ts = session.OpenTransaction()) {
          TestHelper.CollectGarbage();
          var message = string.Format("Access to paired EntitySet[{0} items]", EntitySetItemCount);
          using (warmup ? null : new Measurement(message, count)) {
            PairedSimplestContainerItem t = null;
            while (i < count)
              foreach (var o in session.Query.Execute(qe => qe.All<SimplestContainer>())) {
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
      int count = 0;
      using (var session = Domain.OpenSession())
      using (session.Activate()) {
        TestHelper.CollectGarbage();
        using (var ts = session.OpenTransaction()) {
          SimplestContainer owner = null;
          for (int i = 0; i < insertCount; i++) {
            if (i%EntitySetItemCount == 0) {
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
      using (var session = Domain.OpenSession()) {
        TestHelper.CollectGarbage();
        using (var ts = session.OpenTransaction()) {
          var query = session.Query.Execute(qe => qe.All<SimplestContainer>());
          foreach (var o in query)
            o.Remove();
          ts.Complete();
        }
      }
    }

    private void QueryTest(int count)
    {
      using (var session = Domain.OpenSession())
      using (session.Activate()) {
        using (var ts = session.OpenTransaction()) {
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Query", count)) {
            for (int i = 0; i < count; i++) {
              var id = i%instanceCount;
              var query = session.Query.All<Simplest>().Where(o => o.Id == id);
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
      using (var session = Domain.OpenSession())
      using (session.Activate()) {
        using (var ts = session.OpenTransaction()) {
          var id = 0;
          var query = session.Query.All<Simplest>().Where(o => o.Id == id);
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Single query expression", count)) {
            for (int i = 0; i < count; i++) {
              id = i%instanceCount;
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
      using (var session = Domain.OpenSession())
      using (session.Activate()) {
        using (var ts = session.OpenTransaction()) {
          var id = 0;
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Cached query", count)) {
            for (int i = 0; i < count; i++) {
              id = i%instanceCount;
              var query = session.Query.Execute(qe => qe.All<Simplest>().Where(o => o.Id == id));
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
      using (var session = Domain.OpenSession())
      using (session.Activate()) {
        using (var ts = session.OpenTransaction()) {
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("RSE query", count)) {
            for (int i = 0; i < count; i++) {
              var pKey = new Parameter<Tuple>();
              var rs = Domain.Model.Types[typeof (Simplest)].Indexes.PrimaryIndex.GetQuery().Seek(() => pKey.Value);
              using (new ParameterContext().Activate()) {
                pKey.Value = Tuple.Create((long) (i%instanceCount));
                var es = rs.GetRecordSet(session).ToEntities<Simplest>(0);
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
      using (var session = Domain.OpenSession())
      using (session.Activate()) {
        using (var ts = session.OpenTransaction()) {
          TestHelper.CollectGarbage();
          var pKey = new Parameter<Tuple>();
          var rs = Domain.Model.Types[typeof (Simplest)].Indexes.PrimaryIndex.GetQuery().Seek(() => pKey.Value);
          using (new ParameterContext().Activate()) {
            using (warmup ? null : new Measurement("Cached RSE query", count)) {
              for (int i = 0; i < count; i++) {
                pKey.Value = Tuple.Create((long) (i%instanceCount));
                var es = rs.GetRecordSet(session).ToEntities<Simplest>(0);
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
      using (var session = Domain.OpenSession())
      using (session.Activate()) {
        TestHelper.CollectGarbage();
        using (warmup ? null : new Measurement("Update", instanceCount)) {
          using (var ts = session.OpenTransaction()) {
            var query = session.Query.Execute(qe => qe.All<Simplest>());
            foreach (var o in query)
              o.Value++;
            ts.Complete();
          }
        }
      }
    }

    private void UpdateNoBatchingTest()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate()) {
        TestHelper.CollectGarbage();
        using (warmup ? null : new Measurement("Update (no batching)", instanceCount)) {
          using (var ts = session.OpenTransaction()) {
            var query = session.Query.Execute(qe => qe.All<Simplest>());
            foreach (var o in query) {
              o.Value = o.Value++;
              session.SaveChanges();
            }
            ts.Complete();
          }
        }
      }
    }

    private void SingleStatementLikeUpdateTest()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate()) {
        TestHelper.CollectGarbage();
        using (warmup ? null : new Measurement("Update (like DML query)", instanceCount)) {
          using (var ts = session.OpenTransaction()) {
            var query = session.Query.Execute(qe => qe.All<Simplest>());
            foreach (var o in query) {
              var value = o.Value;
              if (value >= 0)
                o.Value = -value;
            }
            ts.Complete();
          }
        }
      }
    }

    private void RemoveTest()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate()) {
        TestHelper.CollectGarbage();
        using (warmup ? null : new Measurement("Remove", instanceCount)) {
          using (var ts = session.OpenTransaction()) {
            var query = session.Query.Execute(qe => qe.All<Simplest>());
            foreach (var o in query)
              o.Remove();
            ts.Complete();
          }
        }
      }
    }
  }
}