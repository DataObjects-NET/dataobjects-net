// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.06.06

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Orm.Upgrade;
using V1 = Xtensive.Orm.Tests.Issues.StructureFieldRemoveModel1;
using V2 = Xtensive.Orm.Tests.Issues.StructureFieldRemoveModel2;

namespace Xtensive.Orm.Tests.Issues
{
  namespace StructureFieldRemoveModel1
  {
    public class MyStructure1 : Structure
    {
      [Field]
      public Guid GlobalId { get; set; }

      [Field]
      public string Name { get; set; }
    }

    public class MyStructure2 : Structure
    {
      [Field]
      public MyStructure1 S1 { get; set; }
    }

    public class MyStructure3 : Structure
    {
      [Field]
      public MyStructure2 S2 { get; set; }
    }

    [HierarchyRoot]
    public class StructureOwner : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public MyStructure1 S1 { get; set; }

      [Field]
      public MyStructure2 S2 { get; set; }

      [Field]
      public MyStructure3 S3 { get; set; }

      public StructureOwner(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class StructureOwnerToRemove : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public MyStructure3 S3 { get; set; }
    }

    public class Upgrader : UpgradeHandler
    {
      protected override string DetectAssemblyVersion()
      {
        return "2";
      }
    }
  }

  namespace StructureFieldRemoveModel2
  {
    public class MyStructure1 : Structure
    {
      [Field]
      public string Name { get; set; }
    }

    public class MyStructure2 : Structure
    {
      [Field]
      public MyStructure1 S1 { get; set; }
    }

    public class MyStructure3 : Structure
    {
      [Field]
      public MyStructure2 S2 { get; set; }
    }

    [HierarchyRoot]
    public class StructureOwner : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public MyStructure1 S1 { get; set; }

      [Field]
      public MyStructure2 S2 { get; set; }

      [Field]
      public MyStructure3 S3 { get; set; }

      public StructureOwner(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class StructureOwnerToAdd : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public MyStructure3 S3 { get; set; }
    }

    public class Upgrader : UpgradeHandler
    {
      protected override string DetectAssemblyVersion()
      {
        return "2";
      }

      protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
      {
        hints.Add(new RenameTypeHint(typeof (V1.StructureOwner).FullName, typeof (StructureOwner)));
        hints.Add(new RenameTypeHint(typeof (V1.MyStructure1).FullName, typeof (MyStructure1)));
        hints.Add(new RenameTypeHint(typeof (V1.MyStructure2).FullName, typeof (MyStructure2)));
        hints.Add(new RenameTypeHint(typeof (V1.MyStructure3).FullName, typeof (MyStructure3)));
        hints.Add(new RemoveTypeHint(typeof (V1.StructureOwnerToRemove).FullName));
        hints.Add(new RemoveFieldHint(typeof (V1.MyStructure1), "GlobalId"));
      }

      public override bool CanUpgradeFrom(string oldVersion)
      {
        return true;
      }
    }
  }

  [TestFixture]
  public class StructureFieldRemoveTest
  {
    public Domain BuildDomain(Type type, DomainUpgradeMode mode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = mode;
      configuration.Types.Register(type.Assembly, type.Namespace);
      return Domain.Build(configuration);
    }

    [Test]
    public void MainTest()
    {
      using (var domain = BuildDomain(typeof (V1.StructureOwner), DomainUpgradeMode.Recreate))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new V1.StructureOwner(session) {
          S1 = {GlobalId = Guid.NewGuid(), Name = "S1"},
          S2 = {S1 = {GlobalId = Guid.NewGuid(), Name = "S2"}},
          S3 = {S2 = {S1 = {GlobalId = Guid.NewGuid(), Name = "S3"}}},
        };
        tx.Complete();
      }

      using (var domain = BuildDomain(typeof (V2.StructureOwner), DomainUpgradeMode.PerformSafely))
      using (var sessoin = domain.OpenSession())
      using (var tx = sessoin.OpenTransaction()) {
        var owner = sessoin.Query.All<V2.StructureOwner>().Single();
        Assert.That(owner.S1.Name, Is.EqualTo("S1"));
        Assert.That(owner.S2.S1.Name, Is.EqualTo("S2"));
        Assert.That(owner.S3.S2.S1.Name, Is.EqualTo("S3"));
        tx.Complete();
      }
    }
  }
}