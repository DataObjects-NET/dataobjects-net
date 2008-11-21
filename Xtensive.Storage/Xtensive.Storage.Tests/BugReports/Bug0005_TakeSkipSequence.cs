// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.20

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Tests.BugReports.Bug0005_Model;

namespace Xtensive.Storage.Tests.BugReports.Bug0005_Model
{
  [HierarchyRoot(typeof (KeyGenerator), "Id")]
  public class MyEntity : Entity
  {
    [Field]
    public int Id { get; private set; }

    [Field]
    public int Field1 { get; set; }

    [Field]
    public int Field2 { get; set; }

    [Field]
    public int Field3 { get; set; }
  }
}

namespace Xtensive.Storage.Tests.BugReports
{
  public class Bug0005_TakeSkipSequence : AutoBuildTest
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
      Assert.AreEqual(0, new[] {1, 2}.Take(1).Skip(1).Count());

      using (Domain.OpenSession())
      using (var trs = Transaction.Open()) {
        var e1 = new MyEntity();
        var e2 = new MyEntity();
        RecordSet rsMyEntities = Domain.Model.Types[typeof (MyEntity)]
          .Indexes.PrimaryIndex.ToRecordSet()
          .Range(e1.Key.Value, e2.Key.Value);

        Assert.AreEqual(2, rsMyEntities.Count());

        Assert.AreEqual(0, rsMyEntities.Take(1).Skip(1).Count());
        Assert.AreEqual(1, rsMyEntities.Take(1).Take(2).Count());
        Assert.AreEqual(0, rsMyEntities.Skip(1).Skip(1).Count());
        trs.Complete();
      }
    }
  }
}