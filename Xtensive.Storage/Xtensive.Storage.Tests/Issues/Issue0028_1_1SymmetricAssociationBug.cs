// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.02.12

using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue0028_1_1SymmetricAssociationBug_Model;

namespace Xtensive.Storage.Tests.Issues.Issue0028_1_1SymmetricAssociationBug_Model
{
  [HierarchyRoot(typeof (KeyGenerator), "ID")]
  public class First : Entity
  {
    [Field]
    public long ID { get; private set; }

    //symmetric
    [Field(PairTo = "SPair")]
    public First SPair { get; set; }

    //assymetric
    [Field]
    public Second APair { get; set; }
  }

  [HierarchyRoot(typeof (KeyGenerator), "ID")]
  public class Second : Entity
  {
    [Field]
    public long ID { get; private set; }

    [Field(PairTo = "APair")]
    public First APair { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
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
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
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