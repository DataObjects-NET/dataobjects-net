// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2019.04.14

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Tests.Storage.CommandProcessing
{
  public class BatchingCommandProcessorParametersManagement : AutoBuildTest
  {
    public int StorageLimit
    {
      get { return ProviderInfo.MaxQueryParameterCount; }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (ALotOfFieldsEntityValid).Assembly, typeof (ALotOfFieldsEntityValid).Namespace);
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
    }

    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new ALotOfFieldsEntityValid();
        new ALotOfFieldsEntityValid();
        new ALotOfFieldsEntityVersionized();
        new ALotOfFieldsEntityVersionized();
        transaction.Complete();
      }
    }

    [Test]
    public void EachQueryInSeparateCommandTest()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        int[] ids = Enumerable.Range(1, StorageLimit - 1).ToArray();

        var results = new List<IEnumerable<ALotOfFieldsEntityValid>>(10);

        results.Add(session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        var inlineQuery = session.Query.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids));
        using (counter.Attach()) {
          Assert.That(inlineQuery.Any(), Is.True);
          Assert.That(counter.Count, Is.EqualTo(11));
        }

        foreach (var result in results)
          Assert.That(result.Any(), Is.True);
      }
    }

    [Test]
    public void InlineQueryInSeparateBatchTest()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        int[] ids = Enumerable.Range(1, (StorageLimit - 1) / 2).ToArray();

        List<IEnumerable<ALotOfFieldsEntityValid>> results = new List<IEnumerable<ALotOfFieldsEntityValid>>(10);

        results.Add(session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        var inlineQuery = session.Query.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, new []{1 , 2}));
        using (counter.Attach()) {
          Assert.That(inlineQuery.Any(), Is.True);
          Assert.That(counter.Count, Is.EqualTo(6));
        }

        foreach (var result in results)
          Assert.That(result.Any(), Is.True);
      }
    }

    [Test]
    public void InlineQueryInLastBatchTest()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        int[] ids = Enumerable.Range(1, (StorageLimit - 3) / 2).ToArray();

        List<IEnumerable<ALotOfFieldsEntityValid>> results = new List<IEnumerable<ALotOfFieldsEntityValid>>(10);

        results.Add(session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        var inlineQuery = session.Query.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, new[] { 1, 2 }));

        using (counter.Attach()) {
          Assert.That(inlineQuery.Any(), Is.True);
          Assert.That(counter.Count, Is.EqualTo(5));
        }

        foreach (var result in results)
          Assert.That(result.Any(), Is.True);
      }
    }

    [Test]
    public void DelayedSelectsOutOfLimitTest01()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        int[] fittedIds = Enumerable.Range(1, StorageLimit - 1).ToArray();
        int[] idsOutOfRange = Enumerable.Range(1, StorageLimit + 1).ToArray();

        var result = (DelayedSequence<ALotOfFieldsEntityValid>) session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, idsOutOfRange)));

        using (counter.Attach())
          Assert.Throws<ParametersLimitExceededException>(() => result.Run());
        Assert.That(result.Task.Result.Count, Is.EqualTo(0));
        Assert.That(counter.Count, Is.EqualTo(0));
      }
    }

    [Test]
    public void DelayedSelectsOutOfLimitTest02()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        int[] fittedIds = Enumerable.Range(1, StorageLimit - 1).ToArray();
        int[] idsOutOfRange = Enumerable.Range(1, StorageLimit + 1).ToArray();

        var result = (DelayedSequence<ALotOfFieldsEntityValid>) session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, idsOutOfRange)));

        using (counter.Attach())
          Assert.Throws<ParametersLimitExceededException>(() => session.Query.All<ALotOfFieldsEntityValid>().Run());
        Assert.That(result.Task.Result.Count, Is.EqualTo(0));
        Assert.That(counter.Count, Is.EqualTo(0));
      }
    }

    [Test]
    public void DelayedSelectsOutOfLimitTest03()
    {
      using (var session = Domain.OpenSession())
      using (var counter =new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        int[] fittedIds = Enumerable.Range(1, StorageLimit - 1).ToArray();
        int[] idsOutOfRange = Enumerable.Range(1, StorageLimit + 1).ToArray();

        var result = (DelayedSequence<ALotOfFieldsEntityValid>)session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, fittedIds)));

        session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, fittedIds)));

        session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, idsOutOfRange)));

        using (counter.Attach())
          Assert.Throws<ParametersLimitExceededException>(() => session.Query.All<ALotOfFieldsEntityValid>().Run());
        Assert.That(counter.Count, Is.EqualTo(1));
        Assert.That(result.Any(), Is.True);
        
      }
    }

    [Test]
    public void DelayedSelectsOutOfLimitTest04()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        int[] fittedIds = Enumerable.Range(1, StorageLimit - 1).ToArray();
        int[] idsOutOfRange = Enumerable.Range(1, StorageLimit + 1).ToArray();

        var result = (DelayedSequence<ALotOfFieldsEntityValid>)session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, idsOutOfRange)));

        session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, fittedIds)));

        Assert.Throws<ParametersLimitExceededException>(() => session.Query.All<ALotOfFieldsEntityValid>().Run());
        Assert.That(result.Any(), Is.False);
      }
    }

    [Test]
    public void DelayedSelectsOutOfLimitTest05()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        int[] fittedIds = Enumerable.Range(1, StorageLimit - 1).ToArray();
        int[] idsOutOfRange = Enumerable.Range(1, StorageLimit + 1).ToArray();

        var result1 = (DelayedSequence<ALotOfFieldsEntityValid>)session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, fittedIds)));

        var result2 = session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, fittedIds)));

        using (counter.Attach())
          Assert.Throws<ParametersLimitExceededException>(() => session.Query.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, idsOutOfRange)).Run());
        Assert.That(counter.Count, Is.EqualTo(1));
        Assert.That(result1.Any(), Is.True);
        Assert.That(result2.Any(), Is.False);
      }
    }

    [Test]
    public void InsertTest01()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        new ALotOfFieldsEntityValid();

        using (counter.Attach())
          Assert.DoesNotThrow(() => session.SaveChanges());
        Assert.That(counter.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void InsertTest02()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        new ALotOfFieldsEntityValid();
        new ALotOfFieldsEntityValid();
        new ALotOfFieldsEntityValid();
        new ALotOfFieldsEntityValid();

        using(counter.Attach())
          Assert.DoesNotThrow(() => session.SaveChanges());

        var expectedCommandCount = Math.Ceiling(Math.Ceiling(4 * Domain.Model.Types[typeof (ALotOfFieldsEntityValid)].Fields.Count / (decimal) StorageLimit));
        Assert.That(counter.Count, Is.EqualTo(expectedCommandCount));
      }
    }

    [Test]
    public void InsertTest03()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        new SeveralPersistActionsEntityValidA();
        new SeveralPersistActionsEntityValidA();
        new SeveralPersistActionsEntityValidA();

        using(counter.Attach())
          Assert.DoesNotThrow(() => session.SaveChanges());

        var expectedCommandCount = Math.Ceiling(Math.Ceiling(3 * Domain.Model.Types[typeof (SeveralPersistActionsEntityValidA)].Fields.Count / (decimal) StorageLimit));
        Assert.That(counter.Count, Is.EqualTo(expectedCommandCount));
      }
    }

    [Test]
    public void InsertTest04()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        new SeveralPersistActionsEntityValidB();
        new SeveralPersistActionsEntityValidB();
        new SeveralPersistActionsEntityValidB();

        using (counter.Attach())
          Assert.DoesNotThrow(() => session.SaveChanges());
        var expectedCommandCount = Math.Ceiling(Math.Ceiling(3 * Domain.Model.Types[typeof (SeveralPersistActionsEntityValidB)].Fields.Count / (decimal) StorageLimit));
        Assert.That(counter.Count, Is.EqualTo(expectedCommandCount));
      }
    }

    [Test]
    public void InsertTest05()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        new SeveralPersistActionsEntityValidC();
        new SeveralPersistActionsEntityValidC();
        new SeveralPersistActionsEntityValidC();


        using (counter.Attach())
          Assert.DoesNotThrow(() => session.SaveChanges());
        var expectedCommandCount = Math.Ceiling(Math.Ceiling(3 * Domain.Model.Types[typeof (SeveralPersistActionsEntityValidC)].Fields.Count / (decimal) StorageLimit));
        Assert.That(counter.Count, Is.EqualTo(expectedCommandCount));
      }
    }

    [Test]
    public void InsertTest06()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        new SeveralPersistActionsEntityInvalidA();
        new SeveralPersistActionsEntityInvalidA();
        new SeveralPersistActionsEntityInvalidA();

        using (counter.Attach())
          Assert.DoesNotThrow(() => session.SaveChanges());
        var expectedCommandCount = Math.Ceiling(Math.Ceiling(3 * Domain.Model.Types[typeof (SeveralPersistActionsEntityInvalidA)].Fields.Count / (decimal) StorageLimit));
        Assert.That(counter.Count, Is.EqualTo(expectedCommandCount));
      }
    }

    [Test]
    public void InsertTest07()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        new SeveralPersistActionsEntityInvalidB();
        new SeveralPersistActionsEntityInvalidB();
        new SeveralPersistActionsEntityInvalidB();

        using (counter.Attach())
          Assert.DoesNotThrow(() => session.SaveChanges());
        var expectedCommandCount = Math.Ceiling(Math.Ceiling(3 * Domain.Model.Types[typeof (SeveralPersistActionsEntityInvalidB)].Fields.Count / (decimal) StorageLimit));
        Assert.That(counter.Count, Is.EqualTo(expectedCommandCount));
      }
    }

    [Test]
    public void InsertTest08()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        new SeveralPersistActionsEntityInvalidC();
        new SeveralPersistActionsEntityInvalidC();
        new SeveralPersistActionsEntityInvalidC();

        using(counter.Attach())
          Assert.Throws<ParametersLimitExceededException>(() => session.SaveChanges());
        Assert.That(counter.Count, Is.EqualTo(0));
      }
    }

    [Test]
    public void InsertTest09()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        var batchSize = session.Configuration.BatchSize;
        var currentBatchCapacity = batchSize * 3;

        Console.WriteLine(batchSize);
        // three complete batch;
        while (currentBatchCapacity > 0) {
          new NormalAmountOfFieldsEntity();
          currentBatchCapacity--;
        }

        using (counter.Attach())
          Assert.That(session.Query.All<NormalAmountOfFieldsEntity>().Count(),Is.EqualTo(batchSize * 3));

        Assert.That(counter.Count, Is.EqualTo(4));
      }
    }

    [Test]
    public void UpdateVersionizedTest01()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        var overallCount = 0;
        session.Query.All<ALotOfFieldsEntityVersionized>().ForEach(e => {
          overallCount++;
          e.UpdateGeneratedFields();
        });

        new NormalAmountOfFieldsEntity();

        using (counter.Attach())
          Assert.DoesNotThrow(() => session.SaveChanges());
        Assert.That(counter.Count, Is.EqualTo(overallCount/2));
      }
    }

    [Test]
    public void UpdateVersionizedTest02()
    {
      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.Default | SessionOptions.AutoActivation | SessionOptions.ValidateEntityVersions)))
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        var overallCount = 0;
        session.Query.All<ALotOfFieldsEntityVersionized>().ForEach(e => {
          overallCount++;
          e.UpdateGeneratedFields();
        });

        new NormalAmountOfFieldsEntity();
        new NormalAmountOfFieldsEntity();
        new NormalAmountOfFieldsEntity();
        new NormalAmountOfFieldsEntity();

        using (counter.Attach())
          Assert.DoesNotThrow(() => session.SaveChanges());
        Assert.That(counter.Count, Is.EqualTo(overallCount + 1));
      }
    }

    [Test]
    public void UpdateRegularTest01()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        var overallCount = 0;
        session.Query.All<ALotOfFieldsEntityValid>().ForEach(e => {
          overallCount++;
          e.UpdateGeneratedFields();
        });

        new NormalAmountOfFieldsEntity();

        using (counter.Attach())
          Assert.DoesNotThrow(() => session.SaveChanges());
        Assert.That(counter.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void PartitialExecutionAllowedTest01()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        new ALotOfFieldsEntityValid();
        new ALotOfFieldsEntityValid();
        new ALotOfFieldsEntityValid();

        //persist by query causes allowPartialExecution = true;
        using (counter)
          session.Persist(PersistReason.Query);
        Assert.That(counter.Count, Is.EqualTo(0));

        counter.Reset();
        using (counter.Attach())
          Assert.That(session.Query.All<ALotOfFieldsEntityValid>().Count(), Is.EqualTo(5));
        Assert.That(counter.Count, Is.EqualTo(2));
      }
    }

    [Test]
    public void PartitialExecutionAllowedTest02()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        var batchSize = session.Configuration.BatchSize;
        var currentBatchCapacity = batchSize;

        Console.WriteLine(batchSize);
        // one complete batch;
        while (currentBatchCapacity > 0) {
          new NormalAmountOfFieldsEntity();
          currentBatchCapacity--;
        }

        // extra task to have extra batch
        new NormalAmountOfFieldsEntity();

        //persist by query causes allowPartialExecution = true;
        using (counter.Attach())
          session.Persist(PersistReason.Query);
        Assert.That(counter.Count, Is.EqualTo(1));

        counter.Reset();
        using (counter.Attach())
          Assert.That(session.Query.All<NormalAmountOfFieldsEntity>().Count(), Is.EqualTo(26));
        Assert.That(counter.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void PartialExecutionDeniedTest01()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        new ALotOfFieldsEntityValid();
        new ALotOfFieldsEntityValid();
        new ALotOfFieldsEntityValid();

        //manual persist causes allowPartialExecution = false;
        using (counter.Attach())
          session.Persist(PersistReason.Manual);
        Assert.That(counter.Count, Is.EqualTo(2));
      }
    }

    [Test]
    public void PartialExecutionDeniedTest02()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        var batchSize = session.Configuration.BatchSize;
        var currentBatchCapacity = batchSize;

        Console.WriteLine(batchSize);
        // one complete batch;
        while (currentBatchCapacity > 0) {
          new NormalAmountOfFieldsEntity();
          currentBatchCapacity--;
        }

        // extra task to have extra batch
        new NormalAmountOfFieldsEntity();

        //persist by query causes allowPartialExecution = true;
        using (counter.Attach())
          session.Persist(PersistReason.Manual);
        Assert.That(counter.Count, Is.EqualTo(2));
      }
    }
  }
}