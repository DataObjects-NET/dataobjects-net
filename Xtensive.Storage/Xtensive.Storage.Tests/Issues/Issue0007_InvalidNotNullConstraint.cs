// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.25

using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Tests.Issues.Issue0007_Model;

namespace Xtensive.Storage.Tests.Issues.Issue0007_Model
{
  [HierarchyRoot]
  public class Person : Entity
  {
    [Field, KeyField]
    public int ID { get; private set; }

    [Field]
    public Address Address { get; set; }

    [Field]
    public City City { get; set; }
  }

  public class Address : Structure
  {
    [Field]
    public string Street { get; set; }

    [Field]
    public int House { get; set; }
  }

  [HierarchyRoot]
  public class City : Entity
  {
    [Field, KeyField]
    public int ID { get; private set; }

    public string Name { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  public class Issue0007_InvalidNotNullConstraint : AutoBuildTest
  {
    protected override Xtensive.Storage.Configuration.DomainConfiguration BuildConfiguration()
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