// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.06.24

using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue0725.Model;
using Xtensive.Core;

namespace Xtensive.Storage.Tests.Issues.Issue0725.Model
{
  [Serializable]
  [HierarchyRoot]
  public class Unit : Entity
  {
    [Key, Field]
    public Guid Id { get; private set; }

    [Field]
    public string Title { get; set; }

    public override string ToString()
    {
      return Title;
    }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  [TestFixture]
  public class Issue0725_DisconnectedStateDisconnectBugs : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(Unit).Assembly, typeof(Unit).Namespace);
      return configuration;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      var domain = base.BuildDomain(configuration);
      using (var session = Session.Open(domain))
      using (var tx = Transaction.Open()) {
        new Unit() { Title = "Unit" };
        tx.Complete();
      }
      return domain;
    }

    [Test]
    public void CombinedTest()
    {
      var ds = new DisconnectedState();
      using (var session = Session.Open(Domain))
      using (ds.Attach(session)) {
        using (ds.Connect()) {
          using (var tx = Transaction.Open()) {
            var unit = new Unit { Title = "Unit 2" };
            tx.Complete(); // Local transaction completed.
          }
          Assert.Less(0, ds.Operations.Count);
          ds.ApplyChanges();
          Assert.IsTrue(ds.IsAttached);
          Assert.IsTrue(ds.IsConnected);
          Assert.AreEqual(0, ds.Operations.Count);
          
          var oldUnit = Query.All<Unit>().Where(u => u.Title=="Unit").Single();
          int unitCount = Query.All<Unit>().Count();
          Assert.AreEqual("Unit", oldUnit.Title);
          Assert.AreEqual(2, unitCount);
        }
        Assert.IsTrue(ds.IsAttached);
        Assert.IsFalse(ds.IsConnected);
      }
      Assert.IsFalse(ds.IsAttached);
      Assert.IsFalse(ds.IsConnected);
    }
  }
}