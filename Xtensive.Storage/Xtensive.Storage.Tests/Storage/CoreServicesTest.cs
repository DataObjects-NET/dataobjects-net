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

    public X()
    {
      throw new InvalidOperationException();
    }
  }

  public class Y : Structure
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

    public Y()
    {
      throw new InvalidOperationException();
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
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof(X).Namespace);
      return config;
    }

    [Test]
    public void CreateInstanceTest()
    {
      using(Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var accessor = Session.Current.CoreServices.PersistentAccessor;
          accessor.CreateEntity(typeof (X));
          accessor.CreateStructure(typeof (Y));
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
          X x = (X)accessor.CreateEntity(typeof (X));
          x.CoreServices.PersistentAccessor.SetField(x, x.Type.Fields["Value"], "Value");
          Assert.AreEqual("Value", x.Value);
          Y y = (Y) accessor.CreateStructure(typeof(Y));
          y.CoreServices.PersistentAccessor.SetField(y, y.Type.Fields["Value"], "Value");
          Assert.AreEqual("Value", y.Value);
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
          X x = (X)accessor.CreateEntity(typeof (X));
          Key key = x.Key;
          Assert.IsNotNull(key.Resolve());
          x.CoreServices.PersistentAccessor.Remove(x);
          Assert.AreEqual(PersistenceState.Removed, x.PersistenceState);
          Assert.IsNull(key.Resolve());
          t.Complete();
        }
      }
    }

  }
}