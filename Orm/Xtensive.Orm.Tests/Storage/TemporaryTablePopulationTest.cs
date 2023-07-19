// Copyright (C) 2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Storage.TemporaryTablePopulationTestModel;

namespace Xtensive.Orm.Tests.Storage.TemporaryTablePopulationTestModel
{
  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public sealed class TestEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = 50)]
    public string Name { get; private set; }

    public TestEntity(Session session, int id)
      : base(session, id)
    {
      Name = id.ToString();
    }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class TemporaryTablePopulationTest : AutoBuildTest
  {
    protected override void CheckRequirements() =>
      Require.AnyFeatureSupported(ProviderFeatures.TemporaryTables | ProviderFeatures.TemporaryTableEmulation);

    protected override DomainConfiguration BuildConfiguration()
    {
      var domainConfiguration = DomainConfigurationFactory.Create();
      domainConfiguration.MaxNumberOfConditions = 2; // basically force to use temp table
      domainConfiguration.Types.Register(typeof(TestEntity));
      domainConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      return domainConfiguration;
    }

    protected override void PopulateData()
    {
      using(var session = Domain.OpenSession())
      using(var tx = session.OpenTransaction()) {
        _ = new TestEntity(session, -300);

        tx.Complete();
      }
    }

    [Test]
    [TestCase(WellKnown.MultiRowInsertSmallBatchSize - 1)]
    [TestCase(WellKnown.MultiRowInsertSmallBatchSize)]
    [TestCase(WellKnown.MultiRowInsertSmallBatchSize + 2)]
    [TestCase(WellKnown.MultiRowInsertSmallBatchSize * 2 + 2)]
    [TestCase(WellKnown.MultiRowInsertBigBatchSize - 1)]
    [TestCase(WellKnown.MultiRowInsertBigBatchSize)]
    [TestCase(WellKnown.MultiRowInsertBigBatchSize + 2)]
    [TestCase(WellKnown.MultiRowInsertBigBatchSize + WellKnown.MultiRowInsertSmallBatchSize * 2)]
    [TestCase(WellKnown.MultiRowInsertBigBatchSize + WellKnown.MultiRowInsertSmallBatchSize * 2 + 2)]
    [TestCase(WellKnown.MultiRowInsertBigBatchSize * 2)]
    [TestCase(WellKnown.MultiRowInsertBigBatchSize * 2 + WellKnown.MultiRowInsertSmallBatchSize * 2)]
    [TestCase(WellKnown.MultiRowInsertBigBatchSize * 2 + WellKnown.MultiRowInsertSmallBatchSize * 2 + 2)]
    public void MainTest(int numOfValues)
    {
      var commands = new List<(string Command, int ParamtersCount)>();

      var values = Enumerable.Range(0, numOfValues).ToArray(numOfValues);

      var sessionConfig = new SessionConfiguration("DefaultNoBatching", SessionOptions.Default) { BatchSize = 1 };

      using (var session = Domain.OpenSession(sessionConfig))
      using (var tx = session.OpenTransaction()) {
        session.Events.DbCommandExecuting += LogCommands;
        _ = session.Query.All<TestEntity>().Where(e => e.Id.In(values)).ToList();
        session.Events.DbCommandExecuting += LogCommands;
      }

      var (bigBatches, smallBatches, sigleRows) = GetExpectedCommandsCount(numOfValues);

      Assert.That(commands.Count,
        Is.EqualTo(bigBatches + smallBatches + sigleRows));

      var bigBatchesCount = bigBatches;
      var smallBatchesCount = smallBatches;
      var singleRowsCount = sigleRows;

      for(var i = 0; i < commands.Count; i++) {
        var command = commands[i];
        if (bigBatchesCount != 0) {
          Assert.That(command.ParamtersCount, Is.EqualTo(WellKnown.MultiRowInsertBigBatchSize));
          bigBatchesCount--;
        }
        else if (smallBatchesCount != 0) {
          Assert.That(command.ParamtersCount, Is.EqualTo(WellKnown.MultiRowInsertSmallBatchSize));
          smallBatchesCount--;
        }
        else {
          Assert.That(command.ParamtersCount, Is.EqualTo(1));
        }
      }

      void LogCommands(object sender, DbCommandEventArgs args)
      {
        var c = args.Command;
        if (c.CommandText.Contains("INSERT")) {
          commands.Add((c.CommandText, c.Parameters.Count));
        }
      }
    }

    [Test]
    public void BatchesParameterCountControlCompatibilityTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.DmlBatches);

      var maxParametersCount = StorageProviderInfo.Instance.Info.MaxQueryParameterCount;
      if(maxParametersCount == int.MaxValue) {
        throw new IgnoreException("Test does not support unlimited parameter count.");
      }

      var commands = new List<(string Command, int ParamtersCount)>();

      // to not fit into one batch of statements
      var possibleParametersInBatch = SessionConfiguration.DefaultBatchSize * WellKnown.MultiRowInsertBigBatchSize;

      var numOfValues = (maxParametersCount > possibleParametersInBatch)
        ? ((possibleParametersInBatch / WellKnown.MultiRowInsertBigBatchSize) + 1) * WellKnown.MultiRowInsertBigBatchSize
        : ((maxParametersCount / WellKnown.MultiRowInsertBigBatchSize) + 1) * WellKnown.MultiRowInsertBigBatchSize;

      var values = Enumerable.Range(0, numOfValues).ToArray(numOfValues);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        session.Events.DbCommandExecuting += LogCommands;
        _ = session.Query.All<TestEntity>().Where(e => e.Id.In(values)).ToList();
        session.Events.DbCommandExecuting += LogCommands;
      }

      Assert.That(commands.Count, Is.EqualTo(2));

      commands = new List<(string Command, int ParamtersCount)>();

      // to not fit into one batch of statements
      numOfValues = (maxParametersCount > possibleParametersInBatch)
        ? (possibleParametersInBatch / WellKnown.MultiRowInsertBigBatchSize) * WellKnown.MultiRowInsertBigBatchSize
        : (maxParametersCount / WellKnown.MultiRowInsertBigBatchSize) * WellKnown.MultiRowInsertBigBatchSize;
      values = Enumerable.Range(0, numOfValues).ToArray(numOfValues);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        session.Events.DbCommandExecuting += LogCommands;
        _ = session.Query.All<TestEntity>().Where(e => e.Id.In(values)).ToList();
        session.Events.DbCommandExecuting += LogCommands;
      }

      Assert.That(commands.Count, Is.EqualTo(1));

      void LogCommands(object sender, DbCommandEventArgs args)
      {
        var c = args.Command;
        if (c.CommandText.Contains("INSERT")) {
          commands.Add((c.CommandText, c.Parameters.Count));
        }
      }
    }

    private (int bigBatches, int smallBatches, int sigleRows) GetExpectedCommandsCount(int numberOfValues)
    {
      var numberOfBigs = numberOfValues / WellKnown.MultiRowInsertBigBatchSize;
      var valuesLeft = numberOfValues - numberOfBigs * WellKnown.MultiRowInsertBigBatchSize;
      var numberOfSmalls = valuesLeft / WellKnown.MultiRowInsertSmallBatchSize;
      valuesLeft = valuesLeft - numberOfSmalls * WellKnown.MultiRowInsertSmallBatchSize;
      return (numberOfBigs, numberOfSmalls, valuesLeft);
    }
  }
}
