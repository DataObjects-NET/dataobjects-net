// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.01

using NUnit.Framework;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Model.ReferencedKeysModel;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Tests.Model.ReferencedKeysModel
{
  [HierarchyRoot("Name")]
  public class Country : Entity
  {
    [Field]
    public string Name { get; private set;}
      
    [Field]
    public City Capital { get; set; }

    public Country(string name) : base(Tuple.Create(name)) {}
  }

  [HierarchyRoot("Country", "Name")]
  public class City : Entity
  {
    [Field]
    public Country Country { get; private set;}

    [Field]
    public string Name { get; private set;}

    public City(Country country, string name) : base(Tuple.Create(country.Name, name)) {}
  }
}

namespace Xtensive.Storage.Tests.Model
{   
  [TestFixture]
  public class  ReferencedKeys : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Country).Assembly, typeof (Country).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
    }
  }
}