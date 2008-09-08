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
    long Id { get; set; }

    [Field]
    Simplest Value { get; set; }

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
    public void InsertTest()
    {
      int c = InsertCount;
      using (var d = CreateDomain()) {
        using (var ss = d.OpenSession()) {
          var s = ss.Session;
          using (s.OpenTransaction()) {
            using (new Measurement("Insertion", c))
              for (int i = 0; i < c; i++) {
                var o = new Simplest(i, null);
              }
          }
        }
      }
    }
  }
}