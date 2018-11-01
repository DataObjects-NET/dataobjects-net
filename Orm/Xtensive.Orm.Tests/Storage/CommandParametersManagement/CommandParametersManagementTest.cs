// Copyright (C) 2018 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kudelin
// Created:    2018.10.26

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Storage.CommandParametersManagement.Model;

namespace Xtensive.Orm.Tests.Storage.CommandParametersManagement
{
  public class CommandParametersManagementTest : AutoBuildTest
  {
    private  readonly int aLotOfFieldsEntityFieldCount = typeof(ALotOfFieldsEntity)
      .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Length;
    private readonly int aLotOfFieldsEntityFieldVersionedCount = typeof(ALotOfFieldsEntityVersioned)
      .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Length;

    private SessionConfiguration GetLimitedBatchSizeSessionConfiguration(SessionOptions options = SessionOptions.Default)
    {
      return new SessionConfiguration { BatchSize = 10, Options = options };
    }

    [Test]
    public void PersistTest()
    {
      const int requestCount = 100;
      var commands = new List<DbCommandEventArgs>();
      int parametersCount = 0;

      using (var session = Domain.OpenSession(GetLimitedBatchSizeSessionConfiguration()))
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        session.Events.DbCommandExecuted += (s, e) => {
          commands.Add(e);
          parametersCount += e.Command.Parameters.Count;
        };

        for (var i = 0; i < requestCount; i++)
          new ALotOfFieldsEntity();

        Assert.DoesNotThrow(session.SaveChanges);
        Assert.That(parametersCount, Is.EqualTo(requestCount * aLotOfFieldsEntityFieldCount + requestCount));
        Assert.That(commands.All(c => c.Command.Parameters.Count < ProviderInfo.MaxQueryParameterCount), Is.True);
      }
    }

    [Test]
    public void PersistVersionedTest()
    {
      const int requestCount = 100;
      var commands = new List<DbCommandEventArgs>();
      int parametersCount = 0;

      using (var session = Domain.OpenSession(GetLimitedBatchSizeSessionConfiguration(SessionOptions.ValidateEntityVersions)))
      using (session.Activate())
      using (var transaction = session.OpenTransaction())
      {
        session.Events.DbCommandExecuted += (s, e) => {
          commands.Add(e);
          parametersCount += e.Command.Parameters.Count;
        };

        for (var i = 0; i < requestCount; i++)
          new ALotOfFieldsEntityVersioned();

        Assert.DoesNotThrow(session.SaveChanges);

        foreach (var entity in session.Query.All<ALotOfFieldsEntityVersioned>()) {
          entity.VersionedField = 123;
        }

        Assert.DoesNotThrow(session.SaveChanges);
        Assert.That(parametersCount, Is.EqualTo(requestCount * (aLotOfFieldsEntityFieldCount + aLotOfFieldsEntityFieldVersionedCount) 
                                                + requestCount + requestCount * 3));
        Assert.That(commands.All(c => c.Command.Parameters.Count < ProviderInfo.MaxQueryParameterCount), Is.True);
      }
    }

    [Test]
    public void LoadTest()
    {
      var commands = new List<DbCommandEventArgs>();
      const int requestCount = 100, parametersPerRequest = 317;

      using (var session = Domain.OpenSession(GetLimitedBatchSizeSessionConfiguration()))
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        session.Events.DbCommandExecuted += (s, e) => commands.Add(e);
        Enumerable.Range(0, requestCount).Select(
          i => {
            var range = Enumerable.Range(0, parametersPerRequest).ToArray();
            return session.Query.ExecuteDelayed(query => query.All<ALotOfFieldsEntity>().Where(x => x.Id.In(IncludeAlgorithm.ComplexCondition, range)));
          }).ToArray();
        session.Query.All<ALotOfFieldsEntity>().Run();

        Assert.That(commands.All(c => c.Command.Parameters.Count < ProviderInfo.MaxQueryParameterCount), Is.True);
        Assert.That(commands.Sum(x => x.Command.Parameters.Count), Is.EqualTo(requestCount * parametersPerRequest));
      }
    }

    [Test]
    public void ExceedParametersLimitByOneQuery()
    {
      RequireKnowsMaxParametersCount();

      using (var session = Domain.OpenSession(GetLimitedBatchSizeSessionConfiguration()))
      using (session.Activate())
      using (var t = session.OpenTransaction()) {
        var range = Enumerable.Range(0, ProviderInfo.MaxQueryParameterCount).ToArray();
        bool anyCommandExecuted = false;
        bool isExceptionAppeared = false;

        session.Events.DbCommandExecuted += (sender, e) =>
        {
          anyCommandExecuted = true;
        };

        Assert.Throws<StorageException>(() => session.Query.All<ALotOfFieldsEntity>().Where(x => x.Id.In(IncludeAlgorithm.ComplexCondition, range)).Run());
        Assert.That(anyCommandExecuted, Is.False);
      }
    }

    [Test]
    public void MaxValidParamtersCountInSingleQuery()
    {
      RequireKnowsMaxParametersCount();

      using (var session = Domain.OpenSession(GetLimitedBatchSizeSessionConfiguration()))
      using (session.Activate())
      using (var t = session.OpenTransaction()) {
        var range = Enumerable.Range(0, ProviderInfo.MaxQueryParameterCount - 1).ToArray();
        bool anyCommandExecuted = false;
        bool isExceptionAppeared = false;

        session.Events.DbCommandExecuted += (sender, e) =>
        {
          anyCommandExecuted = true;
        };
        try
        {
          session.Query.All<ALotOfFieldsEntity>().Where(x => x.Id.In(IncludeAlgorithm.ComplexCondition, range)).Run();
        }
        catch (Exception)
        {
          isExceptionAppeared = true;
        }

        Assert.That(isExceptionAppeared, Is.False);
        Assert.That(anyCommandExecuted, Is.True);
      }
    }

    [Test]
    public void LastRequestWithOtherQueriesInBatchTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);

      const int requestCount = 12,
                parametersPerRequest = 699,
                lastRequestParametersCount = 2;

      var expectedPrarmetersCount = requestCount * parametersPerRequest + lastRequestParametersCount;
      var expectedCommandCount = ((int) (expectedPrarmetersCount) / ProviderInfo.MaxQueryParameterCount);

      using (var session = Domain.OpenSession(GetLimitedBatchSizeSessionConfiguration()))
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var commands = new List<DbCommandEventArgs>();
        var parametersCount = 0;
        session.Events.DbCommandExecuting += (s, e) => {
          commands.Add(e);
          parametersCount += e.Command.Parameters.Count;
        };

        Enumerable.Range(0, requestCount).Select(
          i => {
            var range = Enumerable.Range(0, parametersPerRequest).ToArray();
            return session.Query.ExecuteDelayed(query => query.All<ALotOfFieldsEntity>().Where(x => x.Id.In(IncludeAlgorithm.ComplexCondition, range)));
          }).ToArray();

        int value1 = 1, value2 = 1;
        session.Query.All<ALotOfFieldsEntity>().Where(x => x.Id > value1).ToArray();

        Assert.That(commands.Count, Is.EqualTo(expectedCommandCount));
        Assert.That(commands[commands.Count - 1].Command.Parameters.Count > lastRequestParametersCount, Is.True);
        Assert.That(parametersCount, Is.EqualTo(requestCount * parametersPerRequest + lastRequestParametersCount));
      }
    }

    [Test]
    public void LastRequestInSeparateBatchTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);

      const int requestCount = 12,
                parametersPerRequest = 699,
                lastRequestParametersCount = 3;

      var expectedPrarmetersCount = requestCount * parametersPerRequest + lastRequestParametersCount;
      var expectedCommandCount = (requestCount / (ProviderInfo.MaxQueryParameterCount / parametersPerRequest) + 1);

      using (var session = Domain.OpenSession(GetLimitedBatchSizeSessionConfiguration()))
      using (session.Activate())
      using (var t = session.OpenTransaction()) {
        var commands = new List<DbCommandEventArgs>();
        var parametersCount = 0;
        session.Events.DbCommandExecuted += (s, e) => {
          commands.Add(e);
          parametersCount += e.Command.Parameters.Count;
        };

        const int reqCount = 12, paramCount = 699, lastReqparamCount = 3;
        Enumerable.Range(0, reqCount).Select(i =>
          {
            var range = Enumerable.Range(0, paramCount).ToArray();
            return session.Query.ExecuteDelayed(query => query.All<ALotOfFieldsEntity>().Where(x => x.Id.In(IncludeAlgorithm.ComplexCondition, range)));
          }).ToArray();

        int value1 = 1, value2 = 1, value3 = 1;
        session.Query.All<ALotOfFieldsEntity>().Where(x => x.Id > value1 && x.Id > value2 && x.Id > value3).ToArray();

        Assert.That(commands.Count, Is.EqualTo(expectedCommandCount));
        Assert.That(commands.Last().Command.Parameters.Count, Is.EqualTo(lastReqparamCount));
        Assert.That(parametersCount, Is.EqualTo(reqCount * paramCount + lastReqparamCount));
      }
    }

    [Test]
    public void AllowPartialExecutionTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);

      using (var session = Domain.OpenSession(GetLimitedBatchSizeSessionConfiguration()))
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var commands = new List<DbCommandEventArgs>();
        session.Events.DbCommandExecuted += (s, e) => commands.Add(e);

        Enumerable.Range(0, 100).Select(
          i => {
            var range = Enumerable.Range(0, 500).ToArray();
            return session.Query.ExecuteDelayed(query => query.All<ALotOfFieldsEntity>().Where(x => x.Id.In(IncludeAlgorithm.ComplexCondition, range)));
          }).ToArray();

        //partial excution allowed
        session.CreateEnumerationContext();
        Assert.That(commands.Count, Is.EqualTo(23));

        //partial execution is not allowed
        session.Handler.ExecuteQueryTasks(new QueryTask[0], false);
        Assert.That(commands.Count, Is.EqualTo(25));
      }
    }

    private void RequireKnowsMaxParametersCount()
    {
      if(ProviderInfo.MaxQueryParameterCount==int.MaxValue)
        throw new IgnoreException("Provider has no limit for parameters or it is unknown.");
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (ALotOfFieldsEntity).Assembly, typeof (ALotOfFieldsEntity).Namespace);
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
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