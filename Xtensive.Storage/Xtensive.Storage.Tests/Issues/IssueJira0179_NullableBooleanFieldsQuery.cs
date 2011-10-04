// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.09.23

using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Sandbox.IssueJira0179_NullableBooleanFieldsQueryModel;

namespace Xtensive.Storage.Tests.Sandbox.IssueJira0179_NullableBooleanFieldsQueryModel
{
  [HierarchyRoot]
  public class FlagContainer : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public bool? Flag { get; private set; }

    public FlagContainer(bool? flag)
    {
      Flag = flag;
    }
  }
}

namespace Xtensive.Storage.Tests.Sandbox
{
  public class IssueJira0179_NullableBooleanFieldsQuery : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (FlagContainer).Assembly, typeof (FlagContainer).Namespace);
      return config;
    }

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          new FlagContainer(true);
          new FlagContainer(false);
          new FlagContainer(null);
          t.Complete();
        }
      }
    }

    [Test]
    public void EqualsNullTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var result = Query.All<FlagContainer>().Count(c => c.Flag==null);
          Assert.AreEqual(1, result);
          // Rollback
        }
      }
    }

    [Test]
    public void NotEqualsNullTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var result = Query.All<FlagContainer>().Count(c => c.Flag!=null);
          Assert.AreEqual(2, result);
          // Rollback
        }
      }
    }

    [Test]
    public void HasValueTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var result = Query.All<FlagContainer>().Count(c => c.Flag.HasValue);
          Assert.AreEqual(2, result);
          // Rollback
        }
      }
    }


    [Test]
    public void NotHasValueTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var result = Query.All<FlagContainer>().Count(c => !c.Flag.HasValue);
          Assert.AreEqual(1, result);
          // Rollback
        }
      }
    }

    [Test]
    public void GetValueOrDefault1Test()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var result = Query.All<FlagContainer>().Count(c => !c.Flag.GetValueOrDefault());
          Assert.AreEqual(2, result);
          // Rollback
        }
      }
    }

    [Test]
    public void GetValueOrDefault2Test()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var result = Query.All<FlagContainer>().Count(c => c.Flag.GetValueOrDefault(true));
          Assert.AreEqual(2, result);
          // Rollback
        }
      }
    }

    [Test]
    public void CoalesceTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var result = Query.All<FlagContainer>().Count(c => c.Flag ?? true);
          Assert.AreEqual(2, result);
          // Rollback
        }
      }
    }

    [Test]
    public void NotCoalesceTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var result = Query.All<FlagContainer>().Count(c => !(c.Flag ?? true));
          Assert.AreEqual(1, result);
          // Rollback
        }
      }
    }
  }
}