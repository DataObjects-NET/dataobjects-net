// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2019.04.14

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Tests.Storage.CommandProcessing
{
  public class SimpleCommandProcessorParametersManagement : AutoBuildTest
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
      Require.AllFeaturesNotSupported(ProviderFeatures.Batches);
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new ALotOfFieldsEntityValid();
        new ALotOfFieldsEntityVersionized();
        transaction.Complete();
      }
    }

    [Test]
    public void DelayedSelectsWithinLimitTest()
    {
      RequireLimit();

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        int[] ids = Enumerable.Range(1, StorageLimit - 1).ToArray();

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

        var inlineQuery = session.Query.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids));
        Assert.That(inlineQuery.Any(), Is.True);

        foreach (var result in results)
          Assert.That(result.Any(), Is.True);
      }
    }

    [Test]
    public void DelayedSelectsOutOfLimitTest01()
    {
      RequireLimit();

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        int[] fittedIds = Enumerable.Range(1, StorageLimit - 1).ToArray();
        int[] idsOutOfRange = Enumerable.Range(1, StorageLimit + 1).ToArray();

        var result = (DelayedSequence<ALotOfFieldsEntityValid>) session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, idsOutOfRange)));
        Assert.Throws<ParametersLimitExceededException>(() => result.Run());
        Assert.That(result.Task.Result.Count, Is.EqualTo(0));
      }
    }

    [Test]
    public void DelayedSelectsOutOfLimitTest02()
    {
      RequireLimit();

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        int[] fittedIds = Enumerable.Range(1, StorageLimit - 1).ToArray();
        int[] idsOutOfRange = Enumerable.Range(1, StorageLimit + 1).ToArray();

        var result = (DelayedSequence<ALotOfFieldsEntityValid>)session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, idsOutOfRange)));

        Assert.Throws<ParametersLimitExceededException>(() => session.Query.All<ALotOfFieldsEntityValid>().Run());
        Assert.That(result.Task.Result.Count, Is.EqualTo(0));
      }
    }

    [Test]
    public void DelayedSelectsOutOfLimitTest03()
    {
      RequireLimit();

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        int[] fittedIds = Enumerable.Range(1, StorageLimit - 1).ToArray();
        int[] idsOutOfRange = Enumerable.Range(1, StorageLimit + 1).ToArray();

        var result = (DelayedSequence<ALotOfFieldsEntityValid>)session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, fittedIds)));

        session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, idsOutOfRange)));

        Assert.Throws<ParametersLimitExceededException>(() => session.Query.All<ALotOfFieldsEntityValid>().Run());
        Assert.That(result.Any(), Is.True);
      }
    }

    [Test]
    public void DelayedSelectsOutOfLimitTest04()
    {
      RequireLimit();

      using (var session = Domain.OpenSession())
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
      RequireLimit();

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        int[] fittedIds = Enumerable.Range(1, StorageLimit - 1).ToArray();
        int[] idsOutOfRange = Enumerable.Range(1, StorageLimit + 1).ToArray();

        var result1 = (DelayedSequence<ALotOfFieldsEntityValid>)session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, fittedIds)));

        var result2 = session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, fittedIds)));

        Assert.Throws<ParametersLimitExceededException>(() => session.Query.All<ALotOfFieldsEntityValid>().Where(e=>e.Id.In(IncludeAlgorithm.ComplexCondition, idsOutOfRange)).Run());
        Assert.That(result1.Any(), Is.True);
        Assert.That(result2.Any(), Is.True);
      }
    }

    [Test]
    public void InsertTest01()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new ALotOfFieldsEntityValid();
        Assert.DoesNotThrow(() => session.SaveChanges());
      }
    }

    [Test]
    public void InsertTest02()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new ALotOfFieldsEntityValid();
        new ALotOfFieldsEntityValid();
        new ALotOfFieldsEntityValid();
        new ALotOfFieldsEntityValid();

        Assert.DoesNotThrow(() => session.SaveChanges());
      }
    }

    [Test]
    public void InsertTest03()
    {
      RequireLimit();

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new ALotOfFieldsEntityInvalid();
        Assert.Throws<ParametersLimitExceededException>(() => session.SaveChanges());
      }
    }

    [Test]
    public void InsertTest04()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new SeveralPersistActionsEntityValidA();
        new SeveralPersistActionsEntityValidA();
        new SeveralPersistActionsEntityValidA();

        Assert.DoesNotThrow(() => session.SaveChanges());
      }
    }

    [Test]
    public void InsertTest05() 
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new SeveralPersistActionsEntityValidB();
        new SeveralPersistActionsEntityValidB();
        new SeveralPersistActionsEntityValidB();

        Assert.DoesNotThrow(() => session.SaveChanges());
      }
    }

    [Test]
    public void InsertTest06()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new SeveralPersistActionsEntityValidC();
        new SeveralPersistActionsEntityValidC();
        new SeveralPersistActionsEntityValidC();

        Assert.DoesNotThrow(() => session.SaveChanges());
      }
    }

    [Test]
    public void InsertTest07()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new SeveralPersistActionsEntityInvalidA();
        new SeveralPersistActionsEntityInvalidA();
        new SeveralPersistActionsEntityInvalidA();

        Assert.DoesNotThrow(() => session.SaveChanges());
      }
    }

    [Test]
    public void InsertTest08()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new SeveralPersistActionsEntityInvalidB();
        new SeveralPersistActionsEntityInvalidB();
        new SeveralPersistActionsEntityInvalidB();

        Assert.DoesNotThrow(() => session.SaveChanges());
      }
    }

    [Test]
    public void InsertTest09()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new SeveralPersistActionsEntityInvalidC();
        new SeveralPersistActionsEntityInvalidC();
        new SeveralPersistActionsEntityInvalidC();

        Assert.DoesNotThrow(() => session.SaveChanges());
      }
    }

    [Test]
    public void UpdateVersionizedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<ALotOfFieldsEntityVersionized>().ForEach(e=> e.UpdateGeneratedFields());

        Assert.DoesNotThrow(() => session.SaveChanges());
      }
    }

    [Test]
    public void UpdateRegularTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<ALotOfFieldsEntityValid>().ForEach(e => e.UpdateGeneratedFields());

        Assert.DoesNotThrow(() => session.SaveChanges());
      }
    }

    private void RequireLimit()
    {
      if (StorageLimit==int.MaxValue)
        throw new IgnoreException("This test requires storage with limit of parameters");
    }
  }
}