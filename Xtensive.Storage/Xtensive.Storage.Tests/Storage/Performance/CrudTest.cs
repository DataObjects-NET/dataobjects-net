// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.09.08

using NUnit.Framework;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.CrudModel;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Tests.Storage.CrudModel
{
  [HierarchyRoot("Id")]
  public class Simplest : Entity
  {
    [Field]
    public long Id { get; private set; }

    [Field]
    public long Value { get; set; }

    public Simplest(long id, long value)
      : base (Tuple.Create(id))
    {
      Value = value;
    }
  }
}


namespace Xtensive.Storage.Tests.Storage
{
  [TestFixture]
  public class CrudTest : AutoBuildTest
  {
    public const int BaseCount = 1000;
    public const int InsertCount = BaseCount;
    public const int QueryCount = BaseCount / 5;
    private bool warmup  = false;
    private bool profile = false;
    private int instanceCount;

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = DomainConfigurationFactory.Create("mssql2005");
      config.Types.Register(typeof(Simplest).Assembly, typeof(Simplest).Namespace);
      return config;
    }

    [Test]
    public void RegularTest()
    {
      warmup = true;
      CombinedTest(10, 10, 10);
      warmup = false;
      CombinedTest(BaseCount, InsertCount, QueryCount);
    }

    [Test]
    [Explicit]
    [Category("Profile")]
    public void ProfileTest()
    {
      int instanceCount = 1000;
      int queryCount = 1000;
      InsertTest(instanceCount);
      FetchTest(instanceCount);
//      QueryTest(instanceCount, queryCount);
    }

    private void CombinedTest(int baseCount, int insertCount, int queryCount)
    {
      InsertTest(insertCount);
      FetchTest(baseCount);
      QueryTest(queryCount);
      RemoveTest();
    }

    private void InsertTest(int inserCount)
    {
      var d = Domain;
      using (var ss = d.OpenSession()) {
        var s = ss.Session;
        long sum = 0;
        using (var ts = s.OpenTransaction()) {
          using (warmup ? null : new Measurement("Insert", inserCount))
            for (int i = 0; i < inserCount; i++) {
              var o = new Simplest(i, i);
              sum += i;
            }
          ts.Complete();
        }
      }
      instanceCount = inserCount;
    }

    private void FetchTest(int count)
    {
      var d = Domain;
      using (var ss = d.OpenSession()) {
        var s = ss.Session;
        long sum = (long)count*(count-1)/2;
        using (var ts = s.OpenTransaction()) {
          using (warmup ? null : new Measurement("Fetch & GetField", count))
            for (int i = 0; i < count; i++) {
              var key = Key.Get<Simplest>(Tuple.Create((long)i % instanceCount));
              var o = key.Resolve<Simplest>();
              sum -= o.Id;
            }
          ts.Complete();
        }
        if (count<=instanceCount)
          Assert.AreEqual(0, sum);
      }
    }

    private void QueryTest(int count)
    {
      var d = Domain;
      using (var ss = d.OpenSession()) {
        var s = ss.Session;
        using (var ts = s.OpenTransaction()) {
          using (warmup ? null : new Measurement("Query", count)) {
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
          }
          ts.Complete();
        }
      }
    }

    private void RemoveTest()
    {
      var d = Domain;
      using (var ss = d.OpenSession()) {
        var s = ss.Session;
        using (var ts = s.OpenTransaction()) {
          var rs = d.Model.Types[typeof (Simplest)].Indexes.PrimaryIndex.ToRecordSet();
          var es = rs.ToEntities<Simplest>();
          using (warmup ? null : new Measurement("Remove", instanceCount))
            foreach (var o in es)
              o.Remove();
          ts.Complete();
        }
      }
    }
  }
}
