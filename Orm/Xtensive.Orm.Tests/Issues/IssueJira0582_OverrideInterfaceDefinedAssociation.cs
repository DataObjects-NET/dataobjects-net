// Copyright (C) 2015 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2015.04.30

using System.Security.Cryptography.X509Certificates;
using NUnit.Framework;
using V1 = Xtensive.Orm.Tests.Issues.IssueJira0582_OverrideInterfaceDefinedAssociationModelV1;
using V2 = Xtensive.Orm.Tests.Issues.IssueJira0582_OverrideInterfaceDefinedAssociationModelV2;
using V3 = Xtensive.Orm.Tests.Issues.IssueJira0582_OverrideInterfaceDefinedAssociationModelV3;
using V4 = Xtensive.Orm.Tests.Issues.IssueJira0582_OverrideInterfaceDefinedAssociationModelV4;

namespace Xtensive.Orm.Tests.Issues.IssueJira0582_OverrideInterfaceDefinedAssociationModelV1
{
  public interface IHasRegimeReference : IEntity
  {
    [Field, Key]
    int Id { get; }

    [Field]
    [Association(OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Default)]
    Regime ZeroToOne { get; set; }

    [Field]
    [Association(OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Default)]
    EntitySet<Regime> ZeroToMany { get; set; }

    [Field]
    [Association(PairTo = "HasRegimeReferenseObject1", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Default)]
    Regime OneToOne { get; set; }

    [Field]
    [Association(PairTo = "HasRegimeReferenseObject2")]
    Regime OnlyPairDeclared { get; set; }

    [Field]
    [Association(PairTo = "HasRegimeReferenseObjects1", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Default)]
    Regime OneToMany { get; set; }

    [Field]
    [Association(PairTo = "HasRegimeReferenseObject2", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Default)]
    EntitySet<Regime> ManyToOne { get; set; }

    [Field]
    [Association(PairTo = "HasRegimeReferenseObjects1", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Default)]
    EntitySet<Regime> ManyToMany { get; set; }
  }

  [HierarchyRoot]
  public class Regime : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public string Description { get; set; }

    [Field]
    public IHasRegimeReference OneToOne { get; set; }

    [Field]
    public EntitySet<IHasRegimeReference> ManyToOne { get; set; }

    [Field]
    public IHasRegimeReference HasRegimeReferenseObject1 { get; set; }

    [Field]
    public IHasRegimeReference HasRegimeReferenseObject2 { get; set; }

    [Field]
    public IHasRegimeReference HasRegimeReferenseObject3 { get; set; }

    [Field]
    public IHasRegimeReference HasRegimeReferenseObject4 { get; set; }

    [Field]
    public EntitySet<IHasRegimeReference> HasRegimeReferenseObjects1 { get; set; }

    [Field]
    public EntitySet<IHasRegimeReference> HasRegimeReferenseObjects2 { get; set; }
  }

  [HierarchyRoot]
  public class Country : Entity, IHasRegimeReference
  {
    public int Id { get; set; }

    public Regime ZeroToOne { get; set; }

    public EntitySet<Regime> ZeroToMany { get; set; }

    public Regime OneToOne { get; set; }

    [Association(PairTo = "HasRegimeReferenseObject4")]
    public Regime OnlyPairDeclared { get; set; }

    public Regime OneToMany { get; set; }

    public EntitySet<Regime> ManyToOne { get; set; }

    public EntitySet<Regime> ManyToMany { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0582_OverrideInterfaceDefinedAssociationModelV2
{
  public interface IHasRegimeReference : IEntity
  {
    [Field, Key]
    int Id { get; }

    [Field]
    [Association(OnOwnerRemove = OnRemoveAction.None, OnTargetRemove = OnRemoveAction.None)]
    Regime Regime { get; }
  }

  [HierarchyRoot]
  public class Regime : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public string Description { get; set; }
  }

  [HierarchyRoot]
  public class Phone : Entity, IHasRegimeReference
  {
    public int Id { get; set; }

    [Field]
    public Regime Regime { get; set; }
  }

  [HierarchyRoot]
  public class Country : Entity, IHasRegimeReference
  {
    public int Id { get; set; }

    [Association(OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Default)]
    public Regime Regime { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0582_OverrideInterfaceDefinedAssociationModelV3
{
  public interface IHasRegimeReference : IEntity
  {
    [Field, Key]
    int Id { get; }

    [Field]
    Regime Regime { get; }
  }

  [HierarchyRoot]
  public class Regime : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public string Description { get; set; }
  }

  [HierarchyRoot]
  public class Country : Entity, IHasRegimeReference
  {
    public int Id { get; set; }

    [Association(OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Default)]
    public Regime Regime { get; set; }
  }

  [HierarchyRoot]
  public class Phone : Entity, IHasRegimeReference
  {
    public int Id { get; set; }

    [Field]
    [Association(OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Default)]
    public Regime Regime { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0582_OverrideInterfaceDefinedAssociationModelV4
{
  public interface IHasRegimeReference : IEntity
  {
    [Field, Key]
    int Id { get; }

    [Field]
    Regime Regime { get; }
  }

  [HierarchyRoot]
  public class Regime : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public string Description { get; set; }
  }

  [HierarchyRoot]
  public class Country : Entity, IHasRegimeReference
  {
    public int Id { get; set; }

    [Association(OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Default)]
    public Regime Regime { get; set; }
  }

  [HierarchyRoot]
  public class Phone : Entity, IHasRegimeReference
  {
    public int Id { get; set; }

    [Field]
    [Association(OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.None)]
    public Regime Regime { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0582_OverrideInterfaceDefinedAssociationModelV5
{
  public abstract class BaseEntity : Entity
  {
    [Key, Field]
    public int Id { get; set; }
  }

  [HierarchyRoot]
  public abstract class Shield : BaseEntity
  {
    [Field]
    [Association(OnOwnerRemove = OnRemoveAction.None, OnTargetRemove = OnRemoveAction.None)]
    public abstract ShieldBehavior Behavior { get; set; }
  }

  [HierarchyRoot]
  public abstract class ShieldBehavior : BaseEntity
  {
    public abstract int ProtectionValue { get; set; }
  }

  public class WoodenShield : Shield
  {
    [Field]
    [Association(OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
    public override ShieldBehavior Behavior { get; set; }
  }

  public class WoodenShieldBehavior : ShieldBehavior
  {
    [Field]
    public override int ProtectionValue { get; set; }
  }
}


namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0582_OverrideInterfaceDefinedAssociation : AutoBuildTest
  {
    [Test]
    [Ignore("Bug hasn't been fixed yet")]
    public void AssociationDefinedInInterface()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof (V1.Regime).Assembly, typeof (V1.Regime).Namespace);
      using (var domain = BuildDomain(configuration)) {
        Assert.AreEqual(1, domain.Model.Associations.Count);
        var interfaceAssociation = domain.Model.Types[typeof (V1.IHasRegimeReference)].Fields["ZeroToOne"].Associations[0];
        Assert.That(interfaceAssociation.OnOwnerRemove,Is.EqualTo(OnRemoveAction.Cascade));
        Assert.That(interfaceAssociation.OnTargetRemove, Is.EqualTo(OnRemoveAction.Default));
        var regimeField = domain.Model.Types[typeof (V1.Country)].Fields["ZeroToOne"];
        Assert.AreEqual(1, regimeField.Associations.Count);
        var association = regimeField.Associations[0];
        Assert.That(interfaceAssociation.OnOwnerRemove, Is.EqualTo(OnRemoveAction.Cascade));
        Assert.That(interfaceAssociation.OnTargetRemove, Is.EqualTo(OnRemoveAction.Default));
      }
    }

    [Test]
    [Ignore("Bug hasn't been fixed yet")]
    public void AssociationDefinedInInterfaceAndOverridenByImplementor()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof (V2.Regime).Assembly, typeof (V2.Regime).Namespace);
      using (var domain = BuildDomain(configuration)) {
        Assert.AreEqual(2, domain.Model.Associations.Count);
        var interfaceAssociation = domain.Model.Types[typeof(V2.IHasRegimeReference)].Fields["ZeroToOne"].Associations[0];
        Assert.That(interfaceAssociation.OnOwnerRemove, Is.EqualTo(OnRemoveAction.None));
        Assert.That(interfaceAssociation.OnTargetRemove, Is.EqualTo(OnRemoveAction.None));
        var countryRegimeField = domain.Model.Types[typeof(V2.Country)].Fields["ZeroToOne"];
        Assert.AreEqual(1, countryRegimeField.Associations.Count);
        var overriddenAssociation = countryRegimeField.Associations[0];
        Assert.That(overriddenAssociation.OnOwnerRemove, Is.EqualTo(OnRemoveAction.Cascade));
        Assert.That(overriddenAssociation.OnTargetRemove, Is.EqualTo(OnRemoveAction.Default));

        var phoneRegimeField = domain.Model.Types[typeof(V2.Phone)].Fields["ZeroToOne"];
        Assert.AreEqual(1, phoneRegimeField.Associations.Count);
        var association = phoneRegimeField.Associations[0];
        Assert.That(association.OnOwnerRemove, Is.EqualTo(interfaceAssociation.OnOwnerRemove));
        Assert.That(association.OnTargetRemove, Is.EqualTo(interfaceAssociation.OnTargetRemove));
      }
    }

    [Test]
    [Ignore("Bug hasn't been fixed yet")]
    public void AssociationDefinedByEachImplementorInTheSameWay()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof (V3.Regime).Assembly, typeof (V3.Regime).Namespace);
      using (var domain = BuildDomain(configuration)) {
        Assert.AreEqual(1, domain.Model.Associations.Count);
        var interfaceAssociation = domain.Model.Types[typeof(V1.IHasRegimeReference)].Fields["ZeroToOne"].Associations[0];
        Assert.That(interfaceAssociation.OnOwnerRemove, Is.EqualTo(OnRemoveAction.Cascade));
        Assert.That(interfaceAssociation.OnTargetRemove, Is.EqualTo(OnRemoveAction.Default));
        var regimeField = domain.Model.Types[typeof(V1.Country)].Fields["ZeroToOne"];
        Assert.AreEqual(1, regimeField.Associations.Count);
        var association = regimeField.Associations[0];
        Assert.That(interfaceAssociation.OnOwnerRemove, Is.EqualTo(OnRemoveAction.Cascade));
        Assert.That(interfaceAssociation.OnTargetRemove, Is.EqualTo(OnRemoveAction.Default));
      }
    }

    [Test]
    [Ignore("Bug hasn't been fixed yet")]
    public void AssociationDefinedByEachImplementorInDifferentWay()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(V4.Regime).Assembly, typeof(V4.Regime).Namespace);
      using (var domain = BuildDomain(configuration))
      {

      }
    }

    public override void TestFixtureSetUp()
    {
      CheckRequirements();
    }
  }
}
