// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.06.22

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.ReferentialIntegrity.ClientProfile.NonPairedReferencesFromEntitiesModel;

namespace Xtensive.Orm.Tests.Storage.ReferentialIntegrity.ClientProfile.NonPairedReferencesFromEntitiesModel
{
  [HierarchyRoot]
  public class EntityWithOneReference : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public ReferencedEntity EntityField { get; set; }
  }

  [HierarchyRoot]
  public class ReferencedEntity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Text { get; set; }
  }

  [HierarchyRoot]
  public class EntityWithManyReferences : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public EntitySet<ReferencedEntity> Entities { get; set; } 
  }
}

namespace Xtensive.Orm.Tests.Storage.ReferentialIntegrity.ClientProfile
{
  public class NonPairedReferencesFromEntitiesTest : AutoBuildTest
  {
    [Test]
    public void InitializeReferenceTest()
    {
      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile | SessionOptions.AutoActivation))) {
        var referencedEntity = new ReferencedEntity();
        var referencingEntity = new EntityWithOneReference();
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        var references = ReferenceFinder.GetReferencesTo(referencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(0));

        referencingEntity.EntityField = referencedEntity;
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(1));
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        references = ReferenceFinder.GetReferencesTo(referencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(1));
        var expectedAssociation = referencingEntity.TypeInfo.Fields["EntityField"].Associations.First();
        Assert.That(expectedAssociation.IsPaired, Is.False);
        Assert.That(expectedAssociation.TargetType, Is.EqualTo(referencedEntity.TypeInfo));
        Assert.That(expectedAssociation.OwnerType, Is.EqualTo(referencingEntity.TypeInfo));
        Assert.That(references.Any(r => r.Association==expectedAssociation && r.ReferencedEntity==referencedEntity && r.ReferencingEntity==referencingEntity), Is.True);

        var anotherReferencedEntity = new ReferencedEntity();
        Assert.That(ReferenceFinder.GetReferencesTo(anotherReferencedEntity).Count(), Is.EqualTo(0));

        referencingEntity.EntityField = anotherReferencedEntity;
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(1));
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        references = ReferenceFinder.GetReferencesTo(referencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(0));
        references = ReferenceFinder.GetReferencesTo(anotherReferencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(1));
        Assert.That(references.Any(r => r.Association==expectedAssociation && r.ReferencedEntity==anotherReferencedEntity && r.ReferencingEntity==referencingEntity), Is.True);

        referencingEntity.EntityField = null;
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        references = ReferenceFinder.GetReferencesTo(referencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(0));
        references = ReferenceFinder.GetReferencesTo(anotherReferencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(0));
      }
    }

    [Test]
    public void ChangeStoredReferenceTest()
    {
      Key referencingEntityKey;
      Key anotherReferencedEntityKey;

      using (var populateSession = Domain.OpenSession())
      using (var transaction = populateSession.OpenTransaction()) {
        var referencedEntity = new ReferencedEntity();
        var anotherReferencedEntity = new ReferencedEntity();
        var referencingEntity = new EntityWithOneReference();
        referencingEntity.EntityField = referencedEntity;
        anotherReferencedEntityKey = anotherReferencedEntity.Key;
        referencingEntityKey = referencingEntity.Key;
        transaction.Complete();
      }

      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile | SessionOptions.AutoActivation))) {
        var referencingEntity = session.Query.Single<EntityWithOneReference>(referencingEntityKey);
        var referencedEntity = referencingEntity.EntityField;
        var anotherReferencedEntity = session.Query.Single<ReferencedEntity>(anotherReferencedEntityKey);
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        var references = ReferenceFinder.GetReferencesTo(anotherReferencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(0));
        references = ReferenceFinder.GetReferencesTo(referencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(1));
        var expectedAssociation = referencingEntity.TypeInfo.Fields["EntityField"].Associations.First();
        Assert.That(references.Any(r => r.Association==expectedAssociation && r.ReferencedEntity==referencedEntity && r.ReferencingEntity==referencingEntity), Is.True);

        referencingEntity.EntityField = anotherReferencedEntity;
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(1));
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(1));
        references = ReferenceFinder.GetReferencesTo(anotherReferencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(1));
        Assert.That(references.Any(r => r.Association==expectedAssociation && r.ReferencedEntity==anotherReferencedEntity && r.ReferencingEntity==referencingEntity), Is.True);
        references = ReferenceFinder.GetReferencesTo(referencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(0));

        referencingEntity.EntityField = null;
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(1));
        references = ReferenceFinder.GetReferencesTo(anotherReferencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(0));
        references = ReferenceFinder.GetReferencesTo(referencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(0));
      }
    }

    [Test]
    public void ChangeReferenceToSameObjectTest()
    {
      Key referencingEntityKey;
      Key anotherReferencedEntityKey;

      using (var populateSession = Domain.OpenSession())
      using (var transaction = populateSession.OpenTransaction()) {
        var referencedEntity = new ReferencedEntity();
        var anotherReferencedEntity = new ReferencedEntity();
        var referencingEntity = new EntityWithOneReference();
        referencingEntity.EntityField = referencedEntity;
        anotherReferencedEntityKey = anotherReferencedEntity.Key;
        referencingEntityKey = referencingEntity.Key;
        transaction.Complete();
      }

      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile | SessionOptions.AutoActivation))) {
        var referencingEntity = session.Query.Single<EntityWithOneReference>(referencingEntityKey);
        var referencedEntity = referencingEntity.EntityField;
        var anotherReferencedEntity = session.Query.Single<ReferencedEntity>(anotherReferencedEntityKey);
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        var references = ReferenceFinder.GetReferencesTo(anotherReferencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(0));
        references = ReferenceFinder.GetReferencesTo(referencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(1));
        var expectedAssociation = referencingEntity.TypeInfo.Fields["EntityField"].Associations.First();
        Assert.That(references.Any(r => r.Association==expectedAssociation && r.ReferencedEntity==referencedEntity && r.ReferencingEntity==referencingEntity), Is.True);

        referencingEntity.EntityField = referencedEntity;
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        references = ReferenceFinder.GetReferencesTo(anotherReferencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(0));
        references = ReferenceFinder.GetReferencesTo(referencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(1));
        Assert.That(references.Any(r => r.Association==expectedAssociation && r.ReferencedEntity==referencedEntity && r.ReferencingEntity==referencingEntity), Is.True);

        referencingEntity.EntityField = null;
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(1));
        references = ReferenceFinder.GetReferencesTo(anotherReferencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(0));
        references = ReferenceFinder.GetReferencesTo(referencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(0));

        referencingEntity.EntityField = referencedEntity;
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        references = ReferenceFinder.GetReferencesTo(anotherReferencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(0));
        references = ReferenceFinder.GetReferencesTo(referencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(1));
        Assert.That(references.Any(r => r.Association==expectedAssociation && r.ReferencedEntity==referencedEntity && r.ReferencingEntity==referencingEntity), Is.True);
      }
    }

    [Test]
    public void AddEntityToEntitySetTest()
    {
      Key firstReferencingKey;
      Key secondReferencingKey;
      var firstList = new List<Key>();
      var secondList = new List<Key>();

      using (var populateSession = Domain.OpenSession())
      using (var transaction = populateSession.OpenTransaction()) {
        var referencedEntities = new List<ReferencedEntity>();
        for (int i = 0; i < 10; i++) {
          referencedEntities.Add(new ReferencedEntity());
        }

        var referencingEntity = new EntityWithManyReferences();
        firstReferencingKey = referencingEntity.Key;
        foreach (var referencedEntity in referencedEntities.Take(5)) {
          referencingEntity.Entities.Add(referencedEntity);
          firstList.Add(referencedEntity.Key);
        }

        referencingEntity = new EntityWithManyReferences();
        secondReferencingKey = referencingEntity.Key;
        foreach (var referencedEntity in referencedEntities.Skip(5)) {
          referencingEntity.Entities.Add(referencedEntity);
          secondList.Add(referencedEntity.Key);
        }
        transaction.Complete();
      }

      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile | SessionOptions.AutoActivation))) {
        var firstReferencingEntity = session.Query.Single<EntityWithManyReferences>(firstReferencingKey);
        var secondReferencingEntity = session.Query.Single<EntityWithManyReferences>(secondReferencingKey);

        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0)); 

        var expectedAssociation = firstReferencingEntity.TypeInfo.Fields["Entities"].Associations.First();
        Assert.That(expectedAssociation.IsPaired, Is.False);
        Assert.That(expectedAssociation.TargetType, Is.EqualTo(Domain.Model.Types[typeof (ReferencedEntity)]));

        List<ReferenceInfo> references;
        foreach (var key in firstList) {
          var referencedEntity = session.Query.Single<ReferencedEntity>(key);
          references = ReferenceFinder.GetReferencesTo(referencedEntity).ToList();
          Assert.That(references.Count, Is.EqualTo(1));
          Assert.That(references.Any(r=>r.Association==expectedAssociation && r.ReferencedEntity==referencedEntity && r.ReferencingEntity==firstReferencingEntity), Is.True);
        }

        foreach (var key in secondList) {
          var referencedEntity = session.Query.Single<ReferencedEntity>(key);
          references = ReferenceFinder.GetReferencesTo(referencedEntity).ToList();
          Assert.That(references.Count, Is.EqualTo(1));
          Assert.That(references.Any(r => r.Association==expectedAssociation && r.ReferencedEntity==referencedEntity && r.ReferencingEntity==secondReferencingEntity), Is.True);
        }

        var newReferencedEntity1 = new ReferencedEntity();
        var newReferencedEntity2 = new ReferencedEntity();
        var newReferencedEntity3 = new ReferencedEntity();
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(newReferencedEntity1).Count(), Is.EqualTo(0));
        firstReferencingEntity.Entities.Add(newReferencedEntity1);
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(1));
        references = ReferenceFinder.GetReferencesTo(newReferencedEntity1).ToList();
        Assert.That(references.Count, Is.EqualTo(1));
        Assert.That(references.Any(r=>r.Association==expectedAssociation && r.ReferencedEntity==newReferencedEntity1 && r.ReferencingEntity==firstReferencingEntity), Is.True);

        secondReferencingEntity.Entities.Add(newReferencedEntity2);
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(2));
        references = ReferenceFinder.GetReferencesTo(newReferencedEntity2).ToList();
        Assert.That(references.Count, Is.EqualTo(1));
        Assert.That(references.Any(r => r.Association==expectedAssociation && r.ReferencedEntity==newReferencedEntity2 && r.ReferencingEntity==secondReferencingEntity), Is.True);

        firstReferencingEntity.Entities.Add(newReferencedEntity3);
        secondReferencingEntity.Entities.Add(newReferencedEntity3);
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(4));
        references = ReferenceFinder.GetReferencesTo(newReferencedEntity3).ToList();
        Assert.That(references.Count, Is.EqualTo(2));
        Assert.That(references.Any(r => r.Association==expectedAssociation && r.ReferencedEntity==newReferencedEntity3 && r.ReferencingEntity==firstReferencingEntity), Is.True);
        Assert.That(references.Any(r => r.Association==expectedAssociation && r.ReferencedEntity==newReferencedEntity3 && r.ReferencingEntity==secondReferencingEntity), Is.True);

        var entityFromFirstList = session.Query.Single<ReferencedEntity>(firstList[0]);
        references = ReferenceFinder.GetReferencesTo(entityFromFirstList).ToList();
        Assert.That(references.Count, Is.EqualTo(1));
        Assert.That(references.Any(r => r.Association==expectedAssociation && r.ReferencedEntity==entityFromFirstList && r.ReferencingEntity==firstReferencingEntity), Is.True);
        secondReferencingEntity.Entities.Add(entityFromFirstList);
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(5));
        references = ReferenceFinder.GetReferencesTo(entityFromFirstList).ToList();
        Assert.That(references.Count, Is.EqualTo(2));
        Assert.That(references.Any(r => r.Association==expectedAssociation && r.ReferencedEntity==entityFromFirstList && r.ReferencingEntity==firstReferencingEntity), Is.True);
        Assert.That(references.Any(r => r.Association==expectedAssociation && r.ReferencedEntity==entityFromFirstList && r.ReferencingEntity==secondReferencingEntity), Is.True);
      }
    }

    [Test]
    public void RemoveEntityFromEntitySetTest()
    {
      Key firstReferencingEntityKey, secondReferencingEntityKey;
      Key firstReferencedEntityKey, secondReferencedEntityKey;
      Key sharedReferencedEntityKey;

      using (var populateSession = Domain.OpenSession())
      using (var transaction = populateSession.OpenTransaction()) {
        var firstReferencingEntity = new EntityWithManyReferences();
        firstReferencingEntityKey = firstReferencingEntity.Key;

        var secondReferencingEntity = new EntityWithManyReferences();
        secondReferencingEntityKey = secondReferencingEntity.Key;

        var firstReferencedEntity = new ReferencedEntity();
        firstReferencedEntityKey = firstReferencedEntity.Key;
        firstReferencingEntity.Entities.Add(firstReferencedEntity);

        var secondReferencedEntity = new ReferencedEntity();
        secondReferencedEntityKey = secondReferencedEntity.Key;
        secondReferencingEntity.Entities.Add(secondReferencedEntity);

        var sharedReferencedEntity = new ReferencedEntity();
        sharedReferencedEntityKey = sharedReferencedEntity.Key;
        firstReferencingEntity.Entities.Add(sharedReferencedEntity);
        secondReferencingEntity.Entities.Add(sharedReferencedEntity);

        transaction.Complete();
      }

      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile | SessionOptions.AutoActivation))) {
        var firstReferencingEntity = session.Query.Single<EntityWithManyReferences>(firstReferencingEntityKey);
        var firstReferencedEntity = session.Query.Single<ReferencedEntity>(firstReferencedEntityKey);
        var secondReferencingEntity = session.Query.Single<EntityWithManyReferences>(secondReferencingEntityKey);
        var secondReferencedEntity = session.Query.Single<ReferencedEntity>(secondReferencedEntityKey);
        var sharedReferencedEntity = session.Query.Single<ReferencedEntity>(sharedReferencedEntityKey);

        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        var expectedAssociation = firstReferencingEntity.TypeInfo.Fields["Entities"].Associations.First();
        Assert.That(expectedAssociation.IsPaired, Is.False);
        Assert.That(expectedAssociation.TargetType, Is.EqualTo(firstReferencedEntity.TypeInfo));

        var references = ReferenceFinder.GetReferencesTo(firstReferencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(1));
        Assert.That(references.Any(r=>r.Association==expectedAssociation && r.ReferencedEntity==firstReferencedEntity && r.ReferencingEntity==firstReferencingEntity), Is.True);

        references = ReferenceFinder.GetReferencesTo(secondReferencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(1));
        Assert.That(references.Any(r=> r.Association==expectedAssociation && r.ReferencedEntity==secondReferencedEntity && r.ReferencingEntity==secondReferencingEntity), Is.True);

        references = ReferenceFinder.GetReferencesTo(sharedReferencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(2));
        Assert.That(references.Any(r => r.Association==expectedAssociation && r.ReferencedEntity==sharedReferencedEntity && r.ReferencingEntity==firstReferencingEntity), Is.True);
        Assert.That(references.Any(r => r.Association==expectedAssociation && r.ReferencedEntity==sharedReferencedEntity && r.ReferencingEntity==secondReferencingEntity), Is.True);

        firstReferencingEntity.Entities.Remove(firstReferencedEntity);
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(1));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        references = ReferenceFinder.GetReferencesTo(firstReferencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(0));

        secondReferencingEntity.Entities.Remove(secondReferencedEntity);
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(2));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        references = ReferenceFinder.GetReferencesTo(secondReferencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(0));

        firstReferencingEntity.Entities.Remove(sharedReferencedEntity);
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(3));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        references = ReferenceFinder.GetReferencesTo(sharedReferencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(1));
        Assert.That(references.Any(r => r.Association==expectedAssociation && r.ReferencedEntity==sharedReferencedEntity && r.ReferencingEntity==firstReferencingEntity), Is.False);
        Assert.That(references.Any(r => r.Association==expectedAssociation && r.ReferencedEntity==sharedReferencedEntity && r.ReferencingEntity==secondReferencingEntity), Is.True);

        secondReferencingEntity.Entities.Remove(sharedReferencedEntity);
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(4));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        references = ReferenceFinder.GetReferencesTo(sharedReferencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(0));
      }
    }

    [Test]
    public void ClearEntitySetTest()
    {
      Key firstReferencingEntityKey, secondReferencingEntityKey;
      Key firstReferencedEntityKey, secondReferencedEntityKey;
      Key sharedReferencedEntityKey;

      using (var populateSession = Domain.OpenSession())
      using (var transaction = populateSession.OpenTransaction()) {
        var firstReferencingEntity = new EntityWithManyReferences();
        firstReferencingEntityKey = firstReferencingEntity.Key;

        var secondReferencingEntity = new EntityWithManyReferences();
        secondReferencingEntityKey = secondReferencingEntity.Key;

        var firstReferencedEntity = new ReferencedEntity();
        firstReferencedEntityKey = firstReferencedEntity.Key;
        firstReferencingEntity.Entities.Add(firstReferencedEntity);

        var secondReferencedEntity = new ReferencedEntity();
        secondReferencedEntityKey = secondReferencedEntity.Key;
        secondReferencingEntity.Entities.Add(secondReferencedEntity);

        var sharedReferencedEntity = new ReferencedEntity();
        sharedReferencedEntityKey = sharedReferencedEntity.Key;
        firstReferencingEntity.Entities.Add(sharedReferencedEntity);
        secondReferencingEntity.Entities.Add(sharedReferencedEntity);

        transaction.Complete();
      }

      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile | SessionOptions.AutoActivation))) {
        var firstReferencingEntity = session.Query.Single<EntityWithManyReferences>(firstReferencingEntityKey);
        var firstReferencedEntity = session.Query.Single<ReferencedEntity>(firstReferencedEntityKey);
        var secondReferencingEntity = session.Query.Single<EntityWithManyReferences>(secondReferencingEntityKey);
        var secondReferencedEntity = session.Query.Single<ReferencedEntity>(secondReferencedEntityKey);
        var sharedReferencedEntity = session.Query.Single<ReferencedEntity>(sharedReferencedEntityKey);

        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        var expectedAssociation = firstReferencingEntity.TypeInfo.Fields["Entities"].Associations.First();
        Assert.That(expectedAssociation.IsPaired, Is.False);
        Assert.That(expectedAssociation.TargetType, Is.EqualTo(firstReferencedEntity.TypeInfo));

        var references = ReferenceFinder.GetReferencesTo(firstReferencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(1));
        Assert.That(references.Any(r => r.Association==expectedAssociation && r.ReferencedEntity==firstReferencedEntity && r.ReferencingEntity==firstReferencingEntity), Is.True);

        references = ReferenceFinder.GetReferencesTo(secondReferencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(1));
        Assert.That(references.Any(r => r.Association==expectedAssociation && r.ReferencedEntity==secondReferencedEntity && r.ReferencingEntity==secondReferencingEntity), Is.True);

        references = ReferenceFinder.GetReferencesTo(sharedReferencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(2));
        Assert.That(references.Any(r => r.Association==expectedAssociation && r.ReferencedEntity==sharedReferencedEntity && r.ReferencingEntity==firstReferencingEntity), Is.True);
        Assert.That(references.Any(r => r.Association==expectedAssociation && r.ReferencedEntity==sharedReferencedEntity && r.ReferencingEntity==secondReferencingEntity), Is.True);

        firstReferencingEntity.Entities.Clear();
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(2));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        references = ReferenceFinder.GetReferencesTo(firstReferencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(0));

        references = ReferenceFinder.GetReferencesTo(secondReferencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(1));
        Assert.That(references.Any(r => r.Association==expectedAssociation && r.ReferencedEntity==secondReferencedEntity && r.ReferencingEntity==secondReferencingEntity), Is.True);

        references = ReferenceFinder.GetReferencesTo(sharedReferencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(1));
        Assert.That(references.Any(r => r.Association==expectedAssociation && r.ReferencedEntity==sharedReferencedEntity && r.ReferencingEntity==firstReferencingEntity), Is.False);
        Assert.That(references.Any(r => r.Association==expectedAssociation && r.ReferencedEntity==sharedReferencedEntity && r.ReferencingEntity==secondReferencingEntity), Is.True);

        secondReferencingEntity.Entities.Clear();
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(4));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        references = ReferenceFinder.GetReferencesTo(firstReferencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(0));

        references = ReferenceFinder.GetReferencesTo(secondReferencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(0));

        references = ReferenceFinder.GetReferencesTo(sharedReferencedEntity).ToList();
        Assert.That(references.Count, Is.EqualTo(0));
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (EntityWithManyReferences).Assembly, typeof (EntityWithManyReferences).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }
  }
}
