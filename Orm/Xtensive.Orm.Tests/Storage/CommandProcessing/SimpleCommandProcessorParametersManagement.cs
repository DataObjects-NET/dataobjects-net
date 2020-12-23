// Copyright (C) 2019-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2019.04.14

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Tests.Storage.CommandProcessing
{
  public class SimpleCommandProcessorParametersManagement : AutoBuildTest
  {
    public int StorageLimit => ProviderInfo.MaxQueryParameterCount;

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(ALotOfFieldsEntityValid).Assembly, typeof(ALotOfFieldsEntityValid).Namespace);
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
    }

    protected override void CheckRequirements() => Require.AllFeaturesNotSupported(ProviderFeatures.Batches);

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new ALotOfFieldsEntityValid();
        _ = new ALotOfFieldsEntityVersionized();
        transaction.Complete();
      }
    }

    [Test]
    public void DelayedSelectsWithinLimitTest()
    {
      RequireLimit();

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var ids = Enumerable.Range(1, StorageLimit - 1).ToArray();
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

        var inlineQuery = session.Query.All<ALotOfFieldsEntityValid>()
          .Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids));
        Assert.That(inlineQuery.Any(), Is.True);

        foreach (var result in results) {
          Assert.That(result.Any(), Is.True);
        }
      }
    }

    [Test]
    public async Task DelayedSelectsWithinLimitAsyncTest()
    {
      RequireLimit();

      using (var session = await Domain.OpenSessionAsync())
      using (var transaction = session.OpenTransaction()) {
        var ids = Enumerable.Range(1, StorageLimit - 1).ToArray(StorageLimit - 1);
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

        var inlineQuery = await session.Query.All<ALotOfFieldsEntityValid>()
          .Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids)).AsAsync();
        Assert.That(inlineQuery.Any(), Is.True);

        foreach (var result in results) {
          Assert.That(result.Any(), Is.True);
        }
      }
    }

    [Test]
    public void DelayedSelectsOutOfLimitTest01()
    {
      RequireLimit();

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var fittedIds = Enumerable.Range(1, StorageLimit - 1).ToArray(StorageLimit - 1);
        var idsOutOfRange = Enumerable.Range(1, StorageLimit + 1).ToArray(StorageLimit + 1);

        var result = (DelayedSequence<ALotOfFieldsEntityValid>) session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, idsOutOfRange)));

        _ = Assert.Throws<ParametersLimitExceededException>(() => result.Run());
        Assert.That(result.Task.Result.Count, Is.EqualTo(0));
      }
    }

    [Test]
    public async Task DelayedSelectsOutOfLimitAsyncTest01()
    {
      RequireLimit();

      using (var session = await Domain.OpenSessionAsync())
      using (var transaction = session.OpenTransaction()) {
        var fittedIds = Enumerable.Range(1, StorageLimit - 1).ToArray(StorageLimit - 1);
        var idsOutOfRange = Enumerable.Range(1, StorageLimit + 1).ToArray(StorageLimit + 1);

        var result = (DelayedSequence<ALotOfFieldsEntityValid>) session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, idsOutOfRange)));

        _ = Assert.ThrowsAsync<ParametersLimitExceededException>(async () => await result.AsAsync());
        Assert.That(result.Task.Result.Count, Is.EqualTo(0));
      }
    }

    [Test]
    public void DelayedSelectsOutOfLimitTest02()
    {
      RequireLimit();

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var fittedIds = Enumerable.Range(1, StorageLimit - 1).ToArray(StorageLimit - 1);
        var idsOutOfRange = Enumerable.Range(1, StorageLimit + 1).ToArray(StorageLimit + 1);

        var result = (DelayedSequence<ALotOfFieldsEntityValid>) session.Query.ExecuteDelayed(q =>
           q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, idsOutOfRange)));

        _ = Assert.Throws<ParametersLimitExceededException>(() => session.Query.All<ALotOfFieldsEntityValid>().Run());
        Assert.That(result.Task.Result.Count, Is.EqualTo(0));
      }
    }

    [Test]
    public async Task DelayedSelectsOutOfLimitAsyncTest02()
    {
      RequireLimit();

      using (var session = await Domain.OpenSessionAsync())
      using (var transaction = session.OpenTransaction()) {
        var fittedIds = Enumerable.Range(1, StorageLimit - 1).ToArray(StorageLimit - 1);
        var idsOutOfRange = Enumerable.Range(1, StorageLimit + 1).ToArray(StorageLimit + 1);

        var result = (DelayedSequence<ALotOfFieldsEntityValid>) session.Query.ExecuteDelayed(q =>
           q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, idsOutOfRange)));

        _ = Assert.ThrowsAsync<ParametersLimitExceededException>(async () => (await session.Query.All<ALotOfFieldsEntityValid>().AsAsync()).Run());
        Assert.That(result.Task.Result.Count, Is.EqualTo(0));
      }
    }

    [Test]
    public void DelayedSelectsOutOfLimitTest03()
    {
      RequireLimit();

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var fittedIds = Enumerable.Range(1, StorageLimit - 1).ToArray(StorageLimit - 1);
        var idsOutOfRange = Enumerable.Range(1, StorageLimit + 1).ToArray(StorageLimit + 1);

        var result = (DelayedSequence<ALotOfFieldsEntityValid>) session.Query.ExecuteDelayed(q =>
           q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, fittedIds)));

        _ = session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, idsOutOfRange)));

        _ = Assert.Throws<ParametersLimitExceededException>(() => session.Query.All<ALotOfFieldsEntityValid>().Run());
        Assert.That(result.Any(), Is.True);
      }
    }

    [Test]
    public async Task DelayedSelectsOutOfLimitAsyncTest03()
    {
      RequireLimit();

      using (var session = await Domain.OpenSessionAsync())
      using (var transaction = session.OpenTransaction()) {
        var fittedIds = Enumerable.Range(1, StorageLimit - 1).ToArray(StorageLimit - 1);
        var idsOutOfRange = Enumerable.Range(1, StorageLimit + 1).ToArray(StorageLimit + 1);

        var result = (DelayedSequence<ALotOfFieldsEntityValid>) session.Query.ExecuteDelayed(q =>
           q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, fittedIds)));

        _ = session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, idsOutOfRange)));

        _ = Assert.ThrowsAsync<ParametersLimitExceededException>(async () => (await session.Query.All<ALotOfFieldsEntityValid>().AsAsync()).Run());
        Assert.That(result.Any(), Is.True);
      }
    }

    [Test]
    public void DelayedSelectsOutOfLimitTest04()
    {
      RequireLimit();

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var fittedIds = Enumerable.Range(1, StorageLimit - 1).ToArray(StorageLimit - 1);
        var idsOutOfRange = Enumerable.Range(1, StorageLimit + 1).ToArray(StorageLimit + 1);

        var result = (DelayedSequence<ALotOfFieldsEntityValid>) session.Query.ExecuteDelayed(q =>
           q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, idsOutOfRange)));

        _ = session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, fittedIds)));

        _ = Assert.Throws<ParametersLimitExceededException>(() => session.Query.All<ALotOfFieldsEntityValid>().Run());
        Assert.That(result.Any(), Is.False);
      }
    }

    [Test]
    public async Task DelayedSelectsOutOfLimitAsyncTest04()
    {
      RequireLimit();

      using (var session = await Domain.OpenSessionAsync())
      using (var transaction = session.OpenTransaction()) {
        var fittedIds = Enumerable.Range(1, StorageLimit - 1).ToArray(StorageLimit - 1);
        var idsOutOfRange = Enumerable.Range(1, StorageLimit + 1).ToArray(StorageLimit + 1);

        var result = (DelayedSequence<ALotOfFieldsEntityValid>) session.Query.ExecuteDelayed(q =>
           q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, idsOutOfRange)));

        _ = session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, fittedIds)));

        _ = Assert.ThrowsAsync<ParametersLimitExceededException>(async () => (await session.Query.All<ALotOfFieldsEntityValid>().AsAsync()).Run());
        Assert.That(result.Any(), Is.False);
      }
    }

    [Test]
    public void DelayedSelectsOutOfLimitTest05()
    {
      RequireLimit();

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var fittedIds = Enumerable.Range(1, StorageLimit - 1).ToArray(StorageLimit - 1);
        var idsOutOfRange = Enumerable.Range(1, StorageLimit + 1).ToArray(StorageLimit + 1);

        var result1 = (DelayedSequence<ALotOfFieldsEntityValid>) session.Query.ExecuteDelayed(q =>
           q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, fittedIds)));

        var result2 = session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, fittedIds)));

        _ = Assert.Throws<ParametersLimitExceededException>(
          () => session.Query.All<ALotOfFieldsEntityValid>()
            .Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, idsOutOfRange)).Run());
        Assert.That(result1.Any(), Is.True);
        Assert.That(result2.Any(), Is.True);
      }
    }

    [Test]
    public async Task DelayedSelectsOutOfLimitAsyncTest05()
    {
      RequireLimit();

      using (var session = await Domain.OpenSessionAsync())
      using (var transaction = session.OpenTransaction()) {
        var fittedIds = Enumerable.Range(1, StorageLimit - 1).ToArray(StorageLimit - 1);
        var idsOutOfRange = Enumerable.Range(1, StorageLimit + 1).ToArray(StorageLimit + 1);

        var result1 = (DelayedSequence<ALotOfFieldsEntityValid>) session.Query.ExecuteDelayed(q =>
           q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, fittedIds)));

        var result2 = session.Query.ExecuteDelayed(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, fittedIds)));

        _ = Assert.ThrowsAsync<ParametersLimitExceededException>(
          async () => (await session.Query.All<ALotOfFieldsEntityValid>()
            .Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, idsOutOfRange)).AsAsync()).Run());
        Assert.That(result1.Any(), Is.True);
        Assert.That(result2.Any(), Is.True);
      }
    }

    [Test]
    public void InsertTest01()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new ALotOfFieldsEntityValid();
        Assert.DoesNotThrow(() => session.SaveChanges());
      }
    }

    [Test]
    public void InsertTest02()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new ALotOfFieldsEntityValid();
        _ = new ALotOfFieldsEntityValid();
        _ = new ALotOfFieldsEntityValid();
        _ = new ALotOfFieldsEntityValid();

        Assert.DoesNotThrow(() => session.SaveChanges());
      }
    }

    [Test]
    public void InsertTest03()
    {
      RequireLimit();

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new ALotOfFieldsEntityInvalid();
        _ = Assert.Throws<ParametersLimitExceededException>(() => session.SaveChanges());
      }
    }

    [Test]
    public void InsertTest04()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new SeveralPersistActionsEntityValidA();
        _ = new SeveralPersistActionsEntityValidA();
        _ = new SeveralPersistActionsEntityValidA();

        Assert.DoesNotThrow(() => session.SaveChanges());
      }
    }

    [Test]
    public void InsertTest05()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new SeveralPersistActionsEntityValidB();
        _ = new SeveralPersistActionsEntityValidB();
        _ = new SeveralPersistActionsEntityValidB();

        Assert.DoesNotThrow(() => session.SaveChanges());
      }
    }

    [Test]
    public void InsertTest06()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new SeveralPersistActionsEntityValidC();
        _ = new SeveralPersistActionsEntityValidC();
        _ = new SeveralPersistActionsEntityValidC();

        Assert.DoesNotThrow(() => session.SaveChanges());
      }
    }

    [Test]
    public void InsertTest07()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new SeveralPersistActionsEntityInvalidA();
        _ = new SeveralPersistActionsEntityInvalidA();
        _ = new SeveralPersistActionsEntityInvalidA();

        Assert.DoesNotThrow(() => session.SaveChanges());
      }
    }

    [Test]
    public void InsertTest08()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new SeveralPersistActionsEntityInvalidB();
        _ = new SeveralPersistActionsEntityInvalidB();
        _ = new SeveralPersistActionsEntityInvalidB();

        Assert.DoesNotThrow(() => session.SaveChanges());
      }
    }

    [Test]
    public void InsertTest09()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new SeveralPersistActionsEntityInvalidC();
        _ = new SeveralPersistActionsEntityInvalidC();
        _ = new SeveralPersistActionsEntityInvalidC();

        Assert.DoesNotThrow(() => session.SaveChanges());
      }
    }

    [Test]
    public void UpdateVersionizedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<ALotOfFieldsEntityVersionized>().ForEach(e => e.UpdateGeneratedFields());

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
      if (StorageLimit == int.MaxValue) {
        throw new IgnoreException("This test requires storage with limit of parameters");
      }
    }
  }
}