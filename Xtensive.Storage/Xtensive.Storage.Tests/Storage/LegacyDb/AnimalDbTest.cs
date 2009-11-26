// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.11.26

using System;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Tests.Storage.LegacyDb.AnimalDbTestModel;

namespace Xtensive.Storage.Tests.Storage.LegacyDb.AnimalDbTestModel
{
  [HierarchyRoot(InheritanceSchema = InheritanceSchema.SingleTable)]
  [TypeDiscriminatorValue(null, Default = true)]
  public class Animal : Entity
  {
    [Field, Key]
    public Guid Id { get; private set; }

    [Field(Length = 50, TypeDiscriminator = true)]
    [FieldMapping("Type")]
    public string ElementType { get; private set; }

    [Field(Length = 50)]
    public string Name { get; set; }

    [Field]
    public int Age { get; set; }

    [Field]
    [FieldMapping("Owner")]
    public Person Owner { get; set; }
  }

  [TypeDiscriminatorValue("Dog")]
  public class Dog : Animal
  {
  }

  [TypeDiscriminatorValue("Cat")]
  public class Cat : Animal
  {
  }

  [HierarchyRoot(InheritanceSchema = InheritanceSchema.ConcreteTable)]
  public class Person : Entity
  {
    [Field, Key]
    public Guid Id { get; private set;}

    [Field(Length = 50)]
    public string Name { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Storage.LegacyDb
{
  public class AnimalDbTest : AutoBuildTest
  {
    protected override void CheckRequirements()
    {
      base.CheckRequirements();
      EnsureProtocolIs(StorageProtocol.SqlServer);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.ConnectionInfo = UrlInfo.Parse("sqlserver://appserver/Animals");
      config.UpgradeMode = DomainUpgradeMode.Legacy;
      config.Types.Register(typeof (Animal).Assembly, typeof (Animal).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          


          // Rollback
        }
      }
    }
  }
}