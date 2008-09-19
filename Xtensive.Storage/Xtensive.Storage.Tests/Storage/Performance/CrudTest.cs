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
    public const int InstanceCount = 1000;
    public const int QueryCount = 200;
    private bool warmup  = false;
    private bool profile = false;

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
      CombinedTest(10, 10);
      warmup = false;
      CombinedTest(InstanceCount, QueryCount);
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

    private void CombinedTest(int instanceCount, int queryCount)
    {
      InsertTest(instanceCount);
      FetchTest(instanceCount);
      QueryTest(instanceCount, queryCount);
      RemoveTest(instanceCount);
    }

    private void InsertTest(int count)
    {
      var d = Domain;
      using (var ss = d.OpenSession()) {
        var s = ss.Session;
        long sum = 0;
        using (var ts = s.OpenTransaction()) {
          using (warmup ? null : new Measurement("Insert", count))
            for (int i = 0; i < count; i++) {
              var o = new Simplest(i, i);
              sum += i;
            }
          ts.Complete();
        }
      }
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
              long id = i;
              var key = Key.Get<Simplest>(Tuple.Create(id));
              var o = key.Resolve<Simplest>();
              sum -= o.Id;
            }
          ts.Complete();
        }
        Assert.AreEqual(0, sum);
      }
    }

    private void QueryTest(int instanceCount, int queryCount)
    {
      var d = Domain;
      using (var ss = d.OpenSession()) {
        var s = ss.Session;
        using (var ts = s.OpenTransaction()) {
          using (warmup ? null : new Measurement("Query", queryCount)) {
            for (int i = 0; i < queryCount; i++) {
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

    private void RemoveTest(int count)
    {
      var d = Domain;
      using (var ss = d.OpenSession()) {
        var s = ss.Session;
        using (var ts = s.OpenTransaction()) {
          var rs = d.Model.Types[typeof (Simplest)].Indexes.PrimaryIndex.ToRecordSet();
          var es = rs.ToEntities<Simplest>();
          using (warmup ? null : new Measurement("Remove", count))
            foreach (var o in es)
              o.Remove();
          ts.Complete();
        }
      }
    }
  }
}