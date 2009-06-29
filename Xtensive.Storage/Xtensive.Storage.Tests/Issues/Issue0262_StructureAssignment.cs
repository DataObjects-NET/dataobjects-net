// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.06.29

using System;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue0262_Model;

namespace Xtensive.Storage.Tests.Issues.Issue0262_Model
{
  [HierarchyRoot]
  public class Container : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public Pair Value { get; set; }
  }

  public class Pair : Structure
  {
    [Field]
    public int One { get; set; }

    [Field]
    public int Two { get; set; }
  }

  public class Triple : Pair
  {
    [Field]
    public int Three { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
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
    public void MainTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          
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
  }
}