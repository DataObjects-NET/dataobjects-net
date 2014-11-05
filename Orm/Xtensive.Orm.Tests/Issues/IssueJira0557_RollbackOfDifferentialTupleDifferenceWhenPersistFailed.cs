// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.10.03

using System;
using System.Globalization;
using System.Linq;
using System.Transactions;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0557_RollbackOfDifferentialTupleDifferenceWhenPersistFailedModel;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Tests.Issues.IssueJira0557_RollbackOfDifferentialTupleDifferenceWhenPersistFailedModel
{
  [HierarchyRoot]
  [Index("UniqueField", Unique= true)]
  public class Call : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string ConflictedField { get; set; }

    [Field(Length = 250)]
    public string UniqueField { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  class IssueJira0557_RollbackOfDifferentialTupleDifferenceWhenPersistFailed : AutoBuildTest
  {
    private bool conflictAlreadyCatched;
    private object locableObject = new object();
    private DifferentialTuple globalTupleToTest;

    [Test]
    public void RestoringModifiedEntityWhenPersistFailedTest()
    {
      using (var session =  Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var firstCall = session.Query.All<Call>().First(el => el.ConflictedField=="3");
        Assert.IsNull(firstCall.State.DifferentialTuple.Difference);
        firstCall.UniqueField = "2";
        Assert.IsNotNull(firstCall.State.DifferentialTuple.Difference);
        bool exceptionCatched = false;
        try {
          session.SaveChanges();
        }
        catch(Exception exception) {
          exceptionCatched = true;
        }
        Assert.IsTrue(exceptionCatched);
        Assert.IsNotNull(firstCall.State.DifferentialTuple.Difference);
        Assert.AreEqual("2", firstCall.UniqueField);
      }
    }

    [Test]
    public void RestoringNewEntityWhenPersistFailed()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var firstCall = session.Query.All<Call>().First(el => el.ConflictedField=="3");
        firstCall.UniqueField = "2";
        var secondCall = new Call{ConflictedField = "new entity"};
        Assert.IsNull(secondCall.State.DifferentialTuple.Difference);
        bool exceptionCatched = false;
        try {
          session.SaveChanges();
        }
        catch (Exception exception) {
          exceptionCatched = true;
        }
        Assert.IsTrue(exceptionCatched);
        Assert.IsNull(secondCall.State.DifferentialTuple.Difference);
      }
    }

    [Test]
    public void RestoringNewModifiedEntityWhenPersistFailed()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var firstCall = session.Query.All<Call>().First(el => el.ConflictedField=="3");
        firstCall.UniqueField = "2";
        var secondCall = new Call { ConflictedField = "new entity" };
        Assert.IsNull(secondCall.State.DifferentialTuple.Difference);
        secondCall.ConflictedField = "new modified entity";
        Assert.IsNotNull(secondCall.State.DifferentialTuple.Difference);
        bool exceptionCatched = false;
        try {
          session.SaveChanges();
        }
        catch (Exception exception) {
          exceptionCatched = true;
        }
        Assert.IsTrue(exceptionCatched);
        Assert.IsNotNull(secondCall.State.DifferentialTuple.Difference);
        Assert.AreEqual("new modified entity", secondCall.ConflictedField);
      }
    }

    [Test]
    public void RestoringDeletedEntityWhenPersistWasFailed()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var firstCall = session.Query.All<Call>().First(el => el.ConflictedField=="3");
        var secondCall = session.Query.All<Call>().First(el => el.ConflictedField == "4");
        firstCall.UniqueField = "2";
        Assert.IsNull(secondCall.State.DifferentialTuple.Difference);
        secondCall.Remove();
        Assert.IsTrue(secondCall.IsRemoved);
        Assert.AreEqual(secondCall.PersistenceState, PersistenceState.Removed);
        bool exceptionCatched = false;
        try {
          session.SaveChanges();
        }
        catch (Exception exception) {
          exceptionCatched = true;
        }
        Assert.IsTrue(exceptionCatched);
        Assert.IsNull(secondCall.State.DifferentialTuple.Difference);
        Assert.IsTrue(secondCall.IsRemoved);
        Assert.AreEqual(secondCall.PersistenceState, PersistenceState.Removed);
      }
    }

    [Test]
    public void RestoringEditedAndDeletedEntityWhenPersistWasFailed()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var firstCall = session.Query.All<Call>().First(el => el.ConflictedField=="3");
        var secondCall = session.Query.All<Call>().First(el => el.ConflictedField == "4");
        firstCall.UniqueField = "2";
        Assert.IsNull(secondCall.State.DifferentialTuple.Difference);
        secondCall.ConflictedField = "edited removed entity";
        Assert.IsNotNull(secondCall.State.DifferentialTuple.Difference);
        secondCall.Remove();
        Assert.IsTrue(secondCall.IsRemoved);
        Assert.AreEqual(secondCall.PersistenceState, PersistenceState.Removed);
        bool exceptionCatched = false;
        try {
          session.SaveChanges();
        }
        catch (Exception exception) {
          exceptionCatched = true;
        }
        Assert.IsTrue(exceptionCatched);
        Assert.IsNotNull(secondCall.State.DifferentialTuple.Difference);
        Assert.IsTrue(secondCall.IsRemoved);
        Assert.AreEqual(secondCall.PersistenceState, PersistenceState.Removed);
      }
    }

    [Test]
    public void RestoreDifferetialTupleTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var call = session.Query.All<Call>().First();
        var originTuple = call.State.DifferentialTuple.Origin.Clone();
        Assert.IsNotNull(call.State.DifferentialTuple.Origin);
        Assert.IsNull(call.State.DifferentialTuple.Difference);
        call.ConflictedField = "aaaa";
        Assert.IsNotNull(call.State.DifferentialTuple.Origin);
        Assert.IsNotNull(call.State.DifferentialTuple.Difference);
        var originTupleAfterChange = call.State.DifferentialTuple.Origin.Clone();
        var differenceTupleAfterChange = call.State.DifferentialTuple.Difference.Clone();
        call.State.CommitDifference();
        Assert.IsNotNull(call.State.DifferentialTuple.Origin);
        Assert.AreNotEqual(originTupleAfterChange, call.State.DifferentialTuple.Origin);
        Assert.IsNull(call.State.DifferentialTuple.Difference);
        call.State.RestoreDifference();
        Assert.IsNotNull(call.State.DifferentialTuple.Origin);
        Assert.AreEqual(originTupleAfterChange, call.State.DifferentialTuple.Origin);
        Assert.IsNotNull(call.State.DifferentialTuple.Difference);
        Assert.AreEqual(differenceTupleAfterChange, call.State.DifferentialTuple.Difference);
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var domainConfiguration = base.BuildConfiguration();
      domainConfiguration.Types.Register(typeof (Call).Assembly, typeof(Call).Namespace);
      domainConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      return domainConfiguration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var tx = session.OpenTransaction()) {
        new Call {ConflictedField = "1", UniqueField = "1"};
        new Call {ConflictedField = "2", UniqueField = "2"};
        new Call {ConflictedField = "3", UniqueField = "3"};
        new Call {ConflictedField = "4", UniqueField = "4"};
        new Call {ConflictedField = "5", UniqueField = "5"};
        new Call {ConflictedField = "6", UniqueField = "6"};
        new Call {ConflictedField = "7", UniqueField = "7"};
        new Call {ConflictedField = "8", UniqueField = "8"};
        new Call {ConflictedField = "9", UniqueField = "9"};
        new Call {ConflictedField = "10", UniqueField = "10"};
        new Call {ConflictedField = "11", UniqueField = "11"};
        new Call {ConflictedField = "12", UniqueField = "12"};
        tx.Complete();
      }
    }
  }
}
