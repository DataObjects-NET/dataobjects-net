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
      configuration.Types.Register(typeof(TestEntity).Assembly, typeof(TestEntity).Namespace);
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
      using (var session = Domain.OpenSession(new SessionConfiguration {BatchSize = batchSize, Options = SessionOptions.ServerProfile | SessionOptions.AutoActivation})) {
        var commandCapturer = new CommandCapturer();
        using (var transcation = session.OpenTransaction()) {
          for (int i = 0; i < TotalEntities; i++) {
            new TestEntity {SomeIntField = i, BatchSize = batchSize};
          }
          using (commandCapturer.Monitor()) {
            session.SaveChanges();
            Assert.That(commandCapturer.CountCommands, Is.EqualTo(expectedNumberOfBatches));
          }
          transcation.Complete();
        }
        using (session.OpenTransaction()) {
          session.Query.All<TestEntity>().Where(e => e.BatchSize==batchSize).ForEach(te => te.SomeIntField++);
          using (commandCapturer.Monitor()) {
            session.SaveChanges();
            Assert.That(commandCapturer.CountCommands, Is.EqualTo(expectedNumberOfBatches));
          }
        }
        using (session.OpenTransaction()) {
          session.Query.All<TestEntity>().Where(e => e.BatchSize==batchSize).ForEach(te => te.Remove());
          using (commandCapturer.Monitor()) {
            session.SaveChanges();
            Assert.That(commandCapturer.CountCommands, Is.EqualTo(expectedNumberOfBatches));
          }
        }
      }
    }

    protected class CommandCapturer
    {
      private readonly Session session;

      public int CountCommands { get; private set; }

      public IDisposable Monitor()
      {
        EventHandler<DbCommandEventArgs> handler = (sender, args) => { CountCommands++; };
        session.Events.DbCommandExecuted += handler;
        return new Disposable(b => {
          session.Events.DbCommandExecuted -= handler;
          CountCommands = 0;
        });
      }

      public CommandCapturer(Session session=null)
      {
        if (session!=null)
          this.session = session;
        else if (Session.Current!=null)
          this.session = Session.Current;
        else
          throw new InvalidOperationException("There is no session in the current scope. And it is not passed as a parameter.");
      }
    }
  }
}