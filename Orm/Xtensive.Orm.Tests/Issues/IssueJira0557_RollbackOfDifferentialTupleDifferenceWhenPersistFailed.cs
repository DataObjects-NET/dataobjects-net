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
    [Test]
    public void RestoringModifiedEntityWhenPersistFailedTest()
    {
      using (var session =  Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var firstCall = session.Query.All<Call>().First(el => el.ConflictedField=="3");
        Assert.That(firstCall.State.DifferentialTuple.Difference, Is.Null);
        firstCall.UniqueField = "2";
        Assert.That(firstCall.State.DifferentialTuple.Difference, Is.Not.Null);
        bool exceptionCatched = false;
        try {
          session.SaveChanges();
        }
        catch(Exception) {
          exceptionCatched = true;
        }
        Assert.That(exceptionCatched, Is.True);
        Assert.That(firstCall.State.DifferentialTuple.Difference, Is.Not.Null);
        Assert.That(firstCall.UniqueField, Is.EqualTo("2"));
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
        Assert.That(secondCall.State.DifferentialTuple.Difference, Is.Null);
        bool exceptionCatched = false;
        try {
          session.SaveChanges();
        }
        catch (Exception) {
          exceptionCatched = true;
        }
        Assert.That(exceptionCatched, Is.True);
        Assert.That(secondCall.State.DifferentialTuple.Difference, Is.Null);
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
        Assert.That(secondCall.State.DifferentialTuple.Difference, Is.Null);
        secondCall.ConflictedField = "new modified entity";
        Assert.That(secondCall.State.DifferentialTuple.Difference, Is.Not.Null);
        bool exceptionCatched = false;
        try {
          session.SaveChanges();
        }
        catch (Exception) {
          exceptionCatched = true;
        }
        Assert.That(exceptionCatched, Is.True);
        Assert.That(secondCall.State.DifferentialTuple.Difference, Is.Not.Null);
        Assert.That(secondCall.ConflictedField, Is.EqualTo("new modified entity"));
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
        Assert.That(secondCall.State.DifferentialTuple.Difference, Is.Null);
        secondCall.Remove();
        Assert.That(secondCall.IsRemoved, Is.True);
        Assert.That(PersistenceState.Removed, Is.EqualTo(secondCall.PersistenceState));
        bool exceptionCatched = false;
        try {
          session.SaveChanges();
        }
        catch (Exception) {
          exceptionCatched = true;
        }
        Assert.That(exceptionCatched, Is.True);
        Assert.That(secondCall.IsRemoved, Is.True);
        Assert.That(PersistenceState.Removed, Is.EqualTo(secondCall.PersistenceState));
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
        Assert.That(secondCall.State.DifferentialTuple.Difference, Is.Null);
        secondCall.ConflictedField = "edited removed entity";
        Assert.That(secondCall.State.DifferentialTuple.Difference, Is.Not.Null);
        secondCall.Remove();
        Assert.That(secondCall.IsRemoved, Is.True);
        Assert.That(PersistenceState.Removed, Is.EqualTo(secondCall.PersistenceState));
        bool exceptionCatched = false;
        try {
          session.SaveChanges();
        }
        catch (Exception) {
          exceptionCatched = true;
        }
        Assert.That(exceptionCatched, Is.True);
        Assert.That(secondCall.IsRemoved, Is.True);
        Assert.That(PersistenceState.Removed, Is.EqualTo(secondCall.PersistenceState));
      }
    }

    [Test]
    public void RestoreDifferetialTupleTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var call = session.Query.All<Call>().First();
        var originTuple = call.State.DifferentialTuple.Origin.Clone();
        Assert.That(call.State.DifferentialTuple.Origin, Is.Not.Null);
        Assert.That(call.State.DifferentialTuple.Difference, Is.Null);
        call.ConflictedField = "aaaa";
        Assert.That(call.State.DifferentialTuple.Origin, Is.Not.Null);
        Assert.That(call.State.DifferentialTuple.Difference, Is.Not.Null);
        var originTupleAfterChange = call.State.DifferentialTuple.Origin.Clone();
        var differenceTupleAfterChange = call.State.DifferentialTuple.Difference.Clone();
        call.State.CommitDifference();
        Assert.That(call.State.DifferentialTuple.Origin, Is.Not.Null);
        Assert.That(call.State.DifferentialTuple.Origin, Is.Not.EqualTo(originTupleAfterChange));
        Assert.That(call.State.DifferentialTuple.Difference, Is.Null);
        call.State.RestoreDifference();
        Assert.That(call.State.DifferentialTuple.Origin, Is.Not.Null);
        Assert.That(call.State.DifferentialTuple.Origin, Is.EqualTo(originTupleAfterChange));
        Assert.That(call.State.DifferentialTuple.Difference, Is.Not.Null);
        Assert.That(call.State.DifferentialTuple.Difference, Is.EqualTo(differenceTupleAfterChange));
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var domainConfiguration = base.BuildConfiguration();
      domainConfiguration.Types.RegisterCaching(typeof (Call).Assembly, typeof(Call).Namespace);
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
