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

namespace Xtensive.Storage.Tests.Storage.CoreServicesModel
{
  [HierarchyRoot(typeof(KeyGenerator), "Id")]
  public class X : Entity
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

    public X()
    {
      throw new InvalidOperationException();
    }

    #region Custom business logic

    protected override void OnInitialize()
    {
      base.OnInitialize();
      throw new InvalidOperationException();
    }

    protected override void OnSettingField<T>(Xtensive.Storage.Model.FieldInfo field, T value)
    {
      base.OnSettingField<T>(field, value);
      throw new InvalidOperationException();
    }

    protected override void OnSetField<T>(Xtensive.Storage.Model.FieldInfo field, T oldValue, T newValue)
    {
      base.OnSetField<T>(field, oldValue, newValue);
      throw new InvalidOperationException();
    }

    protected override void OnRemoving()
    {
      base.OnRemoving();
      throw new InvalidOperationException();
    }

    protected override void OnRemoved()
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
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof(X).Namespace);
      return config;
    }

    [Test]
    public void CreateInstanceTest()
    {
      using(Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var accessor = Session.Current.CoreServices.PersistentAccessor;
          X instance = (X)accessor.CreateInstance(typeof (X));
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
          X instance = (X)accessor.CreateInstance(typeof (X));
          instance.CoreServices.PersistentAccessor.SetField(instance, instance.Type.Fields["Value"], "Value");
          Assert.AreEqual("Value", instance.Value);
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
          X instance = (X)accessor.CreateInstance(typeof (X));
          Key key = instance.Key;
          Assert.IsNotNull(key.Resolve());
          instance.CoreServices.PersistentAccessor.Remove(instance);
          Assert.AreEqual(PersistenceState.Removed, instance.PersistenceState);
          Assert.IsNull(key.Resolve());
          t.Complete();
        }
      }
    }

  }
}