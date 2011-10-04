// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2011.03.25

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.NotEqualsForNullableColumnTest_Model;
using System.Linq;

namespace Xtensive.Storage.Tests.Issues
{
  namespace NotEqualsForNullableColumnTest_Model
  {
    [HierarchyRoot]
    public class SomeWithNullable : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field(Nullable = false)]
      public string Name { get; set; }

      [Field]
      public string Tag { get; set; }

      [Field]
      public Reference Ref { get; set; }
    }

    [HierarchyRoot]
    public class Reference : Entity
    {
      [Field, Key]
      public long Id { get; private set; }
    }
  }

  [Serializable]
  public class NotEqualsForNullableColumnTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (SomeWithNullable).Assembly, typeof (SomeWithNullable).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Session.Open(Domain))
      using (var t = Transaction.Open(session)) {
        var reference = new Reference();
        new SomeWithNullable {Name = "A"};
        new SomeWithNullable {Name = "A", Tag ="Tag"};
        new SomeWithNullable {Name = "A", Ref = reference};
        new SomeWithNullable {Name = "A", Tag = "Tag", Ref=reference};

        var result = Query.All<SomeWithNullable>().Where(s => s.Name == "A").ToList();
        Assert.AreEqual(4, result.Count);
        result = Query.All<SomeWithNullable>().Where(s => s.Name != "B").ToList();
        Assert.AreEqual(4, result.Count);
        result = Query.All<SomeWithNullable>().Where(s => s.Tag != "Tag" || s.Tag == null).ToList();
        Assert.AreEqual(2, result.Count);
        result = Query.All<SomeWithNullable>().Where(s => s.Tag != "!Tag" || s.Tag == null).ToList();
        Assert.AreEqual(4, result.Count);
        result = Query.All<SomeWithNullable>().Where(s => s.Ref == reference).ToList();
        Assert.AreEqual(2, result.Count);
        result = Query.All<SomeWithNullable>().Where(s => s.Ref != reference || s.Ref == null).ToList();
        Assert.AreEqual(2, result.Count);
        
        t.Complete();
      }
    }
  }
}