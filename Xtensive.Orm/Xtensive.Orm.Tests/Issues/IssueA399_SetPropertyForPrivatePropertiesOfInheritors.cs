// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2011.02.07

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueA399_SetPropertyForPrivatePropertiesOfInheritors_Model;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueA399_SetPropertyForPrivatePropertiesOfInheritors_Model
  {
    [HierarchyRoot]
    public abstract class Base : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public string Foo { get; private set; }

      [Field]
      private int Bar { get; set; }

      protected Base(string foo, int bar)
      {
        Foo = foo;
        Bar = bar;
      }
    }

    public class Derived : Base
    {
      public Derived(string foo, int bar)
        : base(foo, bar)
      {
      }
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
      using (var s = Domain.OpenSession())
        using (var t = s.OpenTransaction()) {
        var derived = new Derived("FOO", 100500);

        Assert.AreEqual("FOO", derived.Foo);
        Assert.AreEqual(100500, derived.GetProperty<int>("Bar"));

        derived.SetProperty("Foo", "foo!!!");
        derived.SetProperty("Bar", 9000);

        Assert.AreEqual("foo!!!", derived.Foo);
        Assert.AreEqual(9000, derived.GetProperty<int>("Bar"));
        t.Complete();
      }
    }
  }
}