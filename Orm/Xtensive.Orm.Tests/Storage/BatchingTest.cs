// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.02.18

using System;
using System.Linq;
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

    [Field]
    public int BatchSize { get; set; }
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
      configuration.Types.Register(typeof (TestEntity).Assembly, typeof (TestEntity).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    private int GetExpectedNumberOfBatches(int batchSize)
    {
      var size = (batchSize < 1) ? 1 : batchSize;
      return (int)Math.Ceiling((decimal)TotalEntities / size);
    }

    private void RunTest(int batchSize)
    {
      var expectedNumberOfBatches = GetExpectedNumberOfBatches(batchSize);
      var counter = new CommandCounter();
      using (var session = Domain.OpenSession(new SessionConfiguration {BatchSize = batchSize, Options = SessionOptions.ServerProfile | SessionOptions.AutoActivation})) {
        using (var transcation = session.OpenTransaction()) {
          for (int i = 0; i < TotalEntities; i++) {
            new TestEntity {SomeIntField = i, BatchSize = batchSize};
          }
          using (counter.Attach(session)) {
            session.SaveChanges();
          }
          Assert.That(counter.CountedCommands, Is.EqualTo(expectedNumberOfBatches));
          counter.Reset();
          transcation.Complete();
        }
        using (session.OpenTransaction()) {
          session.Query.All<TestEntity>().Where(e => e.BatchSize==batchSize).ForEach(te => te.SomeIntField++);
          using (counter.Attach(session)) {
            session.SaveChanges();
          }
          Assert.That(counter.CountedCommands, Is.EqualTo(expectedNumberOfBatches));
          counter.Reset();
        }
        using (session.OpenTransaction()) {
          session.Query.All<TestEntity>().Where(e => e.BatchSize==batchSize).ForEach(te => te.Remove());
          using (counter.Attach(session)) {
            session.SaveChanges();
          }
          Assert.That(counter.CountedCommands, Is.EqualTo(expectedNumberOfBatches));
          counter.Reset();
        }
      }
    }

    protected class CommandCounter
    {
      public int CountedCommands { get; private set; }

      public IDisposable Attach(Session session)
      {
        session.Events.DbCommandExecuted += DbCommandExecutedHandler;
        return new Disposable(
          (disposing) => {
            session.Events.DbCommandExecuted -= DbCommandExecutedHandler;
          });
      }

      public void Reset()
      {
        CountedCommands = 0;
      }

      private void DbCommandExecutedHandler(object sender, DbCommandEventArgs e)
      {
        CountedCommands++;
      }
    }
  }
}