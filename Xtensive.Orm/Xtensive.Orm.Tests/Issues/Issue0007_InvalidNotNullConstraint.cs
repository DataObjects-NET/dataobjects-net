// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.25

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0007_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0007_Model
{
  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Field, Key]
    public int ID { get; private set; }

    [Field]
    public Address Address { get; set; }

    [Field]
    public City City { get; set; }
  }

  [Serializable]
  public class Address : Structure
  {
    [Field]
    public string Street { get; set; }

    [Field]
    public int House { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class City : Entity
  {
    [Field, Key]
    public int ID { get; private set; }

    public string Name { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0007_InvalidNotNullConstraint : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof(Person).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      Assert.AreEqual(true, Domain.Model.Types[typeof (Person)].Fields["Address.Street"].IsNullable);
      Assert.AreEqual(false, Domain.Model.Types[typeof (Person)].Fields["Address.House"].IsNullable);
      Assert.AreEqual(true, Domain.Model.Types[typeof (Person)].Fields["City"].IsNullable);
      Assert.AreEqual(true, Domain.Model.Types[typeof (Person)].Fields["City.ID"].IsNullable);
    }
  }
}