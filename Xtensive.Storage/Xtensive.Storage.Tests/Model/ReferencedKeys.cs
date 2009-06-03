// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.01

using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Model.ReferencedKeysModel;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Tests.Model.ReferencedKeysModel
{
  [KeyGenerator(null)]
  [HierarchyRoot]
  public class Country : Entity
  {
    [Field, KeyField]
    public string Name { get; private set;}
      
    [Field]
    public City Capital { get; set; }

    public Country(string name) : base(name) {}
  }

  [KeyGenerator(null)]
  [HierarchyRoot]
  public class City : Entity
  {
    [Field, KeyField(0)]
    public Country Country { get; private set;}

    [Field, KeyField(1)]
    public string Name { get; private set;}

    public City(Country country, string name) : base(country.Name, name) {}
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