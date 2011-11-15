// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.11.15

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Sandbox.Storage.AdvancedDefaultAndTypeDiscriminatorTestModel;

namespace Xtensive.Orm.Tests.Sandbox.Storage.AdvancedDefaultAndTypeDiscriminatorTestModel
{
  public static class CodeRegistry
  {
    public const string DefaultCode = "{9F011719-DD4D-4B45-84F8-56B05A47952B}";
  }

  public interface IDiscriminated : IEntity
  {
    [Field, TypeDiscriminator]
    Guid Code { get; }
  }

  [HierarchyRoot, TypeDiscriminatorValue(CodeRegistry.DefaultCode)]
  public class Discriminated : Entity, IDiscriminated
  {
    [Field, Key]
    public int Id { get; private set; }

    public Guid Code { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Sandbox.Storage
{
  public class TypeDiscriminatorFromInterfaceTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Discriminated).Assembly, typeof (Discriminated).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var d = new Discriminated();
          var items = session.Query.All<Discriminated>().ToList();
          Assert.AreEqual(new Guid(CodeRegistry.DefaultCode), d.Code);
        }
      }
    }
  }
}