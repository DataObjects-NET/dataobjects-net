// Copyright (C) 2020-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Services;
using Xtensive.Orm.Tests.Issues.IssueGitHub0110_SimpleCommandProcessorOverridesOriginalExceptionModel;

namespace Xtensive.Orm.Tests.Issues.IssueGitHub0110_SimpleCommandProcessorOverridesOriginalExceptionModel
{
  [HierarchyRoot(InheritanceSchema.ConcreteTable)]
  public class Bin : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    public Bin(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot(InheritanceSchema.ConcreteTable)]
  [Index("Name", Unique = true)]
  [Index(nameof(Active), nameof(N), nameof(M))]
  public class InventoryBalance : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field(Nullable = false)]
    public Bin Bin { get; private set; }

    [Field]
    public bool Active { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public decimal N { get; set; }

    [Field]
    public decimal M { get; set; }

    [Field(DefaultSqlExpression = "GETUTCDATE()")]
    public DateTime ModifiedOn { get; set; }

    public InventoryBalance(Session session, string name, Bin bin)
      : base(session)
    {
      Name = name;
      Active = true;
      Bin = bin;
    }
  }

  public class OperationState
  {
    public Exception CatchendException { get; set; }

    public bool Ended { get; set; }

    public Domain Domain { get; }

    public OperationState(Domain domain)
    {
      Domain = domain;
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueGitHub0110_SimpleCommandProcessorOverridesOriginalException : AutoBuildTest
  {
    private static ManualResetEvent theStarter;

    protected override void CheckRequirements() => Require.ProviderIs(StorageProvider.SqlServer);

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(InventoryBalance).Assembly, typeof(InventoryBalance).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;

      var defaultSessionConfig = configuration.Sessions.Default;
      if (defaultSessionConfig == null) {
        defaultSessionConfig = new SessionConfiguration(WellKnown.Sessions.Default, SessionOptions.ServerProfile);
        configuration.Sessions.Add(defaultSessionConfig);
      }

      defaultSessionConfig.DefaultIsolationLevel = IsolationLevel.Snapshot;
      defaultSessionConfig.DefaultCommandTimeout = 60 * 60; // 60 min.

      defaultSessionConfig.BatchSize = 1; //force to use SimpleCommandProcessor

      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        _ = new InventoryBalance(session, "aaa", new Bin(session));
        tx.Complete();
      }
    }

    [SetUp]
    public void InitStarter() => theStarter = new ManualResetEvent(false);

    [TearDown]
    public void DisposeStarter()
    {
      theStarter.DisposeSafely();
      theStarter = null;
    }

    [Test]
    public void MainTest()
    {
      var task1State = new OperationState(Domain);
      var task2State = new OperationState(Domain);

      var task1 = new Task(Outer, task1State);
      var task2 = new Task(Outer, task2State);
      task1.Start();
      task2.Start();

      Thread.Sleep(700);

      _ = theStarter.Set();

      while (!task1State.Ended || !task2State.Ended) {
        Thread.Sleep(100);
      }

      var exception = task1State.CatchendException ?? task2State.CatchendException;
      Assert.That(exception, Is.Not.Null);
      Assert.That(exception, Is.InstanceOf<TransactionSerializationFailureException>());
    }

    private static void Outer(object state)
    {
      var operationState = (OperationState) state;

      try {
        using (var session = operationState.Domain.OpenSession(SessionType.User))
        using (var tx = session.OpenTransaction()) {
          var binId = session.Query.All<Bin>().First().Id;
          var bin = session.Query.SingleOrDefault<Bin>(binId);
          Inner(session, bin);
          tx.Complete();
        }
        operationState.Ended = true;
      }
      catch (Exception ex) {
        operationState.CatchendException = ex;
        operationState.Ended = true;
      }
    }

    private static void Inner(Session session, Bin bin)
    {
      using (var tx = session.OpenTransaction()) {
        var doc = session.Query.All<InventoryBalance>().SingleOrDefault(o => o.Active == true && o.Bin == bin);
        doc.N += 1;
        _ = theStarter.WaitOne();
        tx.Complete();
      }
    }
  }
}
