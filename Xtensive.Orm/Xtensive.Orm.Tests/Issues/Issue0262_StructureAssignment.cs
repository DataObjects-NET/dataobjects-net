// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.06.29

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0262_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0262_Model
{
  [Serializable]
  [HierarchyRoot]
  public class Container : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public Pair Value { get; set; }
  }

  [Serializable]
  public class Pair : Structure
  {
    [Field]
    public int One { get; set; }

    [Field]
    public int Two { get; set; }

    public Pair(int one, int two)
    {
      One = one;
      Two = two;
    }

    public Pair()
    {
    }
  }

  [Serializable]
  public class Triple : Pair
  {
    [Field]
    public int Three { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0262_StructureAssignment : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Container).Assembly, typeof (Container).Namespace);
      return config;
    }

    [Test]
    public void SetTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          
          var container = new Container();
          try {
            container.Value = new Triple();
            Assert.Fail();
          } catch (InvalidOperationException) {
            
          }
          
          // Rollback
        }
      }
    }

    [Test]
    public void CastTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          
          var container = new Container();
          try {
            container.Value = (Pair)new Triple();
            Assert.Fail();
          } catch (InvalidOperationException) {
            
          }
          
          // Rollback
        }
      }
    }

    [Test]
    public void ValidTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          
          var container = new Container();
          var triple = new Triple();
          container.Value = new Pair(triple.One, triple.Two);

          // Rollback
        }
      }
    }
  }
}