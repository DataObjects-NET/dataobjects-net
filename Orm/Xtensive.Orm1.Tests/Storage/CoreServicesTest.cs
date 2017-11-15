// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.03

using System;
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
    public MyEntitySet<MyEntity> Items { get; private set; }

    #region Custom business logic

    protected override void OnInitialize()
    {
      base.OnInitialize();
      throw new InvalidOperationException();
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
}

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class CoreServicesTest : AutoBuildTest
  {
    protected override Xtensive.Orm.Configuration.DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof(MyEntity).Namespace);
      return config;
    }

    [Test]
    public void CreateInstanceTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var accessor = Session.Current.Services.Get<DirectPersistentAccessor>();
          accessor.CreateEntity(typeof (MyEntity));
          accessor.CreateStructure(typeof (MyStructure));
          t.Complete();
        }
      }
    }

    [Test]
    public void SetFieldTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var accessor = Session.Current.Services.Get<DirectPersistentAccessor>();
          var myEntity = (MyEntity) accessor.CreateEntity(typeof (MyEntity));
          myEntity.Session.Services.Get<DirectPersistentAccessor>().SetFieldValue(myEntity, myEntity.TypeInfo.Fields["Value"], "Value");
          Assert.AreEqual("Value", myEntity.Value);
          var myStructure = (MyStructure) accessor.CreateStructure(typeof (MyStructure));
          myStructure.Session.Services.Get<DirectPersistentAccessor>().SetFieldValue(myStructure, myStructure.TypeInfo.Fields["Value"], "Value");
          Assert.AreEqual("Value", myStructure.Value);
          t.Complete();
        }
      }
    }

    [Test]
    public void RemoveTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var accessor = Session.Current.Services.Get<DirectPersistentAccessor>();
          MyEntity myEntity = (MyEntity)accessor.CreateEntity(typeof (MyEntity));
          Key key = myEntity.Key;
          Assert.IsNotNull(session.Query.SingleOrDefault(key));
          myEntity.Session.Services.Get<DirectPersistentAccessor>().Remove(myEntity);
          Assert.AreEqual(PersistenceState.Removed, myEntity.PersistenceState);
          Assert.IsNull(session.Query.SingleOrDefault(key));
          t.Complete();
        }
      }
    }

    [Test]
    public void EntitySetTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var pa = Session.Current.Services.Get<DirectPersistentAccessor>();
          MyEntity container = (MyEntity)pa.CreateEntity(typeof (MyEntity));
          MyEntity item1 = (MyEntity)pa.CreateEntity(typeof (MyEntity));
          MyEntity item2 = (MyEntity)pa.CreateEntity(typeof (MyEntity));

          var entitySetAccessor = Session.Current.Services.Get<DirectEntitySetAccessor>();
          var field = container.TypeInfo.Fields["Items"];
          var entitySet = entitySetAccessor.GetEntitySet(container, field);
          entitySetAccessor.Add(entitySet, item1);
          entitySetAccessor.Add(entitySet, item2);
          Assert.AreEqual(2, entitySet.Count);
          entitySetAccessor.Remove(entitySet, item2);
          Assert.AreEqual(1, entitySet.Count);
          entitySetAccessor.Clear(entitySet);
          Assert.AreEqual(0, entitySet.Count);
          t.Complete();
        }
      }
    }
  }
}