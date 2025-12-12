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
      config.Types.RegisterCaching(typeof (First).Assembly, typeof (First).Namespace);
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
          Assert.That(e1a.SPair, Is.EqualTo(e1a));
          e1b.SPair = e1a;
          Assert.That(e1b.SPair, Is.EqualTo(e1a));
          Assert.That(e1a.SPair, Is.EqualTo(e1b));
          e1c.SPair = e1b;
          Assert.That(e1b.SPair, Is.EqualTo(e1c));
          Assert.That(e1c.SPair, Is.EqualTo(e1b));
          Assert.That(e1a.SPair, Is.EqualTo(null));
          e1c.SPair = null;
          Assert.That(e1a.SPair, Is.EqualTo(null));
          Assert.That(e1b.SPair, Is.EqualTo(null));
          Assert.That(e1c.SPair, Is.EqualTo(null));
          Assert.That(e1d.SPair, Is.EqualTo(null));

          e1a.SPair = e1b;
          Assert.That(e1b.SPair, Is.EqualTo(e1a));
          Assert.That(e1a.SPair, Is.EqualTo(e1b));
          e1c.SPair = e1d;
          Assert.That(e1d.SPair, Is.EqualTo(e1c));
          Assert.That(e1c.SPair, Is.EqualTo(e1d));
          e1b.SPair = e1d;
          Assert.That(e1a.SPair, Is.EqualTo(null));
          Assert.That(e1b.SPair, Is.EqualTo(e1d));
          Assert.That(e1c.SPair, Is.EqualTo(null));
          Assert.That(e1d.SPair, Is.EqualTo(e1b));
          e1d.SPair = null;
          Assert.That(e1a.SPair, Is.EqualTo(null));
          Assert.That(e1b.SPair, Is.EqualTo(null));
          Assert.That(e1c.SPair, Is.EqualTo(null));
          Assert.That(e1d.SPair, Is.EqualTo(null));
          // Rollback
        }
      }
    }
  }
}