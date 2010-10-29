// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.02.12

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0028_1_1SymmetricAssociationBug_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0028_1_1SymmetricAssociationBug_Model
{
  [Serializable]
  [HierarchyRoot]
  public class First : Entity
  {
    [Field, Key]
    public long ID { get; private set; }

    //symmetric
    [Field, Association(PairTo = "SPair")]
    public First SPair { get; set; }

    //assymetric
    [Field]
    public Second APair { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Second : Entity
  {
    [Field, Key]
    public long ID { get; private set; }

    [Field, Association(PairTo = "APair")]
    public First APair { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0028_1_1SymmetricAssociationBug : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (First).Assembly, typeof (First).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var e1a = new First();
          var e1b = new First();
          var e1c = new First();
          var e1d = new First();

          e1a.SPair = e1a;
          Assert.AreEqual(e1a, e1a.SPair);
          e1b.SPair = e1a;
          Assert.AreEqual(e1a, e1b.SPair);
          Assert.AreEqual(e1b, e1a.SPair);
          e1c.SPair = e1b;
          Assert.AreEqual(e1c, e1b.SPair);
          Assert.AreEqual(e1b, e1c.SPair);
          Assert.AreEqual(null, e1a.SPair);
          e1c.SPair = null;
          Assert.AreEqual(null, e1a.SPair);
          Assert.AreEqual(null, e1b.SPair);
          Assert.AreEqual(null, e1c.SPair);
          Assert.AreEqual(null, e1d.SPair);

          e1a.SPair = e1b;
          Assert.AreEqual(e1a, e1b.SPair);
          Assert.AreEqual(e1b, e1a.SPair);
          e1c.SPair = e1d;
          Assert.AreEqual(e1c, e1d.SPair);
          Assert.AreEqual(e1d, e1c.SPair);
          e1b.SPair = e1d;
          Assert.AreEqual(null, e1a.SPair);
          Assert.AreEqual(e1d, e1b.SPair);
          Assert.AreEqual(null, e1c.SPair);
          Assert.AreEqual(e1b, e1d.SPair);
          e1d.SPair = null;
          Assert.AreEqual(null, e1a.SPair);
          Assert.AreEqual(null, e1b.SPair);
          Assert.AreEqual(null, e1c.SPair);
          Assert.AreEqual(null, e1d.SPair);
          // Rollback
        }
      }
    }
  }
}