// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.02.18

using System;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Storage.BatchingTestModel;

namespace Xtensive.Orm.Tests.Storage.BatchingTestModel
{
  [HierarchyRoot]
  public class TestEntity : Entity
  {
    [Key, Field]
    public long Id { get; set; }

    [Field]
    public int SomeIntField { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  [Serializable]
  public class BatchingTest : AutoBuildTest
  {
    private const int TotalEntities = 10;

    [Test]
    public void BatchSize0Test()
    {
      RunTest(0);
    }

    [Test]
    public void BatchSize1Test()
    {
      RunTest(1);
    }

    [Test]
    public void BatchSize2Test()
    {
      RunTest(2);
    }

    [Test]
    public void BatchSize100Test()
    {
      RunTest(100);
    }

    [Test]
    public void BatchSizeNegativeTest()
    {
      RunTest(-666); // Heil, satan ]:->
    }

    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Batches);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(TestEntity).Assembly, typeof(TestEntity).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    private int GetExpectedNumberOfBatches(int batchSize, Statement statement)
    {
      var additionalBatches = GetExpectedAdditionalBatches(statement);
      if (batchSize <= 1)
        return TotalEntities + additionalBatches;
      if (batchSize >= TotalEntities)
        return 1 + additionalBatches;
      int remaining;
      var result = Math.DivRem(TotalEntities, batchSize, out remaining);
      result = remaining!=0 ? result + 1 : result;
      return result + additionalBatches;
    }

    private int GetExpectedAdditionalBatches(Statement statement)
    {
      var expectedAdditionalInsertBatches = 1;
      var expectedAdditionlUpadteBatches = 1;
      var expectedAdditionalDeletebatches = 1;

      if (Domain.StorageProviderInfo.ProviderName=="mysql")
        expectedAdditionalInsertBatches = 0;
      switch (statement) {
        case Statement.Insert:
          return expectedAdditionalInsertBatches;
        case Statement.Update:
          return expectedAdditionlUpadteBatches;
        case Statement.Delete:
          return expectedAdditionalDeletebatches;
      }
      throw new InvalidOperationException("Such statement is not defined.");
    }

    private enum Statement
    {
      Insert,
      Update,
      Delete
    }

    private void RunTest(int batchSize)
    {
      var commandsExecuted = 0;
      EventHandler<DbCommandEventArgs> commandExectued = (sender, args) => {
        commandsExecuted++;
      };
      using (var session = Domain.OpenSession(new SessionConfiguration {BatchSize = batchSize, Options = SessionOptions.ServerProfile | SessionOptions.AutoActivation})) {
        session.Events.DbCommandExecuted += commandExectued;
        try {
          using (var transcation = session.OpenTransaction()) {
            for (int i = 0; i < TotalEntities; i++) {
              new TestEntity {SomeIntField = i};
            }
            transcation.Complete();
          }
          Assert.That(commandsExecuted, Is.EqualTo(GetExpectedNumberOfBatches(batchSize, Statement.Insert)));
          commandsExecuted = 0;

          using (var transaction = session.OpenTransaction()) {
            session.Query.All<TestEntity>().ForEach(te => te.SomeIntField++);
            transaction.Complete();
          }
          Assert.That(commandsExecuted, Is.EqualTo(GetExpectedNumberOfBatches(batchSize,Statement.Update)));
          commandsExecuted = 0;

          using (var transaction = session.OpenTransaction()) {
            session.Query.All<TestEntity>().ForEach(te => te.Remove());
            transaction.Complete();
          }
          Assert.That(commandsExecuted, Is.EqualTo(GetExpectedNumberOfBatches(batchSize, Statement.Delete)));
          session.Events.DbCommandExecuted -= commandExectued;
        }
        catch (Exception) {
          session.Events.DbCommandExecuted -= commandExectued;
          throw;
        }
      }      
    }
  }
}