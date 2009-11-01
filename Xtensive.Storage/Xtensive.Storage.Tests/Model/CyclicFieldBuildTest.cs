// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.06.16

using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.KeyProviders;

namespace Xtensive.Storage.Tests.Model.CyclicFieldBuildTestModel
{
  [HierarchyRoot(typeof (Int32Provider), "Id")]
  public class A : Entity
  {
    [Field]
    public int Id { get; set; }

    [Field]
    public A Parent { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Model
{
  [TestFixture]
  public class CyclicFieldBuildTest
  {
    [Test]
    public void MainTest()
    {
      DomainConfiguration config = new DomainConfiguration("memory://localhost/Bugs");
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Model.CyclicFieldBuildTestModel");
      Domain.Build(config);
    }
  }
}