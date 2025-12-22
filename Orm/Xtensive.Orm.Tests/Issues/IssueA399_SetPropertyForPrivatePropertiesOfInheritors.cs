// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2011.02.07

using System;
using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueA399_SetPropertyForPrivatePropertiesOfInheritors_Model;

namespace Xtensive.Orm.Tests.Issues
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
      config.Types.RegisterCaching(typeof (Derived).Assembly, typeof (Derived).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var derived = new Derived();

        derived.SetProperty("Foo", "foo!!!");
        derived.SetProperty("Bar", 9000);

        Assert.That(derived.Foo, Is.EqualTo("foo!!!"));
        Assert.That(derived.GetProperty<int>("Bar"), Is.EqualTo(9000));

        derived["Foo"] = "!!!foo!!!";
        derived["Bar"] =  9;

        Assert.That(derived["Foo"], Is.EqualTo("!!!foo!!!"));
        Assert.That(derived["Bar"], Is.EqualTo(9));
        t.Complete();
      }
    }
  }
}