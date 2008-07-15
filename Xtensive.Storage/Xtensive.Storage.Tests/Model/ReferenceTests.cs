// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.06.16

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
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

  public class Parent : Structure
  {
    [Field]
    public Child Value { get; set; }
  }

  public class Child : Parent
  {
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
      Domain d = null;
      try {
        d = Domain.Build(config);
      }
      catch (AggregateException e) {
          Assert.AreEqual(4, e.Exceptions.Count());
      }
      d.Model.Dump();
    }
  }
}