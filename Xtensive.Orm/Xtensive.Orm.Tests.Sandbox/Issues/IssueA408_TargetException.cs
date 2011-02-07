// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2011.02.07

using System;
using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Testing;
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
      config.Types.Register(typeof (Some).Assembly, typeof (Some).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Session.Open(Domain))
      using (var t = Transaction.Open(session))
      {
        var some = new Some {Tag = 100500, Reference = new Ref{Tag = 9000}, Structure = new Struct{Tag = 777}};
        var tagObject = some.GetProperty<object>("Tag");
        var tagValue = some.GetProperty<uint>("Tag");
        var tagIndexed = some["Tag"];
        AssertEx.Throws<InvalidCastException>(() => some.GetProperty<long>("Tag"));
        Assert.AreEqual(100500, tagValue);
        Assert.AreEqual(100500, (uint)tagObject);
        Assert.AreEqual(100500, (uint)tagIndexed);

        var refObject = some.GetProperty<object>("Reference.Tag");
        var refValue = some.Reference.Tag;
        Assert.AreEqual(9000, refValue);
        Assert.AreEqual(9000, (uint)refObject);

        var strObject = some.GetProperty<object>("Structure.Tag");
        var strValue = some.Structure.Tag;
        Assert.AreEqual(777, strValue);
        Assert.AreEqual(777, (uint)strObject);

        some.SetProperty("Tag", 111u);
        some.SetProperty("Reference.Tag", 111u);
        some.SetProperty("Structure.Tag", 111u);

        Assert.AreEqual(111u, some.Tag);
        Assert.AreEqual(111u, some.Reference.Tag);
        Assert.AreEqual(111u, some.Structure.Tag);

        t.Complete();
      }
    }
  }
}