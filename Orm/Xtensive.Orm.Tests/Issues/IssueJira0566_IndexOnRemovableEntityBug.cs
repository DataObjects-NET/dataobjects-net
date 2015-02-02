// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.11.26

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using model1 = Xtensive.Orm.Tests.Issues.IssueJira0566_BuildingUnderlyingIndexForForeignKeyBugModel1;
using model2 = Xtensive.Orm.Tests.Issues.IssueJira0566_BuildingUnderlyingIndexForForeignKeyBugModel2;
using model3 = Xtensive.Orm.Tests.Issues.IssueJira0566_BuildingUnderlyingIndexForForeignKeyBugModel3;

namespace Xtensive.Orm.Tests.Issues.IssueJira0566_BuildingUnderlyingIndexForForeignKeyBugModel1
{
  [Index("Entity", Clustered = true, Name = "Clustered")]
  [Serializable]
  public abstract class BaseEntity : Entity
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; private set; }

    [Field]
    [Association(OnTargetRemove = OnRemoveAction.Cascade, OnOwnerRemove = OnRemoveAction.Clear)]
    public WithEntitySet Entity { get; set; }
  }

  
  [HierarchyRoot(InheritanceSchema.ConcreteTable)]
  public class TestEntity : BaseEntity
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; private set; }

    [Field(Nullable = false, Length = 400)]
    public string Name { get; set; }
  }

  [HierarchyRoot]
  [Serializable]
  public class WithEntitySet : Entity
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; private set; }

    [Field(Nullable = false, Length = 400)]
    public string Name { get; set; }

    [Field]
    [Association(PairTo = "Entity")]
    public EntitySet<TestEntity> List { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0566_BuildingUnderlyingIndexForForeignKeyBugModel2
{
  [Index("Entity", Clustered = true, Name = "Clustered")]
  [Serializable]
  public abstract class BaseEntity : Entity
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; private set; }

    [Field]
    [Association(OnTargetRemove = OnRemoveAction.Cascade, OnOwnerRemove = OnRemoveAction.Clear)]
    public WithEntitySet Entity { get; set; }
  }


  [HierarchyRoot(InheritanceSchema = InheritanceSchema.ClassTable)]
  public class TestEntity : BaseEntity
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; private set; }

    [Field(Nullable = false, Length = 400)]
    public string Name { get; set; }
  }

  public class TestEntityChild : TestEntity
  {
    [Field]
    public string TestEntityChildField { get; set; }
  }

  [HierarchyRoot]
  [Serializable]
  public class WithEntitySet : Entity
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; private set; }

    [Field(Nullable = false, Length = 400)]
    public string Name { get; set; }

    [Field]
    [Association(PairTo = "Entity")]
    public EntitySet<TestEntity> List { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0566_BuildingUnderlyingIndexForForeignKeyBugModel3
{
  [Index("Entity", Clustered = true, Name = "Clustered")]
  [Serializable]
  public abstract class BaseEntity : Entity
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; private set; }

    [Field]
    [Association(OnTargetRemove = OnRemoveAction.Cascade, OnOwnerRemove = OnRemoveAction.Clear)]
    public WithEntitySet Entity { get; set; }
  }


  [HierarchyRoot(InheritanceSchema.SingleTable)]
  public class TestEntity : BaseEntity
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; private set; }

    [Field(Nullable = false, Length = 400)]
    public string Name { get; set; }
  }

  public class TestEntityChild : TestEntity
  {
    [Field]
    public string TestEntityChildField { get; set; }
  }

  [HierarchyRoot]
  [Serializable]
  public class WithEntitySet : Entity
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; private set; }

    [Field(Nullable = false, Length = 400)]
    public string Name { get; set; }

    [Field]
    [Association(PairTo = "Entity")]
    public EntitySet<TestEntity> List { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class IssueJira0566_IndexOnRemovableEntityBug : AutoBuildTest
  {
    [Test]
    public void ConcreteTableTest()
    {
      using (var domain = BuildDomain(BuildConfiguration(typeof (model1.BaseEntity)))) {
        Assert.That(domain.Model.Types.Contains(typeof (model1.BaseEntity)), Is.False);
        Assert.That(domain.Model.Types[typeof(model1.TestEntity)].Indexes.Any(info => info.IsSecondary && info.IsClustered), Is.True);
        Assert.That(domain.Model.Types[typeof (model1.TestEntity)].Fields.Contains("Entity"), Is.True);
        Assert.That(domain.Model.Types[typeof (model1.TestEntity)].Fields["Entity"].Associations.Count, Is.EqualTo(1));
        Assert.That(domain.Model.Types[typeof (model1.TestEntity)].Fields["Entity"].Associations.First().UnderlyingIndex, Is.Not.Null);
        Assert.That(domain.Model.Types[typeof(model1.WithEntitySet)].Fields.Contains("List"), Is.True);
        Assert.That(domain.Model.Types[typeof(model1.WithEntitySet)].Fields["List"].Associations.Count, Is.EqualTo(1));
        Assert.That(domain.Model.Types[typeof(model1.WithEntitySet)].Fields["List"].Associations.First().UnderlyingIndex, Is.Not.Null);
        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var transaction = session.OpenTransaction()) {
          var entity = new model1.WithEntitySet { Name = "Test" };

          for (int i = 0; i < 25; i++) {
            new model1.TestEntity { Entity = entity, Name = "Test" };
          }

          transaction.Complete();
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          model1.TestEntity[] array = null;
          Assert.DoesNotThrow(() => { array = session.Query.All<model1.WithEntitySet>().First().List.ToArray(); });
          Assert.IsNotNull(array);
          Assert.AreEqual(25, array.Length);
        }
      }
    }

    [Test]
    public void ClassTableTest()
    {
      using (var domain = BuildDomain(BuildConfiguration(typeof(model2.BaseEntity)))) {
        Assert.That(domain.Model.Types.Contains(typeof(model2.BaseEntity)), Is.False);
        Assert.That(domain.Model.Types[typeof (model2.TestEntity)].Fields.Contains("Entity"), Is.True);
        Assert.That(domain.Model.Types[typeof (model2.TestEntity)].Indexes.Any(info => info.IsSecondary && info.IsClustered), Is.True);
        Assert.That(domain.Model.Types[typeof(model2.TestEntity)].Fields["Entity"].Associations.Count, Is.EqualTo(1));
        Assert.That(domain.Model.Types[typeof(model2.TestEntity)].Fields["Entity"].Associations.First().UnderlyingIndex, Is.Not.Null);
        Assert.That(domain.Model.Types[typeof(model2.WithEntitySet)].Fields.Contains("List"), Is.True);
        Assert.That(domain.Model.Types[typeof(model2.WithEntitySet)].Fields["List"].Associations.Count, Is.EqualTo(1));
        Assert.That(domain.Model.Types[typeof(model2.WithEntitySet)].Fields["List"].Associations.First().UnderlyingIndex, Is.Not.Null);
        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var transaction = session.OpenTransaction()) {
          var entity = new model2.WithEntitySet { Name = "Test" };

          for (int i = 0; i < 25; i++) {
            new model2.TestEntity { Entity = entity, Name = "Test" };
          }

          transaction.Complete();
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          model2.TestEntity[] array = null;
          Assert.DoesNotThrow(() => { array = session.Query.All<model2.WithEntitySet>().First().List.ToArray(); });
          Assert.IsNotNull(array);
          Assert.AreEqual(25, array.Length);
        }
      }
    }

    [Test]
    public void SingleTableTest()
    {
      using (var domain = BuildDomain(BuildConfiguration(typeof(model3.BaseEntity)))) {
        Assert.That(domain.Model.Types.Contains(typeof(model3.BaseEntity)), Is.False);
        Assert.That(domain.Model.Types[typeof(model3.TestEntity)].Fields.Contains("Entity"), Is.True);
        Assert.That(domain.Model.Types[typeof(model3.TestEntity)].Indexes.Any(info => info.IsSecondary && info.IsClustered), Is.True);
        Assert.That(domain.Model.Types[typeof(model3.TestEntity)].Fields["Entity"].Associations.Count, Is.EqualTo(1));
        Assert.That(domain.Model.Types[typeof(model3.TestEntity)].Fields["Entity"].Associations.First().UnderlyingIndex, Is.Not.Null);
        Assert.That(domain.Model.Types[typeof(model3.WithEntitySet)].Fields.Contains("List"), Is.True);
        Assert.That(domain.Model.Types[typeof(model3.WithEntitySet)].Fields["List"].Associations.Count, Is.EqualTo(1));
        Assert.That(domain.Model.Types[typeof(model3.WithEntitySet)].Fields["List"].Associations.First().UnderlyingIndex, Is.Not.Null);
        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var transaction = session.OpenTransaction()) {
          var entity = new model3.WithEntitySet { Name = "Test" };

          for (int i = 0; i < 25; i++) {
            new model3.TestEntity { Entity = entity, Name = "Test" };
          }

          transaction.Complete();
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          model3.TestEntity[] array = null;
          Assert.DoesNotThrow(() => { array = session.Query.All<model3.WithEntitySet>().First().List.ToArray(); });
          Assert.IsNotNull(array);
          Assert.AreEqual(25, array.Length);
        }
      }
    }

    private DomainConfiguration BuildConfiguration(Type typeFromRegisteringModel)
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeFromRegisteringModel.Assembly, typeFromRegisteringModel.Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }
  }
}
