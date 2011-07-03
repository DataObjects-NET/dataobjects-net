// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2011.02.07

using System;
using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.IssueA399_SetPropertyForPrivatePropertiesOfInheritors_Model;

namespace Xtensive.Storage.Tests.Issues
{
  namespace IssueA399_SetPropertyForPrivatePropertiesOfInheritors_Model
  {
    public abstract class Base : Entity
    {
      [Field]
      public string Foo { get; private set; }
    }

    [HierarchyRoot]
    public class Derived : Base
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      private int Bar { get; set; }
    }
  }

  [Serializable]
  public class IssueA399_SetPropertyForPrivatePropertiesOfInheritors : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Derived).Assembly, typeof (Derived).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Session.Open(Domain))
      using (var t = Transaction.Open(session)) {
        var derived = new Derived();

//        Assert.AreEqual("FOO", derived.Foo);
//        Assert.AreEqual(100500, derived.GetProperty<int>("Bar"));

        derived.SetProperty("Foo", "foo!!!");
        derived.SetProperty("Bar", 9000);

        Assert.AreEqual("foo!!!", derived.Foo);
        Assert.AreEqual(9000, derived.GetProperty<int>("Bar"));

        derived["Foo"] = "!!!foo!!!";
        derived["Bar"] =  9;

        Assert.AreEqual("!!!foo!!!", derived["Foo"]);
        Assert.AreEqual(9, derived["Bar"]);
        t.Complete();
      }
    }
  }
}