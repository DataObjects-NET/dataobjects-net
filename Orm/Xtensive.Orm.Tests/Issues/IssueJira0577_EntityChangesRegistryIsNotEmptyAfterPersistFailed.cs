// Copyright (C) 2015 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.12.30

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0577_EntityChangesRegistryIsNotEmptyAfterPersistFailedModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0577_EntityChangesRegistryIsNotEmptyAfterPersistFailedModel
{
  [HierarchyRoot]
  [Index("UniqueField", Unique = true)]
  public class Call : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string TextField { get; set; }

    [Field(Length = 250)]
    public string UniqueField { get; set; }

    [Field]
    public Caller Caller { get; set; }

    [Field]
    public EntitySet<Parameter> Parameters { get; set; }
  }

  [HierarchyRoot]
  public class Caller : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Name { get; set; }
  }

  [HierarchyRoot]
  public class Parameter : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Value { get; set; }
  }
}


namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0577_EntityChangesRegistryIsNotEmptyAfterPersistFailed : AutoBuildTest
  {
    [Test]
    public void PersistFailedServerProfileOnTransactionComplete()
    {
      using (var session = Domain.OpenSession()) {
        Assert.Catch<Exception>(
          () => {
            using (var transaction = session.OpenTransaction()) {
              var call = session.Query.All<Call>().First(el => el.TextField == "3");
              Assert.IsNull(call.State.DifferentialTuple.Difference);
              call.UniqueField = "2";
              call.Caller = new Caller() {Name = "Caller"};
              call.Parameters.Add(new Parameter() {Value = "parameterValue"});
              Assert.AreEqual(4, session.EntityChangeRegistry.Count);
              Assert.AreEqual(0, session.EntityReferenceChangesRegistry.RemovedReferencesCount);
              Assert.AreEqual(2, session.EntityReferenceChangesRegistry.AddedReferencesCount);
              Assert.AreEqual(1, session.EntitySetChangeRegistry.Count);

              //must be persist
              transaction.Complete();
            }
          }
          );
        Assert.AreEqual(0, session.EntityChangeRegistry.Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.RemovedReferencesCount);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.AddedReferencesCount);
        Assert.AreEqual(0, session.EntitySetChangeRegistry.Count);
      }
    }

    [Test]
    public void PersistFailedServerProfileOnTransactionRollbacked()
    {
      using (var session = Domain.OpenSession()) {
        Assert.DoesNotThrow(
          () => {
            using (var transaction = session.OpenTransaction()) {
              var call = session.Query.All<Call>().First(el => el.TextField == "3");
              Assert.IsNull(call.State.DifferentialTuple.Difference);
              call.UniqueField = "2";
              call.Caller = new Caller() { Name = "Caller" };
              call.Parameters.Add(new Parameter() { Value = "parameterValue" });
              Assert.AreEqual(4, session.EntityChangeRegistry.Count);
              Assert.AreEqual(0, session.EntityReferenceChangesRegistry.RemovedReferencesCount);
              Assert.AreEqual(2, session.EntityReferenceChangesRegistry.AddedReferencesCount);
              Assert.AreEqual(1, session.EntitySetChangeRegistry.Count);
            }
          }
          );
        Assert.AreEqual(0, session.EntityChangeRegistry.Count);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.RemovedReferencesCount);
        Assert.AreEqual(0, session.EntityReferenceChangesRegistry.AddedReferencesCount);
        Assert.AreEqual(0, session.EntitySetChangeRegistry.Count);
      }
    }

    [Test]
    public void PerstistFailedClientProfile()
    {
      
    }


    protected override DomainConfiguration BuildConfiguration()
    {
      var domainConfiguration = base.BuildConfiguration();
      domainConfiguration.Types.Register(typeof(Call).Assembly, typeof(Call).Namespace);
      domainConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      return domainConfiguration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var tx = session.OpenTransaction()) {
        new Call {TextField = "1", UniqueField = "1"};
        new Call {TextField = "2", UniqueField = "2"};
        new Call {TextField = "3", UniqueField = "3"};
        new Call {TextField = "4", UniqueField = "4"};
        new Call {TextField = "5", UniqueField = "5"};
        new Call {TextField = "6", UniqueField = "6"};
        new Call {TextField = "7", UniqueField = "7"};
        new Call {TextField = "8", UniqueField = "8"};
        new Call {TextField = "9", UniqueField = "9"};
        new Call {TextField = "10", UniqueField = "10"};
        new Call {TextField = "11", UniqueField = "11"};
        new Call {TextField = "12", UniqueField = "12"};
        tx.Complete();
      }
    }

  }
}
