// Copyright (C) 2018 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kudelin
// Created:    2018.10.26

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Storage.CommandParametersManagement.Model;

namespace Xtensive.Orm.Tests.Storage.CommandParametersManagement
{
  public class CommandParametersManagementTest : AutoBuildTest
  {
    [Test]
    public void PersistTest()
    {
      const int requestCount = 100;
      var commandParametersCount = new List<int>();
      int parametersCount = 0;

      using (var session = Domain.OpenSession(GetLimitedBatchSizeSessionConfiguration()))
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var expectedPrarmetersCount = requestCount * session.Domain.Model.Types[typeof (ALotOfFieldsEntity)].Fields.Count;

        session.Events.DbCommandExecuted += (s, e) => {
          commandParametersCount.Add(e.Command.Parameters.Count);
        };

        for (var i = 0; i < requestCount; i++)
          new ALotOfFieldsEntity();

        Assert.DoesNotThrow(session.SaveChanges);
        Assert.That(commandParametersCount.Sum(), Is.EqualTo(expectedPrarmetersCount));
        Assert.That(commandParametersCount.All(c => c < ProviderInfo.MaxQueryParameterCount), Is.True);
      }
    }

    [Test]
    public void PersistVersionedTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Batches);

      const int requestCount = 100;
      var commandParametersCount = new List<int>();

      using (var session = Domain.OpenSession(GetLimitedBatchSizeSessionConfiguration(SessionOptions.ValidateEntityVersions)))
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var expectedPrarmetersCount = requestCount * session.Domain.Model.Types[typeof (ALotOfFieldsEntityVersioned)].Fields.Count
                                      + requestCount * 3;

        session.Events.DbCommandExecuted += (s, e) => {
          commandParametersCount.Add(e.Command.Parameters.Count);
        };

        for (var i = 0; i < requestCount; i++)
          new ALotOfFieldsEntityVersioned();

        Assert.DoesNotThrow(session.SaveChanges);

        foreach (var entity in session.Query.All<ALotOfFieldsEntityVersioned>()) {
          entity.VersionedField = 123;
        }

        Assert.DoesNotThrow(session.SaveChanges);
        Assert.That(commandParametersCount.Sum(), Is.EqualTo(expectedPrarmetersCount));
        Assert.That(commandParametersCount.All(c => c < ProviderInfo.MaxQueryParameterCount), Is.True);
      }
    }

    [Test]
    public void LoadTest()
    {
      var commandParametersCount = new List<int>();
      int requestCount = 100,
        parametersPerRequest = 317,
        expectedPrarmetersCount = requestCount * parametersPerRequest;

      if (ProviderInfo.ProviderName==WellKnown.Provider.Oracle)
        expectedPrarmetersCount += requestCount + 1;

      using (var session = Domain.OpenSession(GetLimitedBatchSizeSessionConfiguration()))
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        session.Events.DbCommandExecuted += (s, e) => commandParametersCount.Add(e.Command.Parameters.Count);
        Enumerable.Range(0, requestCount).Select(
          i => {
            var range = Enumerable.Range(0, parametersPerRequest).ToArray();
            return session.Query.ExecuteDelayed(query => query.All<ALotOfFieldsEntity>().Where(x => x.Id.In(IncludeAlgorithm.ComplexCondition, range)));
          }).ToArray();
        session.Query.All<ALotOfFieldsEntity>().Run();

        Assert.That(commandParametersCount.All(c => c < ProviderInfo.MaxQueryParameterCount), Is.True);
        Assert.That(commandParametersCount.Sum(), Is.EqualTo(expectedPrarmetersCount));
      }
    }

    [Test]
    public void ExceedParametersLimitByOneQuery()
    {
      RequireKnowsMaxParametersCount();

      var maxQueryParameterCount = ProviderInfo.MaxQueryParameterCount;
      if (ProviderInfo.ProviderName!=WellKnown.Provider.Oracle)
        maxQueryParameterCount += 1;

      using (var session = Domain.OpenSession(GetLimitedBatchSizeSessionConfiguration()))
      using (session.Activate())
      using (var t = session.OpenTransaction()) {
        session.Events.DbCommandExecuted += (sender, e) => Assert.Fail();
        Assert.Throws<StorageException>(() => GenerateInQueries(session.Query.All<ALotOfFieldsEntity>(), maxQueryParameterCount).Run());
      }
    }

    [Test]
    public void MaxValidParamtersCountInSingleQuery()
    {
      RequireKnowsMaxParametersCount();
      Require.ProviderIsNot(StorageProvider.Oracle, "Connection timeout..");

      var maxQueryParameterCount = ProviderInfo.MaxQueryParameterCount;
      /*if (ProviderInfo.ProviderName==WellKnown.Provider.Oracle)
        maxQueryParameterCount -= 1;*/
      var config = GetLimitedBatchSizeSessionConfiguration();
      using (var session = Domain.OpenSession(config))
      using (session.Activate())
      using (var t = session.OpenTransaction()) {
        var anyCommandExecuted = false;
        session.Events.DbCommandExecuted += (sender, e) => anyCommandExecuted = true;

        Assert.DoesNotThrow(() => GenerateInQueries(session.Query.All<ALotOfFieldsEntity>(), maxQueryParameterCount).Run());
        Assert.That(anyCommandExecuted, Is.True);
      }
    }

    [Test]
    public void LastRequestWithOtherQueriesInBatchTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);

      var commandParametersCount = new List<int>();
      int requestCount = 12,
        parametersPerRequest = (ProviderInfo.MaxQueryParameterCount - 1) / 3,
        lastRequestParametersCount = 2,
        expectedPrarmetersCount = requestCount * parametersPerRequest + lastRequestParametersCount;

      using (var session = Domain.OpenSession(GetLimitedBatchSizeSessionConfiguration()))
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        session.Events.DbCommandExecuting += (s, e) => commandParametersCount.Add(e.Command.Parameters.Count);;

        Enumerable.Range(0, requestCount).Select(
          i => {
            var range = Enumerable.Range(0, parametersPerRequest).ToArray();
            return session.Query.ExecuteDelayed(query => query.All<ALotOfFieldsEntity>().Where(x => x.Id.In(IncludeAlgorithm.ComplexCondition, range)));
          }).ToArray();

        int value1 = 1, value2 = 1;
        session.Query.All<ALotOfFieldsEntity>().Where(x => x.Id > value1 || x.Id > value2).ToArray();

        Assert.That(commandParametersCount.Sum(), Is.EqualTo(expectedPrarmetersCount));
        Assert.That(commandParametersCount.Last(), Is.EqualTo(lastRequestParametersCount));
      }
    }

    [Test]
    public void LastRequestInSeparateBatchTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);

      var commandParametersCount = new List<int>();
      int requestCount = 12,
        parametersPerRequest = (ProviderInfo.MaxQueryParameterCount - 1) / 3,
        lastRequestParametersCount = 3;

      var expectedPrarmetersCount = requestCount * parametersPerRequest + lastRequestParametersCount;
      var expectedCommandCount = (requestCount / (ProviderInfo.MaxQueryParameterCount / parametersPerRequest) + 1);

      using (var session = Domain.OpenSession(GetLimitedBatchSizeSessionConfiguration()))
      using (session.Activate())
      using (var t = session.OpenTransaction()) {
        session.Events.DbCommandExecuted += (s, e) => commandParametersCount.Add(e.Command.Parameters.Count);

        Enumerable.Range(0, requestCount).Select(
          i => {
            var range = Enumerable.Range(0, parametersPerRequest).ToArray();
            return session.Query.ExecuteDelayed(query => query.All<ALotOfFieldsEntity>().Where(x => x.Id.In(IncludeAlgorithm.ComplexCondition, range)));
          }).ToArray();

        int value1 = 1, value2 = 1, value3 = 1;
        session.Query.All<ALotOfFieldsEntity>().Where(x => x.Id > value1 && x.Id > value2 && x.Id > value3).ToArray();

        Assert.That(commandParametersCount.Count, Is.EqualTo(expectedCommandCount));
        Assert.That(commandParametersCount.Last(), Is.EqualTo(lastRequestParametersCount));
        Assert.That(commandParametersCount.Sum(), Is.EqualTo(expectedPrarmetersCount));
      }
    }

    [Test]
    public void AllowPartialExecutionTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);

      var commandParametersCount = new List<int>();
      using (var session = Domain.OpenSession(GetLimitedBatchSizeSessionConfiguration()))
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        session.Events.DbCommandExecuted += (s, e) => commandParametersCount.Add(e.Command.Parameters.Count);

        Enumerable.Range(0, 100).Select(
          i => {
            var range = Enumerable.Range(0, 500).ToArray();
            return session.Query.ExecuteDelayed(query => query.All<ALotOfFieldsEntity>().Where(x => x.Id.In(IncludeAlgorithm.ComplexCondition, range)));
          }).ToArray();

        //partial excution allowed
        session.CreateEnumerationContext();
        Assert.That(commandParametersCount.Count, Is.EqualTo(23));

        //partial execution is not allowed
        session.Handler.ExecuteQueryTasks(new QueryTask[0], false);
        Assert.That(commandParametersCount.Count, Is.EqualTo(25));
      }
    }

    private void RequireKnowsMaxParametersCount()
    {
      if (ProviderInfo.MaxQueryParameterCount==int.MaxValue)
        throw new IgnoreException("Provider has no limit for parameters or it is unknown.");
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (ALotOfFieldsEntity).Assembly, typeof (ALotOfFieldsEntity).Namespace);
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
    }

    private SessionConfiguration GetLimitedBatchSizeSessionConfiguration(SessionOptions options = SessionOptions.Default)
    {
      return new SessionConfiguration {BatchSize = 10, Options = options};
    }

    private IQueryable<T> GenerateInQueries<T>(IQueryable<T> query, int paramsCount)
      where T : ALotOfFieldsEntity
    {
      const int maxInParamsCount = 999;
      Expression expression = null;
      var tParameter = Expression.Parameter(typeof (T), "x");
      while (paramsCount > 0) {
        var current = (paramsCount -= maxInParamsCount) > 0 ? maxInParamsCount : maxInParamsCount + paramsCount;
        var range = Enumerable.Range(0, current).ToArray();
        Expression<Func<T, bool>> rightExpression = x => x.Id.In(IncludeAlgorithm.ComplexCondition, range);
        expression = expression==null ? rightExpression.Body : Expression.Or(expression, rightExpression.Body);
        var arguments = ((MethodCallExpression) rightExpression.Body).Arguments;
        var tParameterOld = ((MemberExpression) arguments.First()).Expression;
        expression = ExpressionReplacer.Replace(expression, tParameterOld, tParameter);
      }

      var lambda = Expression.Lambda<Func<T, bool>>(expression, tParameter);
      return query.Where(lambda);
    }
  }
}

namespace Xtensive.Orm.Tests.Storage.CommandParametersManagement.Model
{
  public class ALotOfFieldsEntityVersioned: ALotOfFieldsEntity
  {
    [Field]
    [Version(VersionMode.Auto)]
    public int VersionedField { get; set; }
  }
}