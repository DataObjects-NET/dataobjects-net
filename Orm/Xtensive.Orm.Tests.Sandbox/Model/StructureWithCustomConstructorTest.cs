// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.05.04

using NUnit.Framework;
using Xtensive.Orm.Tests.Model.StructureWithCustomConstructorTestModel;
using Xtensive.Orm.Validation;

namespace Xtensive.Orm.Tests.Model
{
  namespace StructureWithCustomConstructorTestModel
  {
    [HierarchyRoot]
    public class MyEntity : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public MyStructure MyStructureProperty { get; private set; }

      public MyEntity()
      {
        MyStructureProperty = new MyStructure("This is new");
      }
    }

    public class MyStructure : Structure
    {
      [Field]
      [NotNullOrEmptyConstraint]
      public string Value { get; private set; }

      public void Set(string value)
      {
        Value = value ?? string.Empty;
      }

      public MyStructure(string value)
      {
        Set(value);
      }
    }
  }

  public class StructureWithCustomConstructorTest : AutoBuildTest
  {
    protected override Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (MyEntity));
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new MyEntity();
        tx.Complete();
      }
    }
  }
}