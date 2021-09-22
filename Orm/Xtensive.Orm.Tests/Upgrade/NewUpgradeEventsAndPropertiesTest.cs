// Copyright (C) 2012-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.09.28

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Orm.Tests.Upgrade.NewUpgradeEventsAndPropertiesTestModel;
using Xtensive.Orm.Upgrade;
using V1 = Xtensive.Orm.Tests.Upgrade.NewUpgradeEventsAndPropertiesTestModel.Version1;
using V2 = Xtensive.Orm.Tests.Upgrade.NewUpgradeEventsAndPropertiesTestModel.Version2;

namespace Xtensive.Orm.Tests.Upgrade
{
  namespace NewUpgradeEventsAndPropertiesTestModel
  {
    public enum ExpectedEvent
    {
      OnPrepare,
      OnPrepareAsync,
      OnBeforeStage,
      OnBeforeStageAsync,
      OnStage,
      OnStageAsync,
      OnUpgrade,
      OnUpgradeAsync,
      OnBeforeExecuteActions,
      OnBeforeExecuteActionsAsync,
      OnSchemaReady,
      OnSchemaReadyAsync,
      OnComplete,
      OnCompleteAsync,
      None,
    }

    public class EventExecutionSequenceInfo
    {
      public readonly IReadOnlyCollection<ExpectedEvent> ExecutionSequence;

      public EventExecutionSequenceInfo(IReadOnlyCollection<ExpectedEvent> executionSequence)
      {
        ExecutionSequence = executionSequence;
      }
    }

    public abstract class CheckingUpgradeHandler : UpgradeHandler
    {
      private ExpectedEvent expectedEvent = ExpectedEvent.OnPrepare;
      private ExpectedEvent expectedEventAsync = ExpectedEvent.OnPrepareAsync;

      private readonly List<ExpectedEvent> executionSequence = new List<ExpectedEvent>();

      public override void OnPrepare()
      {
        CheckExpectedEvent(ExpectedEvent.OnPrepare, ExpectedEvent.OnBeforeStage);
        CheckNonStageEvent();
        executionSequence.Add(ExpectedEvent.OnPrepare);
      }

      public override async ValueTask OnPrepareAsync(CancellationToken token = default)
      {
        CheckExpectedEventForAsync(ExpectedEvent.OnPrepareAsync, ExpectedEvent.OnBeforeStageAsync);
        CheckNonStageEvent();
        executionSequence.Add(ExpectedEvent.OnPrepareAsync);
        _ = await Task.FromResult(0);
      }

      public override void OnBeforeStage()
      {
        base.OnBeforeStage();

        CheckExpectedEvent(ExpectedEvent.OnBeforeStage, ExpectedEvent.OnSchemaReady);
        CheckNonStageEvent();
        executionSequence.Add(ExpectedEvent.OnBeforeStage);
      }

      public override async ValueTask OnBeforeStageAsync(CancellationToken token = default)
      {
        CheckExpectedEventForAsync(ExpectedEvent.OnBeforeStageAsync, ExpectedEvent.OnSchemaReadyAsync);
        CheckNonStageEvent();
        executionSequence.Add(ExpectedEvent.OnBeforeStageAsync);
        _ = await Task.FromResult(0);
      }

      public override void OnSchemaReady()
      {
        CheckExpectedEvent(ExpectedEvent.OnSchemaReady, ExpectedEvent.OnBeforeExecuteActions);
        CheckNonStageEvent();
        executionSequence.Add(ExpectedEvent.OnSchemaReady);
      }

      public override async ValueTask OnSchemaReadyAsync(CancellationToken token = default)
      {
        CheckExpectedEventForAsync(ExpectedEvent.OnSchemaReadyAsync, ExpectedEvent.OnBeforeExecuteActionsAsync);
        CheckNonStageEvent();
        executionSequence.Add(ExpectedEvent.OnSchemaReadyAsync);
        _ = await Task.FromResult(0);
      }

      public override void OnBeforeExecuteActions(UpgradeActionSequence actions)
      {
        Assert.That(actions, Is.Not.Null);
        CheckExpectedEvent(ExpectedEvent.OnBeforeExecuteActions, ExpectedEvent.OnStage);
        CheckNonStageEvent();
        executionSequence.Add(ExpectedEvent.OnBeforeExecuteActions);
      }

      public override async ValueTask OnBeforeExecuteActionsAsync(UpgradeActionSequence actions, CancellationToken token = default)
      {
        Assert.That(actions, Is.Not.Null);
        CheckExpectedEventForAsync(ExpectedEvent.OnBeforeExecuteActionsAsync, ExpectedEvent.OnStageAsync);
        CheckNonStageEvent();
        executionSequence.Add(ExpectedEvent.OnBeforeExecuteActionsAsync);
        _ = await Task.FromResult(0);
      }

      public override void OnStage()
      {
        var nextEvent = UpgradeContext.Stage==UpgradeStage.Upgrading
          ? ExpectedEvent.OnUpgrade
          : ExpectedEvent.OnComplete;

        CheckExpectedEvent(ExpectedEvent.OnStage, nextEvent);
        CheckStageEvent();
        base.OnStage();

        executionSequence.Add(ExpectedEvent.OnStage);
      }

      public override async ValueTask OnStageAsync(CancellationToken token = default)
      {
        var nextEvent = UpgradeContext.Stage == UpgradeStage.Upgrading
          ? ExpectedEvent.OnUpgradeAsync
          : ExpectedEvent.OnCompleteAsync;

        CheckExpectedEventForAsync(ExpectedEvent.OnStageAsync, nextEvent);
        CheckStageEvent();
        executionSequence.Add(ExpectedEvent.OnStageAsync);
        await base.OnStageAsync();
        _ = await Task.FromResult(0);
      }

      public override void OnUpgrade()
      {
        base.OnUpgrade();
        CheckExpectedEvent(ExpectedEvent.OnUpgrade, ExpectedEvent.OnBeforeStage);
        CheckStageEvent();
        executionSequence.Add(ExpectedEvent.OnUpgrade);
      }

      protected override async ValueTask OnUpgradeAsync(CancellationToken token = default)
      {
        CheckExpectedEventForAsync(ExpectedEvent.OnUpgradeAsync, ExpectedEvent.OnBeforeStageAsync);
        CheckStageEvent();
        executionSequence.Add(ExpectedEvent.OnUpgradeAsync);
        _ = await Task.FromResult(0);
      }

      public override void OnComplete(Domain domain)
      {
        Assert.That(domain, Is.Not.Null);
        CheckExpectedEvent(ExpectedEvent.OnComplete, ExpectedEvent.None);
        CheckNonStageEvent();
        executionSequence.Add(ExpectedEvent.OnComplete);
        domain.Extensions.Set(new EventExecutionSequenceInfo(executionSequence));
      }

      public override async ValueTask OnCompleteAsync(Domain domain, CancellationToken token = default)
      {
        Assert.That(domain, Is.Not.Null);
        CheckExpectedEventForAsync(ExpectedEvent.OnCompleteAsync, ExpectedEvent.None);
        CheckNonStageEvent();
        executionSequence.Add(ExpectedEvent.OnCompleteAsync);
        domain.Extensions.Set(new EventExecutionSequenceInfo(executionSequence));
        _ = await Task.FromResult(0);
      }

      private void CheckNonStageEvent()
      {
        Assert.That(UpgradeContext.Session, Is.Null);
        CheckConnection();
      }

      private void CheckStageEvent() => Assert.That(UpgradeContext.Session, Is.Not.Null);

      private void CheckConnection()
      {
        Assert.That(UpgradeContext.Connection, Is.Not.Null);
        Assert.That(UpgradeContext.Connection.State, Is.EqualTo(ConnectionState.Open));
        Assert.That(UpgradeContext.Transaction, Is.Not.Null);
      }

      private void CheckExpectedEvent(ExpectedEvent actual, ExpectedEvent next)
      {
        Assert.That(actual, Is.EqualTo(expectedEvent));
        expectedEvent = next;
      }

      private void CheckExpectedEventForAsync(ExpectedEvent actual, ExpectedEvent next)
      {
        Assert.That(actual, Is.EqualTo(expectedEventAsync));
        expectedEventAsync = next;
      }
    }

    namespace Version1
    {
      [HierarchyRoot]
      public abstract class MyEntity : Entity
      {
        [Key, Field]
        public long Id { get; private set; }
      }

      public class Upgrader : CheckingUpgradeHandler
      {
        protected override string DetectAssemblyVersion() => "1";
      }
    }

    namespace Version2
    {
      [HierarchyRoot]
      public abstract class MyEntity : Entity
      {
        [Key, Field]
        public long Id { get; private set; }
      }

      public class Upgrader : CheckingUpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override string DetectAssemblyVersion() => "2";

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          _ = hints.Add(new RenameTypeHint(typeof (V1.MyEntity).FullName, typeof (MyEntity)));
        }
      }
    }
  }

  [TestFixture]
  public class NewUpgradeEventsAndPropertiesTest
  {
    [Test]
    public void ParallelTest() => RunTest(true);

    [Test]
    public async Task ParallelAsyncTest() => await RunAsyncTest(true);

    [Test]
    public void SequentalTest() => RunTest(false);

    [Test]
    public async Task SequentalAsyncTest() => await RunAsyncTest(false);

    private void RunTest(bool buildInParallel)
    {
      var initialDomain = BuildInitialDomain(buildInParallel);
      using (initialDomain) {
        var sequenceInfo = initialDomain.Extensions.Get<EventExecutionSequenceInfo>();
        CheckAllSync(sequenceInfo.ExecutionSequence);
      }

      var upgradedDomain = BuildUpgradedDomain(buildInParallel);
      using (upgradedDomain) {
        var sequenceInfo = initialDomain.Extensions.Get<EventExecutionSequenceInfo>();
        CheckAllSync(sequenceInfo.ExecutionSequence);
      }
    }

    private async Task RunAsyncTest(bool buildInParallel)
    {
      var initialDomain = await BuildInitialDomainAsync(buildInParallel);
      using (initialDomain) {
        var sequenceInfo = initialDomain.Extensions.Get<EventExecutionSequenceInfo>();
        CheckAllAsync(sequenceInfo.ExecutionSequence);
      }
      var upgradedDomain = await BuildUpgradedDomainAsync(buildInParallel);
      using (upgradedDomain) {
        var sequenceInfo = initialDomain.Extensions.Get<EventExecutionSequenceInfo>();
        CheckAllAsync(sequenceInfo.ExecutionSequence);
      }
    }

    private void CheckAllAsync(IReadOnlyCollection<ExpectedEvent> sequence)
    {
      foreach (var item in sequence) {
        switch (item) {
          case ExpectedEvent.OnPrepare:
          case ExpectedEvent.OnBeforeStage:
          case ExpectedEvent.OnStage:
          case ExpectedEvent.OnUpgrade:
          case ExpectedEvent.OnBeforeExecuteActions:
          case ExpectedEvent.OnSchemaReady:
          case ExpectedEvent.OnComplete:
            throw new AssertionException("Not all events executed asynchronously.");
        }
      }
    }

    private void CheckAllSync(IReadOnlyCollection<ExpectedEvent> sequence)
    {
      foreach (var item in sequence) {
        switch (item) {
          case ExpectedEvent.OnPrepareAsync:
          case ExpectedEvent.OnBeforeStageAsync:
          case ExpectedEvent.OnStageAsync:
          case ExpectedEvent.OnUpgradeAsync:
          case ExpectedEvent.OnBeforeExecuteActionsAsync:
          case ExpectedEvent.OnSchemaReadyAsync:
          case ExpectedEvent.OnCompleteAsync:
            throw new AssertionException("Not all events executed synchronously.");
        }
      }
    }

    private Domain BuildInitialDomain(bool buildInParallel)
    {
      return BuildDomain(DomainUpgradeMode.Recreate, typeof (V1.MyEntity), buildInParallel);
    }

    private async Task<Domain> BuildInitialDomainAsync(bool buildInParallel)
    {
      return await BuildDomainAsync(DomainUpgradeMode.Recreate, typeof(V1.MyEntity), buildInParallel);
    }

    private Domain BuildUpgradedDomain(bool inParallel)
    {
      return BuildDomain(DomainUpgradeMode.PerformSafely, typeof (V2.MyEntity), inParallel);
    }

    private async Task<Domain> BuildUpgradedDomainAsync(bool inParallel)
    {
      return await BuildDomainAsync(DomainUpgradeMode.PerformSafely, typeof(V2.MyEntity), inParallel);
    }

    private Domain BuildDomain(DomainUpgradeMode upgradeMode, Type sampleType, bool buildInParallel)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(sampleType.Assembly, sampleType.Namespace);
      configuration.BuildInParallel = buildInParallel;
      return Domain.Build(configuration);
    }

    private async Task<Domain> BuildDomainAsync(DomainUpgradeMode upgradeMode, Type sampleType, bool buildInParallel)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(sampleType.Assembly, sampleType.Namespace);
      configuration.BuildInParallel = buildInParallel;
      return await Domain.BuildAsync(configuration);
    }
  }
}