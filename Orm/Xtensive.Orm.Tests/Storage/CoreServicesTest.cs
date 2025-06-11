// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2008.11.03

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Orm.Services;
using Xtensive.Orm.Tests.Storage.CoreServicesModel;
using FieldInfo=Xtensive.Orm.Model.FieldInfo;

namespace Xtensive.Orm.Tests.Storage.CoreServicesModel
{
  [Serializable]
  [HierarchyRoot]
  public class MyEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Value
    {
      get { return GetFieldValue<string>("Value"); }
      set
      {
        throw new InvalidOperationException();
        // SetFieldValue("Value", value);
      }
    }

    [Field]
    public MyRefEntity Reference
    {
      get { return GetFieldValue<MyRefEntity>("Reference"); }
      set {
        throw new InvalidOperationException();
        // SetFieldValue("Reference", value);
      }
    }

    [Field]
    public MyEntitySet<MyEntity> Items { get; private set; }

    #region Custom business logic

    protected override void OnInitialize()
    {
      base.OnInitialize();
    }

    protected override void OnSettingFieldValue(FieldInfo field, object value)
    {
      base.OnSettingFieldValue(field, value);
      throw new InvalidOperationException();
    }

    protected override void OnSetFieldValue(FieldInfo field, object oldValue, object newValue)
    {
      base.OnSetFieldValue(field, oldValue, newValue);
      throw new InvalidOperationException();
    }

    protected override void OnRemoving()
    {
      base.OnRemoving();
      throw new InvalidOperationException();
    }

    protected override void OnRemove()
    {
      base.OnRemoving();
      throw new InvalidOperationException();
    }

    protected override void OnValidate()
    {
      base.OnValidate();
      throw new InvalidOperationException();
    }

    #endregion

    public MyEntity()
    {
      throw new InvalidOperationException();
    }
  }

  [Serializable]
  public class MyStructure : Structure
  {
    [Field]
    public string Value
    {
      get { return GetFieldValue<string>("Value"); }
      set
      {
        throw new InvalidOperationException();
        // SetFieldValue("Value", value);
      }
    }

    #region Custom business logic

    protected override void OnInitialize()
    {
      base.OnInitialize();
      throw new InvalidOperationException();
    }

    protected override void OnSettingFieldValue(Xtensive.Orm.Model.FieldInfo field, object value)
    {
      base.OnSettingFieldValue(field, value);
      throw new InvalidOperationException();
    }

    protected override void OnSetFieldValue(Xtensive.Orm.Model.FieldInfo field, object oldValue, object newValue)
    {
      base.OnSetFieldValue(field, oldValue, newValue);
      throw new InvalidOperationException();
    }

    protected override void OnValidate()
    {
      base.OnValidate();
      throw new InvalidOperationException();
    }

    #endregion

    public MyStructure()
    {
      throw new InvalidOperationException();
    }
  }

  public class MyEntitySet<T> : EntitySet<T>
    where T : IEntity
  {

    protected override void OnAdding(Entity item)
    {
      throw new InvalidOperationException();
    }

    protected override void OnAdd(Entity item)
    {
      throw new InvalidOperationException();
    }

    protected override void OnRemoving(Entity item)
    {
      throw new InvalidOperationException();
    }

    protected override void OnRemove(Entity item)
    {
      throw new InvalidOperationException();
    }

    protected override void OnClearing()
    {
      throw new InvalidOperationException();
    }

    protected override void OnClear()
    {
      throw new InvalidOperationException();
    }

    protected override void OnInitialize()
    {
      throw new InvalidOperationException();
    }

    public MyEntitySet(Entity owner, FieldInfo field)
      : base(owner, field)
    {
    }
  }

  [HierarchyRoot]
  public class MyRefEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Value { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class CoreServicesTest : AutoBuildTest
  {
    protected override Orm.Configuration.DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.RegisterCaching(typeof(MyEntity).Assembly, typeof(MyEntity).Namespace);
      return config;
    }

    [Test]
    public void CreateInstanceTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var accessor = session.Services.Get<DirectPersistentAccessor>();
        var entity = accessor.CreateEntity(typeof(MyEntity));
        var structure = accessor.CreateStructure(typeof(MyStructure));
        Assert.That(entity.PersistenceState, Is.EqualTo(PersistenceState.New));
        Assert.That(structure.Owner, Is.Null);
        t.Complete();
      }
    }

    [Test]
    public void SetFieldOfNewEntityTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var accessor = session.Services.Get<DirectPersistentAccessor>();
        var myEntity = (MyEntity) accessor.CreateEntity(typeof(MyEntity));
        accessor.SetFieldValue(myEntity, myEntity.TypeInfo.Fields["Value"], "Value");
        Assert.AreEqual("Value", myEntity.Value);
        Assert.That(myEntity.PersistenceState, Is.EqualTo(PersistenceState.New));
        t.Complete();
      }
    }

    [Test]
    public void SetFieldOfExistingEntityTest()
    {
      var entityId = 0;
      var updatedValue = string.Empty;
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var accessor = session.Services.Get<DirectPersistentAccessor>();
        var myEntity = (MyEntity) accessor.CreateEntity(typeof(MyEntity));
        entityId = myEntity.Id;
        t.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var accessor = session.Services.Get<DirectPersistentAccessor>();
        var myEntity = session.Query.All<MyEntity>().First(e => e.Id == entityId);
        updatedValue = "Value";
        accessor.SetFieldValue(myEntity, myEntity.TypeInfo.Fields["Value"], updatedValue);
        Assert.AreEqual(updatedValue, myEntity.Value);
        Assert.That(myEntity.PersistenceState, Is.EqualTo(PersistenceState.Modified));
        t.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var myEntity = session.Query.All<MyEntity>().First(e => e.Id == entityId);
        Assert.That(myEntity.Value, Is.EqualTo(updatedValue));
      }
    }

    [Test]
    public void SetFieldOfStructureTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var accessor = session.Services.Get<DirectPersistentAccessor>();
        var myStructure = (MyStructure) accessor.CreateStructure(typeof(MyStructure));
        accessor.SetFieldValue(myStructure, myStructure.TypeInfo.Fields["Value"], "Value");
        Assert.AreEqual("Value", myStructure.Value);
        Assert.That(myStructure.Owner, Is.Null);
        t.Complete();
      }
    }

    [Test]
    public void RemoveTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var accessor = session.Services.Get<DirectPersistentAccessor>();
        var myEntity = (MyEntity) accessor.CreateEntity(typeof(MyEntity));
        var key = myEntity.Key;
        Assert.IsNotNull(session.Query.SingleOrDefault(key));
        accessor.Remove(myEntity);
        Assert.AreEqual(PersistenceState.Removed, myEntity.PersistenceState);
        Assert.IsNull(session.Query.SingleOrDefault(key));
        t.Complete();
      }
    }

    [Test]
    public void EntitySetTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var pa = session.Services.Get<DirectPersistentAccessor>();
        var container = (MyEntity) pa.CreateEntity(typeof(MyEntity));
        var item1 = (MyEntity) pa.CreateEntity(typeof(MyEntity));
        var item2 = (MyEntity) pa.CreateEntity(typeof(MyEntity));

        var entitySetAccessor = session.Services.Get<DirectEntitySetAccessor>();
        var field = container.TypeInfo.Fields["Items"];
        var entitySet = entitySetAccessor.GetEntitySet(container, field);
        _ = entitySetAccessor.Add(entitySet, item1);
        _ = entitySetAccessor.Add(entitySet, item2);
        Assert.AreEqual(2, entitySet.Count);
        _ = entitySetAccessor.Remove(entitySet, item2);
        Assert.AreEqual(1, entitySet.Count);
        entitySetAccessor.Clear(entitySet);
        Assert.AreEqual(0, entitySet.Count);
        t.Complete();
      }
    }

    [Test]
    public void SetReferenceFieldForNewEntity()
    {
      var entityId = 0;
      var entityRefId = 0;
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var accessor = session.Services.Get<DirectPersistentAccessor>();
        var myEntity = (MyEntity) accessor.CreateEntity(typeof(MyEntity));
        var referenceEntity = new MyRefEntity();
        var field = myEntity.TypeInfo.Fields["Reference"];
        entityId = myEntity.Id;
        entityRefId = referenceEntity.Id;

        accessor.SetReferenceKey(myEntity, field, referenceEntity.Key);

        t.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var entity = session.Query.All<MyEntity>().First(e => e.Id == entityId);
        Assert.That(entity.Reference, Is.Not.Null);
        Assert.That(entity.Reference.Id, Is.EqualTo(entityRefId));
      }
    }

    [Test]
    public void SetReferenceFieldOfExistingEntity()
    {
      var entityId = 0;
      var entityRefId = 0;
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var accessor = session.Services.Get<DirectPersistentAccessor>();
        var myEntity = (MyEntity) accessor.CreateEntity(typeof(MyEntity));
        entityId = myEntity.Id;
        entityRefId = new MyRefEntity().Id;
        t.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var myEntity = session.Query.All<MyEntity>().First(e => e.Id == entityId);
        var referenceEntity = session.Query.All<MyRefEntity>().First(e => e.Id == entityRefId);

        var field = myEntity.TypeInfo.Fields["Reference"];
        var accessor = session.Services.Get<DirectPersistentAccessor>();
        accessor.SetReferenceKey(myEntity, field, referenceEntity.Key);
        t.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var myEntity = session.Query.All<MyEntity>().First(e => e.Id == entityId);
        Assert.That(myEntity.Reference, Is.Not.Null);
        Assert.That(myEntity.Reference.Id, Is.EqualTo(entityRefId));
      }
    }

    [Test]
    public void SetReferenceFieldOfExistingEntityWithSameKeyTest()
    {
      var entityId = 0;
      var entityRefId = 0;
      var referenceKey = (Key) null;
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var accessor = session.Services.Get<DirectPersistentAccessor>();
        var myEntity = (MyEntity) accessor.CreateEntity(typeof(MyEntity));
        var referenceEntity = new MyRefEntity();
        var field = myEntity.TypeInfo.Fields["Reference"];
        entityId = myEntity.Id;
        entityRefId = referenceEntity.Id;
        referenceKey = referenceEntity.Key;

        accessor.SetReferenceKey(myEntity, field, referenceEntity.Key);
        t.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var myEntity = session.Query.All<MyEntity>().First(e => e.Id == entityId);

        var field = myEntity.TypeInfo.Fields["Reference"];
        var accessor = session.Services.Get<DirectPersistentAccessor>();
        accessor.SetReferenceKey(myEntity, field, referenceKey);
        Assert.That(myEntity.PersistenceState, Is.EqualTo(PersistenceState.Synchronized));
        t.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var myEntity = session.Query.All<MyEntity>().First(e => e.Id == entityId);
        Assert.That(myEntity.Reference, Is.Not.Null);
        Assert.That(myEntity.Reference.Id, Is.EqualTo(entityRefId));
      }
    }

    [Test]
    public void SetReferenceFieldOfExistingEntityWithEquivalentKeyTest()
    {
      var entityId = 0;
      var entityRefId = 0;
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var accessor = session.Services.Get<DirectPersistentAccessor>();
        var myEntity = (MyEntity) accessor.CreateEntity(typeof(MyEntity));
        var referenceEntity = new MyRefEntity();
        var field = myEntity.TypeInfo.Fields["Reference"];
        entityId = myEntity.Id;
        entityRefId = referenceEntity.Id;

        accessor.SetReferenceKey(myEntity, field, referenceEntity.Key);
        t.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var myEntity = session.Query.All<MyEntity>().First(e => e.Id == entityId);

        var field = myEntity.TypeInfo.Fields["Reference"];
        var accessor = session.Services.Get<DirectPersistentAccessor>();

        var referenceKey = Key.Create(Domain, typeof(MyRefEntity), TypeReferenceAccuracy.ExactType, entityRefId);
        accessor.SetReferenceKey(myEntity, field, referenceKey);
        Assert.That(myEntity.PersistenceState, Is.EqualTo(PersistenceState.Synchronized));

        referenceKey = Key.Create(Domain, typeof(MyRefEntity), TypeReferenceAccuracy.BaseType, entityRefId);
        accessor.SetReferenceKey(myEntity, field, referenceKey);
        Assert.That(myEntity.PersistenceState, Is.EqualTo(PersistenceState.Synchronized));

        referenceKey = Key.Create(Domain, typeof(MyRefEntity), TypeReferenceAccuracy.Hierarchy, entityRefId);
        accessor.SetReferenceKey(myEntity, field, referenceKey);
        Assert.That(myEntity.PersistenceState, Is.EqualTo(PersistenceState.Synchronized));
        t.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var myEntity = session.Query.All<MyEntity>().First(e => e.Id == entityId);
        Assert.That(myEntity.Reference, Is.Not.Null);
        Assert.That(myEntity.Reference.Id, Is.EqualTo(entityRefId));
      }
    }
  }
}