// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.03

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Tests.Storage.CoreServicesModel;
using FieldInfo=Xtensive.Storage.Model.FieldInfo;

namespace Xtensive.Storage.Tests.Storage.CoreServicesModel
{
  [HierarchyRoot(typeof(KeyGenerator), "Id")]
  public class MyEntity : Entity
  {
    [Field]
    public int Id { get; private set; }

    [Field]
    public string Value
    {
      get { return GetField<string>("Value"); }
      set
      {
        throw new InvalidOperationException();
        SetField("Value", value);
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

    protected override void OnSettingField(Xtensive.Storage.Model.FieldInfo field, object value)
    {
      base.OnSettingField(field, value);
      throw new InvalidOperationException();
    }

    protected override void OnSetField(Xtensive.Storage.Model.FieldInfo field, object oldValue, object newValue)
    {
      base.OnSetField(field, oldValue, newValue);
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

    public override void OnValidate()
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

  public class MyStructure : Structure
  {
    [Field]
    public string Value
    {
      get { return GetField<string>("Value"); }
      set
      {
        throw new InvalidOperationException();
        SetField("Value", value);
      }
    }

    #region Custom business logic

    protected override void OnInitialize()
    {
      base.OnInitialize();
      throw new InvalidOperationException();
    }

    protected override void OnSettingField(Xtensive.Storage.Model.FieldInfo field, object value)
    {
      base.OnSettingField(field, value);
      throw new InvalidOperationException();
    }

    protected override void OnSetField(Xtensive.Storage.Model.FieldInfo field, object oldValue, object newValue)
    {
      base.OnSetField(field, oldValue, newValue);
      throw new InvalidOperationException();
    }

    public override void OnValidate()
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
    where T : Entity
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

namespace Xtensive.Storage.Tests.Storage
{
  [TestFixture]
  public class CoreServicesTest : AutoBuildTest
  {
    protected override Xtensive.Storage.Configuration.DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof(MyEntity).Namespace);
      return config;
    }

    [Test]
    public void CreateInstanceTest()
    {
      using(Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var accessor = Session.Current.CoreServices.PersistentAccessor;
          accessor.CreateEntity(typeof (MyEntity));
          accessor.CreateStructure(typeof (MyStructure));
          t.Complete();
        }
      }
    }

    [Test]
    public void SetFieldTest()
    {
      using(Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var accessor = Session.Current.CoreServices.PersistentAccessor;
          MyEntity myEntity = (MyEntity)accessor.CreateEntity(typeof (MyEntity));
          myEntity.CoreServices.PersistentAccessor.SetField(myEntity, myEntity.Type.Fields["Value"], "Value");
          Assert.AreEqual("Value", myEntity.Value);
          MyStructure myStructure = (MyStructure) accessor.CreateStructure(typeof(MyStructure));
          myStructure.CoreServices.PersistentAccessor.SetField(myStructure, myStructure.Type.Fields["Value"], "Value");
          Assert.AreEqual("Value", myStructure.Value);
          t.Complete();
        }
      }
    }

    [Test]
    public void RemoveTest()
    {
      using(Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var accessor = Session.Current.CoreServices.PersistentAccessor;
          MyEntity myEntity = (MyEntity)accessor.CreateEntity(typeof (MyEntity));
          Key key = myEntity.Key;
          Assert.IsNotNull(key.Resolve());
          myEntity.CoreServices.PersistentAccessor.Remove(myEntity);
          Assert.AreEqual(PersistenceState.Removed, myEntity.PersistenceState);
          Assert.IsNull(key.Resolve());
          t.Complete();
        }
      }
    }

    [Test]
    public void EntitySetTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var pa = Session.Current.CoreServices.PersistentAccessor;
          MyEntity container = (MyEntity)pa.CreateEntity(typeof (MyEntity));
          MyEntity item1 = (MyEntity)pa.CreateEntity(typeof (MyEntity));
          MyEntity item2 = (MyEntity)pa.CreateEntity(typeof (MyEntity));

          var esa = Session.Current.CoreServices.EntitySetAccessor;
          esa.Add(container.Items, item1);
          esa.Add(container.Items, item2);
          Assert.AreEqual(2, container.Items.Count);
          t.Complete();
        }
      }
    }
  }
}