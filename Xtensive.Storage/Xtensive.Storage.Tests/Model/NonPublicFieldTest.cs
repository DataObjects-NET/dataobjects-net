// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.09.14

using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Model.NonPublicFieldTestModel;

namespace Xtensive.Storage.Tests.Model.NonPublicFieldTestModel
{
  [HierarchyRoot]
  public class MyEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string PublicField { get; set;}

    [Field]
    internal string InternalField { get; set; }

    [Field]
    private string PrivateField { get; set; }

    [Field]
    public string ReadOnlyField { get; private set; }

    public void SetState()
    {
      PublicField = "public";
      InternalField = "internal";
      PrivateField = "private";
    }

    public bool ValidateState()
    {
      return PublicField=="public" && InternalField=="internal" && PrivateField=="private";
    }
  }
}

namespace Xtensive.Storage.Tests.Model
{
  public class NonPublicFieldTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (MyEntity).Assembly, typeof (MyEntity).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      Key key;
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {

          var myEntity = new MyEntity();
          key = myEntity.Key;
          myEntity.SetState();

          t.Complete();
        }
      }

      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {

          var myEntity = Query<MyEntity>.Single(key);
          Assert.IsTrue(myEntity.ValidateState());

          t.Complete();
        }
      }
    }
  }
}