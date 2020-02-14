// Copyright (C) 2020 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2020.02.14

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Validation;
using Xtensive.Orm.Tests.Issues.IssueJira0792_UnableToRemoveAssignedEntityWithNonNullableAssociationFieldModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0792_UnableToRemoveAssignedEntityWithNonNullableAssociationFieldModel
{
  [HierarchyRoot]
  public class Job : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    [Association(PairTo = nameof (JobTechnology.ReqiredJob), OnTargetRemove = OnRemoveAction.Clear, OnOwnerRemove = OnRemoveAction.Cascade)]
    public JobTechnology Technology { get; set; }

    public Job(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  public class JobTechnology : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field(Nullable = false), NotNullConstraint]
    public Job ReqiredJob { get; set; }

    public JobTechnology(Session session, Job job)
      : base(session)
    {
      ReqiredJob = job;
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0792_UnableToRemoveAssignedEntityWithNonNullableAssociationField : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Job).Assembly, typeof (Job).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new JobTechnology(session, new Job(session));
        new JobTechnology(session, new Job(session));
        new JobTechnology(session, new Job(session));
        new JobTechnology(session, new Job(session));

        transaction.Complete();
      }
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        foreach (var job in session.Query.All<Job>()) {
          job.Technology.Remove();
          session.SaveChanges();
        }
      }
    }
  }
}
