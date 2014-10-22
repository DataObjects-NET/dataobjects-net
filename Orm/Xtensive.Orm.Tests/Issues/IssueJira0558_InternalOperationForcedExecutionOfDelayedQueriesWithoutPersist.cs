// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.10.20

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Tests.Issues.IssueJira0558_InternalOperationForcedExecutionOfDelayedQueriesWithoutPersistModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0558_InternalOperationForcedExecutionOfDelayedQueriesWithoutPersistModel
{
  [HierarchyRoot]
  public class Job : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Entity
  {
    [Key, Field]
    public int Id { get; set; }

    [Field]
    public EntitySet<Membership> Memberships { get; set; }
  }

  [HierarchyRoot]
  public class Membership : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public MembershipStatus Status { get; set; }

    [Field]
    public MembershipType Type { get; set; }
  }

  [HierarchyRoot]
  public class MembershipType : Entity
  {
    [Field, Key]
    public int Id { get; set; }
  }

  public enum MembershipStatus
  {
    Active,
    Inactive
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  
  public class IssueJira0558_InternalOperationForcedExecutionOfDelayedQueriesWithoutPersist : AutoBuildTest
  {
    private Key jobKey;

    protected override Orm.Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (MembershipType).Assembly, typeof (MembershipType).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var firstType = new MembershipType();
        var secondType = new MembershipType();
        var customer = new Customer();
        customer.Memberships.Add(new Membership { Status = MembershipStatus.Active, Type = firstType });
        customer.Memberships.Add(new Membership { Status = MembershipStatus.Active, Type = secondType });
        customer.Memberships.Add(new Membership { Status = MembershipStatus.Inactive, Type = firstType });
        customer.Memberships.Add(new Membership { Status = MembershipStatus.Inactive, Type = secondType });
        jobKey = new Job { Customer = customer }.Key;
        transaction.Complete();
      }
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var countBeforeInsertion = session.Query.All<Membership>().Count();
        var membership = new Membership() {Type = new MembershipType(), Status = MembershipStatus.Active};
        var countAfterInsertionUsingDelayedQuery = session.Query.ExecuteDelayed(endpoint => endpoint.All<Membership>().Count());
        GetAllMembershipWithTypes(GetMembershipTypes(jobKey));
        Assert.Greater(countAfterInsertionUsingDelayedQuery.Value, countBeforeInsertion);
        Assert.AreEqual(1, countAfterInsertionUsingDelayedQuery.Value - countBeforeInsertion);
      }
    }

    [Test]
    public void UserDefinedDelayedQueries()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var firstCustomer = session.Query.All<Customer>().First();
        var allMemberships = session.Query.ExecuteDelayed(queryEndpoint => queryEndpoint.All<Membership>());
        var countOfMemberships = session.Query.ExecuteDelayed(queryEndpoint => queryEndpoint.All<Membership>().Count());
        var allActiveMemberships = session.Query.ExecuteDelayed(queryEndpoint => queryEndpoint.All<Membership>().Where(el => el.Status==MembershipStatus.Active));
        var countOfActiveMemberships = session.Query.ExecuteDelayed(queryEndpoint => queryEndpoint.All<Membership>().Count(el => el.Status==MembershipStatus.Active));
        var userDefinedQueryTasks = GetUserDefinedQueryTasks(session);
        Assert.AreEqual(4, userDefinedQueryTasks.Count);
        //must be fetching without execution of user defined queries
        var customerMembership = firstCustomer.Memberships.ToList();
        userDefinedQueryTasks = GetUserDefinedQueryTasks(session);
        Assert.AreEqual(4, userDefinedQueryTasks.Count);
      }
    }

    private IQueryable<MembershipType> GetMembershipTypes(Key key)
    {
      var membershipTypes = Query.Single<Job>(key).Customer.Memberships
                .Where(m => m.Status == MembershipStatus.Active)
                .Select(m => m.Type);
      return membershipTypes;
    }

    private void GetAllMembershipWithTypes(IQueryable<MembershipType> types)
    {
      var memberships = Query.All<Membership>().Where(el => el.Type.In(types)).ToList();
    }

    private List<QueryTask> GetUserDefinedQueryTasks(Session session)
    {
      var type = session.GetType();
      var property = type.GetField("queryTasks", BindingFlags.Instance | BindingFlags.NonPublic);
      var result = property.GetValue(session);
      return (List<QueryTask>) result;
    }

    private List<QueryTask> GetInternalQueryTasks(Session session)
    {
      var type = session.GetType();
      var property = type.GetField("internalQueryTasks", BindingFlags.Instance | BindingFlags.NonPublic);
      var result = property.GetValue(session);
      return (List<QueryTask>)result;
    }
  }
}
