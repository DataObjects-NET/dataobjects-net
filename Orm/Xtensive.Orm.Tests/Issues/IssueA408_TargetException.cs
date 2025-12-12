// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2011.02.07

using System;
using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueA408_TargetException_Model;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueA408_TargetException_Model
  {
    [HierarchyRoot]
    public class Some : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public uint Tag { get; set; }

      [Field]
      public Struct Structure { get; set; }

      [Field]
      public Ref Reference { get; set; }
    }

    public class Struct: Structure
    {
      [Field]
      public uint Tag { get; set; }
    }

    [HierarchyRoot]
    public class Ref : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public uint Tag { get; set; }
    }
  }

  [Serializable]
  public class IssueA408_TargetException : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.RegisterCaching(typeof (Some).Assembly, typeof (Some).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction())
      {
        var some = new Some {Tag = 100500, Reference = new Ref{Tag = 9000}, Structure = new Struct{Tag = 777}};
        var tagObject = some.GetProperty<object>("Tag");
        var tagValue = some.GetProperty<uint>("Tag");
        var tagIndexed = some["Tag"];
        AssertEx.Throws<InvalidCastException>(() => some.GetProperty<long>("Tag"));
        Assert.That(tagValue, Is.EqualTo(100500));
        Assert.That((uint)tagObject, Is.EqualTo(100500));
        Assert.That((uint)tagIndexed, Is.EqualTo(100500));

        var refObject = some.GetProperty<object>("Reference.Tag");
        var refValue = some.Reference.Tag;
        Assert.That(refValue, Is.EqualTo(9000));
        Assert.That((uint)refObject, Is.EqualTo(9000));

        var strObject = some.GetProperty<object>("Structure.Tag");
        var strValue = some.Structure.Tag;
        Assert.That(strValue, Is.EqualTo(777));
        Assert.That((uint)strObject, Is.EqualTo(777));

        some.SetProperty("Tag", 111u);
        some.SetProperty("Reference.Tag", 111u);
        some.SetProperty("Structure.Tag", 111u);

        Assert.That(some.Tag, Is.EqualTo(111u));
        Assert.That(some.Reference.Tag, Is.EqualTo(111u));
        Assert.That(some.Structure.Tag, Is.EqualTo(111u));

        t.Complete();
      }
    }
  }
}