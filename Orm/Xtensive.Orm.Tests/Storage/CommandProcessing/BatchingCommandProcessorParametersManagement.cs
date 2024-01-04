// Copyright (C) 2019-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
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
  public class BatchingCommandProcessorParametersManagement : AutoBuildTest
  {
    private int maxQueryParameterCount;
    private int[] parametersFitInSingleCommand;
    private int[] parametersDontFitInSingleCommand;

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(ALotOfFieldsEntityValid).Assembly, typeof(ALotOfFieldsEntityValid).Namespace);
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
    }

    protected override void CheckRequirements() => Require.ProviderIs(StorageProvider.SqlServer);

    protected override void PopulateData()
    {
      var sessionConfig = SessionConfiguration.Default.Clone();
      sessionConfig.Options |= SessionOptions.AutoActivation;
      sessionConfig.BatchSize = 1;

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new ALotOfFieldsEntityValid();
        _ = new ALotOfFieldsEntityValid();
        _ = new ALotOfFieldsEntityVersionized();
        _ = new ALotOfFieldsEntityVersionized();
        transaction.Complete();
      }

      maxQueryParameterCount = StorageProviderInfo.Instance.Info.MaxQueryParameterCount;

      parametersFitInSingleCommand = Enumerable.Range(1, maxQueryParameterCount - 1).ToArray(maxQueryParameterCount - 1);
      parametersDontFitInSingleCommand = Enumerable.Range(1, maxQueryParameterCount + 1).ToArray(maxQueryParameterCount + 1);
    }

    [Test]
    public void EachQueryInSeparateCommandTest()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        var results = new List<IEnumerable<ALotOfFieldsEntityValid>>(10);

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersFitInSingleCommand))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersFitInSingleCommand))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersFitInSingleCommand))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersFitInSingleCommand))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersFitInSingleCommand))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersFitInSingleCommand))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersFitInSingleCommand))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersFitInSingleCommand))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersFitInSingleCommand))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersFitInSingleCommand))));

        var inlineQuery = session.Query.All<ALotOfFieldsEntityValid>()
          .Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersFitInSingleCommand));

        using (counter.Attach()) {
          Assert.That(inlineQuery.Any(), Is.True);
          Assert.That(counter.Count, Is.EqualTo(11));
        }

        foreach (var result in results) {
          Assert.That(result.Any(), Is.True);
        }
      }
    }

    [Test]
    public async Task EachQueryInSeparateCommandAsyncTest()
    {
      using (var session = await Domain.OpenSessionAsync())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        var results = new List<IEnumerable<ALotOfFieldsEntityValid>>(10);

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersFitInSingleCommand))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersFitInSingleCommand))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersFitInSingleCommand))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersFitInSingleCommand))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersFitInSingleCommand))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersFitInSingleCommand))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersFitInSingleCommand))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersFitInSingleCommand))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersFitInSingleCommand))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersFitInSingleCommand))));

        using (counter.Attach()) {
          var inlineQuery = await session.Query.All<ALotOfFieldsEntityValid>()
          .Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersFitInSingleCommand)).ExecuteAsync();
          Assert.That(inlineQuery.Any(), Is.True);
          Assert.That(counter.Count, Is.EqualTo(11));
        }

        foreach (var result in results) {
          Assert.That(result.Any(), Is.True);
        }
      }
    }

    [Test]
    public void InlineQueryInSeparateBatchTest()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        var parametersCountPerQuery = (maxQueryParameterCount - 1) / 2;
        var ids = Enumerable.Range(1, parametersCountPerQuery).ToArray(parametersCountPerQuery);
        var results = new List<IEnumerable<ALotOfFieldsEntityValid>>(10);

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        var inlineQuery = session.Query.All<ALotOfFieldsEntityValid>()
          .Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, new[] { 1, 2, 3, 4 }));

        using (counter.Attach()) {
          Assert.That(inlineQuery.Any(), Is.True);
          Assert.That(counter.Count, Is.EqualTo(6));
        }

        foreach (var result in results) {
          Assert.That(result.Any(), Is.True);
        }
      }
    }

    [Test]
    public async Task InlineQueryInSeparateBatchAsyncTest()
    {
      using (var session = await Domain.OpenSessionAsync())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        var parametersCountPerQuery = (maxQueryParameterCount - 1) / 2;
        var ids = Enumerable.Range(1, parametersCountPerQuery).ToArray(parametersCountPerQuery);
        var results = new List<IEnumerable<ALotOfFieldsEntityValid>>(10);

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        using (counter.Attach()) {
          var inlineQuery = await session.Query.All<ALotOfFieldsEntityValid>()
            .Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, new[] { 1, 2, 3, 4 })).ExecuteAsync();
          Assert.That(inlineQuery.Any(), Is.True);
          Assert.That(counter.Count, Is.EqualTo(6));
        }

        foreach (var result in results) {
          Assert.That(result.Any(), Is.True);
        }
      }
    }

    [Test]
    public void InlineQueryInLastBatchTest()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        var parametersCountPerQuery = ((maxQueryParameterCount - 3) / 2) - 1;
        var ids = Enumerable.Range(1, parametersCountPerQuery).ToArray(parametersCountPerQuery);
        var results = new List<IEnumerable<ALotOfFieldsEntityValid>>(10);

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        var inlineQuery = session.Query.All<ALotOfFieldsEntityValid>()
          .Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, new[] { 1, 2 }));

        using (counter.Attach()) {
          Assert.That(inlineQuery.Any(), Is.True);
          Assert.That(counter.Count, Is.EqualTo(5));
        }

        foreach (var result in results) {
          Assert.That(result.Any(), Is.True);
        }
      }
    }

    [Test]
    public async Task InlineQueryInLastBatchAsyncTest()
    {
      using (var session = await Domain.OpenSessionAsync())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        var parametersCountPerQuery = ((maxQueryParameterCount - 3) / 2) - 1;
        var ids = Enumerable.Range(1, parametersCountPerQuery).ToArray(parametersCountPerQuery);
        var results = new List<IEnumerable<ALotOfFieldsEntityValid>>(10);

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        results.Add(session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, ids))));

        using (counter.Attach()) {
          var inlineQuery = await session.Query.All<ALotOfFieldsEntityValid>()
          .Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, new[] { 1, 2 })).ExecuteAsync();
          Assert.That(inlineQuery.Any(), Is.True);
          Assert.That(counter.Count, Is.EqualTo(5));
        }

        foreach (var result in results) {
          Assert.That(result.Any(), Is.True);
        }
      }
    }

    [Test]
    public void DelayedSelectsOutOfLimitTest01()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {

        var result = (DelayedQuery<ALotOfFieldsEntityValid>) session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersDontFitInSingleCommand)));

        using (counter.Attach()) {
          _ = Assert.Throws<ParametersLimitExceededException>(() => result.Run());
        }
        Assert.That(result.Task.Result.Count, Is.EqualTo(0));
        Assert.That(counter.Count, Is.EqualTo(0));
      }
    }

    [Test]
    public async Task DelayedSelectsOutOfLimitAsyncTest01()
    {
      using (var session = await Domain.OpenSessionAsync())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {

        var result = (DelayedQuery<ALotOfFieldsEntityValid>) session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersDontFitInSingleCommand)));

        using (counter.Attach()) {
          _ = Assert.ThrowsAsync<ParametersLimitExceededException>(async () => (await result.ExecuteAsync()).Run());
        }
        Assert.That(result.Task.Result.Count, Is.EqualTo(0));
        Assert.That(counter.Count, Is.EqualTo(0));
        var currentSession = Session.Current;
        Assert.That(currentSession, Is.EqualTo(session));
      }
    }

    [Test]
    public void DelayedSelectsOutOfLimitTest02()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {

        var result = (DelayedQuery<ALotOfFieldsEntityValid>) session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersDontFitInSingleCommand)));

        using (counter.Attach()) {
          _ = Assert.Throws<ParametersLimitExceededException>(() => session.Query.All<ALotOfFieldsEntityValid>().Run());
        }

        Assert.That(result.Task.Result.Count, Is.EqualTo(0));
        Assert.That(counter.Count, Is.EqualTo(0));
      }
    }

    [Test]
    public async Task DelayedSelectsOutOfLimitAsyncTest02()
    {
      using (var session = await Domain.OpenSessionAsync())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {

        var result = (DelayedQuery<ALotOfFieldsEntityValid>) session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersDontFitInSingleCommand)));

        using (counter.Attach()) {
          _ = Assert.ThrowsAsync<ParametersLimitExceededException>(
            async () => (await session.Query.All<ALotOfFieldsEntityValid>().ExecuteAsync()).Run());
        }

        Assert.That(result.Task.Result.Count, Is.EqualTo(0));
        Assert.That(counter.Count, Is.EqualTo(0));
      }
    }

    [Test]
    public void DelayedSelectsOutOfLimitTest03()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {

        var result = (DelayedQuery<ALotOfFieldsEntityValid>) session.Query.CreateDelayedQuery(q =>
           q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersFitInSingleCommand)));

        _ = session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersFitInSingleCommand)));

        _ = session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersDontFitInSingleCommand)));

        using (counter.Attach()) {
          _ = Assert.Throws<ParametersLimitExceededException>(() => session.Query.All<ALotOfFieldsEntityValid>().Run());
        }
        Assert.That(counter.Count, Is.EqualTo(1));
        Assert.That(result.Any(), Is.True);
      }
    }

    [Test]
    public async Task DelayedSelectsOutOfLimitAsyncTest03()
    {
      using (var session = await Domain.OpenSessionAsync())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {

        var result = (DelayedQuery<ALotOfFieldsEntityValid>) session.Query.CreateDelayedQuery(q =>
           q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersFitInSingleCommand)));

        _ = session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersFitInSingleCommand)));

        _ = session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersDontFitInSingleCommand)));

        using (counter.Attach()) {
          _ = Assert.ThrowsAsync<ParametersLimitExceededException>(
            async () => (await session.Query.All<ALotOfFieldsEntityValid>().ExecuteAsync()).Run());
        }
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

        var result = (DelayedQuery<ALotOfFieldsEntityValid>) session.Query.CreateDelayedQuery(q =>
           q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersDontFitInSingleCommand)));

        _ = session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersFitInSingleCommand)));

        using (counter.Attach()) {
          _ = Assert.Throws<ParametersLimitExceededException>(() => session.Query.All<ALotOfFieldsEntityValid>().Run());
        }
        Assert.That(counter.Count, Is.EqualTo(0));
        Assert.That(result.Any(), Is.False);
      }
    }

    [Test]
    public async Task DelayedSelectsOutOfLimitAsyncTest04()
    {
      using (var session = await Domain.OpenSessionAsync())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {

        var result = (DelayedQuery<ALotOfFieldsEntityValid>) session.Query.CreateDelayedQuery(q =>
           q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersDontFitInSingleCommand)));

        _ = session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersFitInSingleCommand)));

        _ = Assert.ThrowsAsync<ParametersLimitExceededException>(
          async () => (await session.Query.All<ALotOfFieldsEntityValid>().ExecuteAsync()).Run());
        Assert.That(result.Any(), Is.False);
      }
    }

    [Test]
    public void DelayedSelectsOutOfLimitTest05()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {

        var result1 = (DelayedQuery<ALotOfFieldsEntityValid>) session.Query.CreateDelayedQuery(q =>
           q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersFitInSingleCommand)));

        var result2 = session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersFitInSingleCommand)));

        using (counter.Attach()) {
          _ = Assert.Throws<ParametersLimitExceededException>(
            () => session.Query.All<ALotOfFieldsEntityValid>()
             .Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersDontFitInSingleCommand)).Run());
        }
        Assert.That(counter.Count, Is.EqualTo(1));
        Assert.That(result1.Any(), Is.True);
        Assert.That(result2.Any(), Is.False);
      }
    }

    [Test]
    public async Task DelayedSelectsOutOfLimitAsyncTest05()
    {
      using (var session = await Domain.OpenSessionAsync())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {

        var result1 = session.Query.CreateDelayedQuery(q =>
           q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersFitInSingleCommand)));

        var result2 = session.Query.CreateDelayedQuery(q =>
          q.All<ALotOfFieldsEntityValid>().Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersFitInSingleCommand)));

        using (counter.Attach()) {
          _ = Assert.ThrowsAsync<ParametersLimitExceededException>(
            async () => (await session.Query.All<ALotOfFieldsEntityValid>()
             .Where(e => e.Id.In(IncludeAlgorithm.ComplexCondition, parametersDontFitInSingleCommand)).ExecuteAsync()).Run());
        }
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
        var before = session.Query.All<ALotOfFieldsEntityValid>().Count();

        _ = new ALotOfFieldsEntityValid();

        using (counter.Attach()) {
          Assert.DoesNotThrow(() => session.SaveChanges());
        }

        Assert.That(counter.Count, Is.EqualTo(1));
        Assert.That(session.Query.All<ALotOfFieldsEntityValid>().Count(), Is.EqualTo(before + 1));
      }
    }

    [Test]
    public void InsertTest02()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        var before = session.Query.All<ALotOfFieldsEntityValid>().Count();

        _ = new ALotOfFieldsEntityValid();
        _ = new ALotOfFieldsEntityValid();
        _ = new ALotOfFieldsEntityValid();
        _ = new ALotOfFieldsEntityValid();

        using (counter.Attach()) {
          Assert.DoesNotThrow(() => session.SaveChanges());
        }

        var type = typeof(ALotOfFieldsEntityValid);
        var expectedCommandCount = Math.Ceiling(
          Math.Ceiling(4 * Domain.Model.Types[type].Fields.Count / (decimal) maxQueryParameterCount));
        Assert.That(counter.Count, Is.EqualTo(expectedCommandCount));

        Assert.That(session.Query.All<ALotOfFieldsEntityValid>().Count(), Is.EqualTo(before + 4));
      }
    }

    [Test]
    public void InsertTest03()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        _ = new SeveralPersistActionsEntityValidA();
        _ = new SeveralPersistActionsEntityValidA();
        _ = new SeveralPersistActionsEntityValidA();

        using (counter.Attach()) {
          Assert.DoesNotThrow(() => session.SaveChanges());
        }

        var type = typeof(SeveralPersistActionsEntityValidA);
        var expectedCommandCount = Math.Ceiling(Math.Ceiling(3 * Domain.Model.Types[type].Fields.Count / (decimal) maxQueryParameterCount));
        Assert.That(counter.Count, Is.EqualTo(expectedCommandCount));
      }
    }

    [Test]
    public void InsertTest04()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        _ = new SeveralPersistActionsEntityValidB();
        _ = new SeveralPersistActionsEntityValidB();
        _ = new SeveralPersistActionsEntityValidB();

        using (counter.Attach()) {
          Assert.DoesNotThrow(() => session.SaveChanges());
        }

        var type = typeof(SeveralPersistActionsEntityValidB);
        var expectedCommandCount = Math.Ceiling(Math.Ceiling(3 * Domain.Model.Types[type].Fields.Count / (decimal) maxQueryParameterCount));
        Assert.That(counter.Count, Is.EqualTo(expectedCommandCount));
      }
    }

    [Test]
    public void InsertTest05()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        _ = new SeveralPersistActionsEntityValidC();
        _ = new SeveralPersistActionsEntityValidC();
        _ = new SeveralPersistActionsEntityValidC();


        using (counter.Attach()) {
          Assert.DoesNotThrow(() => session.SaveChanges());
        }

        var type = typeof(SeveralPersistActionsEntityValidC);
        var expectedCommandCount = Math.Ceiling(
          Math.Ceiling(3 * Domain.Model.Types[type].Fields.Count / (decimal) maxQueryParameterCount));
        Assert.That(counter.Count, Is.EqualTo(expectedCommandCount));
      }
    }

    [Test]
    public void InsertTest06()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        _ = new SeveralPersistActionsEntityInvalidA();
        _ = new SeveralPersistActionsEntityInvalidA();
        _ = new SeveralPersistActionsEntityInvalidA();

        using (counter.Attach()) {
          Assert.DoesNotThrow(() => session.SaveChanges());
        }
        var type = typeof(SeveralPersistActionsEntityInvalidA);
        var expectedCommandCount = Math.Ceiling(
          Math.Ceiling(3 * Domain.Model.Types[type].Fields.Count / (decimal) maxQueryParameterCount));
        Assert.That(counter.Count, Is.EqualTo(expectedCommandCount));
      }
    }

    [Test]
    public void InsertTest07()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        _ = new SeveralPersistActionsEntityInvalidB();
        _ = new SeveralPersistActionsEntityInvalidB();
        _ = new SeveralPersistActionsEntityInvalidB();

        using (counter.Attach()) {
          Assert.DoesNotThrow(() => session.SaveChanges());
        }
        var type = typeof(SeveralPersistActionsEntityInvalidB);
        var expectedCommandCount = Math.Ceiling(
          Math.Ceiling(3 * Domain.Model.Types[type].Fields.Count / (decimal) maxQueryParameterCount));
        Assert.That(counter.Count, Is.EqualTo(expectedCommandCount));
      }
    }

    [Test]
    public void InsertTest08()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        _ = new SeveralPersistActionsEntityInvalidC();
        _ = new SeveralPersistActionsEntityInvalidC();
        _ = new SeveralPersistActionsEntityInvalidC();

        using (counter.Attach()) {
          _ = Assert.Throws<ParametersLimitExceededException>(() => session.SaveChanges());
        }

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
          _ = new NormalAmountOfFieldsEntity();
          currentBatchCapacity--;
        }

        using (counter.Attach()) {
          Assert.That(session.Query.All<NormalAmountOfFieldsEntity>().Count(), Is.EqualTo(batchSize * 3));
        }

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
        var abc = session.Query.All<ALotOfFieldsEntityVersionized>().Count();
        session.Query.All<ALotOfFieldsEntityVersionized>().ForEach(e => {
          overallCount++;
          e.UpdateGeneratedFields();
        });

        _ = new NormalAmountOfFieldsEntity();

        using (counter.Attach()) {
          Assert.DoesNotThrow(() => session.SaveChanges());
        }
        Assert.That(counter.Count, Is.EqualTo(overallCount / 2));
      }
    }

    [Test]
    public void UpdateVersionizedTest02()
    {
      var sessionConfiguration = new SessionConfiguration(SessionOptions.Default | SessionOptions.AutoActivation | SessionOptions.ValidateEntityVersions);
      using (var session = Domain.OpenSession(sessionConfiguration))
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        var overallCount = 0;
        session.Query.All<ALotOfFieldsEntityVersionized>().ForEach(e => {
          overallCount++;
          e.UpdateGeneratedFields();
        });

        _ = new NormalAmountOfFieldsEntity();
        _ = new NormalAmountOfFieldsEntity();
        _ = new NormalAmountOfFieldsEntity();
        _ = new NormalAmountOfFieldsEntity();

        using (counter.Attach()) {
          Assert.DoesNotThrow(() => session.SaveChanges());
        }

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

        _ = new NormalAmountOfFieldsEntity();

        using (counter.Attach()) {
          Assert.DoesNotThrow(() => session.SaveChanges());
        }

        Assert.That(counter.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void PartialExecutionAllowedTest01()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        var countBefore = session.Query.All<ALotOfFieldsEntityValid>().Count();

        _ = new ALotOfFieldsEntityValid();
        _ = new ALotOfFieldsEntityValid();
        _ = new ALotOfFieldsEntityValid();

        //persist by query causes allowPartialExecution = true;
        using (counter) {
          session.Persist(PersistReason.Query);
        }

        Assert.That(counter.Count, Is.EqualTo(0));

        counter.Reset();
        using (counter.Attach()) {
          Assert.That(session.Query.All<ALotOfFieldsEntityValid>().Count(), Is.EqualTo(countBefore + 3));
        }

        Assert.That(counter.Count, Is.EqualTo(2));
      }
    }

    [Test]
    public async Task PartialExecutionAllowedAsyncTest01()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        var countBefore = session.Query.All<ALotOfFieldsEntityValid>().Count();

        _ = new ALotOfFieldsEntityValid();
        _ = new ALotOfFieldsEntityValid();
        _ = new ALotOfFieldsEntityValid();

        //persist by query causes allowPartialExecution = true;
        using (counter) {
          session.Persist(PersistReason.Query);
        }

        Assert.That(counter.Count, Is.EqualTo(0));

        counter.Reset();
        using (counter.Attach()) {
          var result = (await session.Query.All<ALotOfFieldsEntityValid>().ExecuteAsync()).ToArray().Length;
          Assert.That(result, Is.EqualTo(countBefore + 3));
        }

        Assert.That(counter.Count, Is.EqualTo(2));
      }
    }

    [Test]
    public void PartialExecutionAllowedTest02()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        var batchSize = session.Configuration.BatchSize;
        var currentBatchCapacity = batchSize;

        Console.WriteLine(batchSize);
        // one complete batch;
        while (currentBatchCapacity > 0) {
          _ = new NormalAmountOfFieldsEntity();
          currentBatchCapacity--;
        }

        // extra task to have extra batch
        _ = new NormalAmountOfFieldsEntity();

        //persist by query causes allowPartialExecution = true;
        using (counter.Attach()) {
          session.Persist(PersistReason.Query);
        }

        Assert.That(counter.Count, Is.EqualTo(1));
        counter.Reset();
        using (counter.Attach()) {
          Assert.That(session.Query.All<NormalAmountOfFieldsEntity>().Count(), Is.EqualTo(batchSize + 1));
        }
        Assert.That(counter.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public async Task PartialExecutionAllowedAsyncTest02()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        var batchSize = session.Configuration.BatchSize;
        var currentBatchCapacity = batchSize;

        Console.WriteLine(batchSize);
        // one complete batch;
        while (currentBatchCapacity > 0) {
          _ = new NormalAmountOfFieldsEntity();
          currentBatchCapacity--;
        }

        // extra task to have extra batch
        _ = new NormalAmountOfFieldsEntity();

        //persist by query causes allowPartialExecution = true;
        using (counter.Attach()) {
          session.Persist(PersistReason.Query);
        }

        Assert.That(counter.Count, Is.EqualTo(1));
        counter.Reset();
        using (counter.Attach()) {
          var result = (await session.Query.All<NormalAmountOfFieldsEntity>().ExecuteAsync()).ToArray().Length;
          Assert.That(result, Is.EqualTo(batchSize + 1));
        }
        Assert.That(counter.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void PartialExecutionAllowedTest03()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        var batchSize = session.Configuration.BatchSize;
        var currentBatchCapacity = batchSize;

        Console.WriteLine(batchSize);
        // one complete batch;
        while (currentBatchCapacity > 0) {
          _ = new NormalAmountOfFieldsEntity();
          currentBatchCapacity--;
        }

        // extra task to have extra batch
        _ = new NormalAmountOfFieldsEntity();

        //persist by query causes allowPartialExecution = true;
        using (counter.Attach()) {
          var result = session.Query.All<NormalAmountOfFieldsEntity>().ToArray();
          Assert.That(result.Length, Is.EqualTo(batchSize + 1));
        }
        Assert.That(counter.Count, Is.EqualTo(2));
      }
    }

    [Test]
    public async Task PartialExecutionAllowedAsyncTest03()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        var batchSize = session.Configuration.BatchSize;
        var currentBatchCapacity = batchSize;

        Console.WriteLine(batchSize);
        // one complete batch;
        while (currentBatchCapacity > 0) {
          _ = new NormalAmountOfFieldsEntity();
          currentBatchCapacity--;
        }

        // extra task to have extra batch
        _ = new NormalAmountOfFieldsEntity();

        //persist by query causes allowPartialExecution = true;
        using (counter.Attach()) {
          var result = (await session.Query.All<NormalAmountOfFieldsEntity>().ExecuteAsync()).ToArray();
          Assert.That(result.Length, Is.EqualTo(batchSize + 1));
        }
        Assert.That(counter.Count, Is.EqualTo(2));
      }
    }

    [Test]
    public void PartialExecutionAllowedTest04()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        var countBefore = session.Query.All<ALotOfFieldsEntityValid>().Count();

        var batchSize = session.Configuration.BatchSize;
        var currentBatchCapacity = batchSize;

        Console.WriteLine(batchSize);
        // one complete batch;
        while (currentBatchCapacity > 0) {
          _ = new ALotOfFieldsEntityValid();
          currentBatchCapacity--;
        }

        // extra task to have extra batch
        _ = new ALotOfFieldsEntityValid();

        //persist by query causes allowPartialExecution = true;
        using (counter.Attach()) {
          var result = session.Query.All<ALotOfFieldsEntityValid>().ToArray();
          Assert.That(result.Length, Is.EqualTo(countBefore + batchSize + 1));
        }
        Assert.That(counter.Count, Is.EqualTo(13));
      }
    }

    [Test]
    public async Task PartialExecutionAllowedAsyncTest04()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        var batchSize = session.Configuration.BatchSize;
        var currentBatchCapacity = batchSize;

        Console.WriteLine(batchSize);
        // one complete batch;
        while (currentBatchCapacity > 0) {
          _ = new NormalAmountOfFieldsEntity();
          currentBatchCapacity--;
        }

        // extra task to have extra batch
        _ = new NormalAmountOfFieldsEntity();

        //persist by query causes allowPartialExecution = true;
        using (counter.Attach()) {
          var result = (await session.Query.All<NormalAmountOfFieldsEntity>().ExecuteAsync()).ToArray();
          Assert.That(result.Length, Is.EqualTo(batchSize + 1));
        }
        Assert.That(counter.Count, Is.EqualTo(2));
      }
    }

    [Test]
    public void PartialExecutionAllowedTest05()
    {
      Require.ProviderIs(StorageProvider.SqlServer);

      var sessionOf25ItemsBatch = new SessionConfiguration(WellKnown.Sessions.Default, SessionOptions.Default | SessionOptions.AutoActivation) { BatchSize = 25 };
      using (var session = Domain.OpenSession(sessionOf25ItemsBatch))
      using (var tx = session.OpenTransaction()) {
        Assert.AreEqual(0, session.Query.All<OneHundredFieldsEntity>().Count());

        for (var i = 0; i < session.Configuration.BatchSize; i++) {
          var item = new OneHundredFieldsEntity();

          for (var j = 1; j < 98; j++) {
            item["Value" + j] = i;
          }
        }

        var count = session.Query.All<OneHundredFieldsEntity>().ToArray().Length;
        Assert.AreEqual(session.Configuration.BatchSize, count);
      }
    }

    [Test]
    public void PartialExecutionDeniedTest01()
    {
      using (var session = Domain.OpenSession())
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        _ = new ALotOfFieldsEntityValid();
        _ = new ALotOfFieldsEntityValid();
        _ = new ALotOfFieldsEntityValid();

        //manual persist causes allowPartialExecution = false;
        using (counter.Attach()) {
          session.SaveChanges();
        }

        Assert.That(counter.Count, Is.EqualTo(2));
      }
    }
  }
}
