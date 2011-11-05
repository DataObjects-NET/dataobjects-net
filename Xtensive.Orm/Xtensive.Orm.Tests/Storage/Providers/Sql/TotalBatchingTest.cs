// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.20

using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Providers;
using Xtensive.Storage.Rse;
using Xtensive.Orm.Tests.Storage.DbTypeSupportModel;

namespace Xtensive.Orm.Tests.Storage.Providers.Sql
{
  [TestFixture]
  public class TotalBatchingTest : AutoBuildTest
  {
    private const int BatchSize = 25;

    private List<QueryTask> tasks = new List<QueryTask>();
    private Session session;

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (X).Assembly, typeof (X).Namespace);
      return configuration;
    }

    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.Sql);
    }

    [Test]
    public void PersistTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        Initialize();
        CreateEntities(2 * BatchSize + 5);
        ValidateEntities(2 * BatchSize + 5);
      }
    }

    [Test]
    public void QueryTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        Initialize();
        CreateEntities(3);
        session.SaveChanges();
        CreateQueries(5);
        session.ExecuteDelayedQueries(true);
        ValidateQueries(3);
      }
    }

    [Test]
    public void PersistQueryTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        Initialize();
        CreateEntities(5);
        CreateQueries(5);
        session.ExecuteDelayedQueries(false);
        ValidateQueries(5);
        ValidateEntities(5);
      }
    }

    [Test]
    public void PersistQuerySelectTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        Initialize();
        CreateEntities(2);
        CreateQueries(2);
        ValidateEntities(2);
        ValidateQueries(2);
      }      
    }

    private void Initialize()
    {
      tasks.Clear();
      session = Session.Demand();
    }

    private void CreateQueries(int amount)
    {
      for (int i = 0; i < amount; i++) {
        var rs = Domain.Model.Types[typeof (X)].Indexes.PrimaryIndex.ToRecordQuery()
          .Aggregate(ArrayUtils<int>.EmptyArray,
            new AggregateColumnDescriptor("_count_", 0, AggregateType.Count));
        var compiledProvider = session.CompilationService.Compile(rs.Provider);
        var task = new QueryTask(compiledProvider, null);
        tasks.Add(task);
        Session.Current.RegisterDelayedQuery(task);
      }
    }

    private void CreateEntities(int amount)
    {
      for (int i = 0; i < amount; i++) {
        new X {FString = "Hello Batcher", FInt = BatchSize};
      }
    }

    private void ValidateEntities(int expectedCount)
    {
      Assert.AreEqual(expectedCount, Session.Demand().Query.All<X>().Count());
      foreach (var item in Session.Demand().Query.All<X>()) {
        Assert.AreEqual(item.FString, "Hello Batcher");
        Assert.AreEqual(item.FInt, BatchSize);
      }
    }

    private void ValidateQueries(long expectedResult)
    {
      foreach (var task in tasks) {
        Assert.IsNotNull(task.Result);
        Assert.AreEqual(1, task.Result.Count);
        Assert.AreEqual(expectedResult, task.Result[0].GetValueOrDefault<long>(0));
      }
    }
  }
}