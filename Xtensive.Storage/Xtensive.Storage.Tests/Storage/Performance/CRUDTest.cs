// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.09.08

using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Diagnostics;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Storage
{
  [HierarchyRoot(typeof(Generator), "Id")]
  public class Simplest : Entity
  {
    [Field]
    public long Id { get; set; }

    [Field]
    public long Value { get; set; }

    public Simplest(long id, long value)
    {
      Id = id;
      Value = value;
    }
  }

  [TestFixture]
  public class CRUDTest : AutoBuildTest
  {
    public const int OperationsCount = 10000;

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = DomainConfigurationFactory.Create("mssql2005");
      config.Types.Register(typeof(Simplest).Assembly, typeof(Simplest).Namespace);
      return config;
    }

    [Test]
    public void WarmingTest()
    {
      Combined(0,1);
    }

    [Test]
    public void RegularTest()
    {
      Combined(1,OperationsCount);
    }

    private void Combined(int start, int count)
    {
      using (var d = Domain) {
        using (var ss = d.OpenSession()) {
          var s = ss.Session;
          long sum = 0;
          using (var ts = s.OpenTransaction()) {
            using (new Measurement("Insert", count))
              for (int i = 0; i < count; i++) {
                var o = new Simplest(i+start, i);
                sum += i;
              }
            ts.Complete();
          }

          using (var ts = s.OpenTransaction()) {
            var rs = d.Model.Types[typeof (Simplest)].Indexes.PrimaryIndex.ToRecordSet();
            var es = rs.ToEntities<Simplest>();
            using (new Measurement("Fetch & GetField", count))
              foreach (var o in es) {
                sum -= o.Id;
              }
            ts.Complete();
          }
          Assert.AreEqual(0, sum);

          using (var ts = s.OpenTransaction()) {
            var rs = d.Model.Types[typeof (Simplest)].Indexes.PrimaryIndex.ToRecordSet();
            var es = rs.ToEntities<Simplest>();
            using (new Measurement("Fetch & Remove", count))
              foreach (var o in es)
                o.Remove();
            ts.Complete();
          }
        }
      }
    }
  }
}