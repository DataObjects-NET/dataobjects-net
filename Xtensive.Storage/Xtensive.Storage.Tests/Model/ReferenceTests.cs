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

namespace Xtensive.Storage.Tests.Model.ReferenceTestsModel
{
  [HierarchyRoot(typeof (Int32Provider), "Id")]
  public class A : Entity
  {
    [Field]
    public int Id { get; set; }

    [Field]
    public A Parent { get; set; }
  }

  public class S : Structure
  {
    [Field]
    public S Value { get; set; }
  }

  public class S1 : Structure
  {
    [Field]
    public S2 Value { get; set; }
  }

  public class S2 : Structure
  {
    [Field]
    public S3 Value { get; set; }
  }

  public class S3 : Structure
  {
    [Field]
    public S1 Value { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Model
{
  [TestFixture]
  public class ReferenceTests
  {
    [Test]
    public void MainTest()
    {
      DomainConfiguration config = new DomainConfiguration("memory://localhost/Bugs");
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Model.ReferenceTestsModel");
      Domain d = Domain.Build(config);
      d.Model.Dump();
    }
  }
}