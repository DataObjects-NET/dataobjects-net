// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.12.26

using System;
using NUnit.Framework;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Building;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Model.Schemas;

namespace Xtensive.Storage.Tests.Model.Schemas
{
  public interface I0 : IEntity
  {
    [Field]
    string AName { get; set; }
  }

  public interface IA : I0
  {
  }

  public class A : Entity, IA
  {
    [Field]
    string I0.AName { get; set; }

    [Field]
    public string AName { get; set; }
  }

  [HierarchyRoot(typeof (DefaultGenerator), "ID")]
  public class AB : A
  {
    [Field]
    public long ID { get; set; }

    [Field]
    public string ABName { get; set; }
  }

  public class ABC : AB
  {
  }

  public abstract class B : Entity
  {
  }

  [HierarchyRoot(typeof (DefaultGenerator), "ID")]
  public class BC : B
  {
    [Field]
    public Guid ID { get; set; }
  }

  [HierarchyRoot(typeof (DefaultGenerator), "ID")]
  public class BD : B
  {
    [Field]
    public long ID { get; set; }

    [Field]
    public string AName { get; set; }
  }

  [HierarchyRoot(typeof (DefaultGenerator), "ID")]
  public class BE : B
  {
    [Field]
    public int ID { get; set; }
  }

  public class CustomStorageDefinitionBuilder : IDomainBuilder
  {
    public void Build(BuildingContext context, DomainDef domain)
    {
      TypeDef type;

      type = domain.Types["A"];
      Assert.IsFalse(context.Definition.FindRoot(type)==type);

      type = domain.Types["AB"];
      Assert.IsTrue(context.Definition.FindRoot(type)==type);

      type = domain.Types["ABC"];
      Assert.IsFalse(context.Definition.FindRoot(type)==type);

      type = domain.Types["B"];
      Assert.IsFalse(context.Definition.FindRoot(type)==type);

      type = domain.Types["BC"];
      Assert.IsTrue(context.Definition.FindRoot(type)==type);
    }
  }
}

namespace Xtensive.Storage.Tests.Model
{
  [TestFixture]
  public class HierarchyTests
  {
    [Test]
    public void MainTest()
    {
      Domain domain = CreateDomain();
      domain.Model.Dump();
    }

    private Domain CreateDomain()
    {
      DomainConfiguration configuration =
        new DomainConfiguration(@"memory://localhost\sql2005/ABC");

      configuration.Types.Register(typeof (A).Assembly, "Xtensive.Storage.Tests.Model.Schemas");

      configuration.Builders.Add(typeof (CustomStorageDefinitionBuilder));
      Domain domain = Domain.Build(configuration);

      Assert.IsFalse(domain.Model.Types.Contains(typeof (A)));
      Assert.IsNotNull(domain.Model.Types[typeof (AB)]);
      Assert.IsNotNull(domain.Model.Types[typeof (AB)].Fields["ID"]);
      Assert.IsNotNull(domain.Model.Types[typeof (AB)].Fields["ABName"]);
      Assert.IsNotNull(domain.Model.Types[typeof (AB)].Fields["AName"]);
      Assert.AreEqual(domain.Model.Types[typeof (AB)], domain.Model.Types[typeof (AB)].Fields["ABName"].DeclaringType);
      Assert.AreEqual(domain.Model.Types[typeof (AB)], domain.Model.Types[typeof (AB)].Fields["AName"].DeclaringType);
      Assert.AreEqual(domain.Model.Types[typeof (AB)], domain.Model.Types[typeof (AB)].Hierarchy.Root);
      Assert.AreEqual(domain.Model.Types[typeof (AB)], domain.Model.Types[typeof (ABC)].Hierarchy.Root);
      Assert.AreEqual(domain.Model.Types[typeof (AB)].Hierarchy, domain.Model.Types[typeof (ABC)].Hierarchy);
      Assert.AreEqual(typeof (long), domain.Model.Types[typeof (AB)].Fields["ID"].ValueType);
      Assert.AreEqual(typeof (long), domain.Model.Types[typeof (ABC)].Fields["ID"].ValueType);
      Assert.AreEqual(typeof (Guid), domain.Model.Types[typeof (BC)].Fields["ID"].ValueType);
      Assert.AreEqual(typeof (long), domain.Model.Types[typeof (BD)].Fields["ID"].ValueType);
      Assert.AreEqual(typeof (int), domain.Model.Types[typeof (BE)].Fields["ID"].ValueType);

      return domain;
    }
  }
}