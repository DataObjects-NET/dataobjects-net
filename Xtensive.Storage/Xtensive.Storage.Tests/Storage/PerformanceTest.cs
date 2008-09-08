// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.09.08

using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Diagnostics;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Storage
{
  [HierarchyRoot("Id")]
  public class Simplest : Entity
  {
    [Field]
    public long Id { get; set; }

    [Field]
    public Simplest Value { get; set; }

    public Simplest(long id, Simplest value)
    {
      Id = id;
      Value = value;
    }
  }

  [TestFixture]
  public class PerformanceTest
  {
    public const int BaseIterationCount = 1000;
    public const int InsertCount = 100*BaseIterationCount;

    public Domain CreateDomain()
    {
      var c = new DomainConfiguration();
      c.Types.Register(typeof (Simplest).Assembly, typeof (Simplest).Namespace);
      c.ConnectionInfo = new UrlInfo("memory://localhost/");
      return Domain.Build(c);
    }

    [Test]
    public void CombinedTest()
    {
      int c = InsertCount;
      using (var d = CreateDomain()) {
        using (var ss = d.OpenSession()) {
          var s = ss.Session;
          long sum = 0;
          using (s.OpenTransaction()) {
            using (new Measurement("Insert", c))
              for (int i = 0; i < c; i++) {
                var o = new Simplest(i, null);
                sum += i;
              }
          }
          using (s.OpenTransaction()) {
            var rs = d.Model.Types[typeof (Simplest)].Indexes.PrimaryIndex.ToRecordSet();
            var es = rs.ToEntities<Simplest>();
            using (new Measurement("Fetch & GetField", c))
              foreach (var o in es) {
                sum -= o.Id;
              }
          }
          Assert.AreEqual(0, sum);
        }
      }
    }
  }
}