// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.06.25

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Model.StringKeyTestModel;

namespace Xtensive.Orm.Tests.Model.StringKeyTestModel
{
  [Serializable]
  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class Product : Entity
  {
    [Key, Field(Length = 100)]
    public string Code { get; private set; }

    [Field(Length = 100)]
    public string Name { get; set; }

    public Product(string code) : base(code) { }
  }
}

namespace Xtensive.Orm.Tests.Model
{
  public class StringKeyTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Product));
      return config;
    }

    [Test]
    public void MainTest()
    {
    }
  }
}