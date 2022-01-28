// Copyright (C) 2012-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.02.27

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Services;
using Xtensive.Orm.Tests.Storage.QueryBuilderApiTestMode;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Tests.Storage
{
  namespace QueryBuilderApiTestMode
  {
    [HierarchyRoot]
    public class CheckEntity : Entity
    {
      [Field, Key]
      public int Id { get; private set; }
    }
  }

  public class QueryBuilderTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (CheckEntity));
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        _ = new CheckEntity();
        tx.Complete();
      }
    }

    [Test]
    public void ModifyQueryTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var builder = session.Services.Get<QueryBuilder>();
        Assert.That(builder, Is.Not.Null);

        var query = Query.All<CheckEntity>();
        var translated = builder.TranslateQuery(query);
        Assert.That(translated, Is.Not.Null);
        Assert.That(translated.Query, Is.Not.Null);
        Assert.That(translated.ParameterBindings, Is.Not.Null);

        var select = translated.Query;
        select.Columns.Clear();
        select.Columns.Add(SqlDml.Count());
        var compiled = builder.CompileQuery(translated.Query);
        Assert.That(compiled, Is.Not.Null);

        var request = builder.CreateRequest(compiled, translated.ParameterBindings);
        Assert.That(request, Is.Not.Null);

        var command = builder.CreateCommand(request);
        Assert.That(command, Is.Not.Null);

        using (command) {
          var result = Convert.ToInt32(command.ExecuteScalar());
          Assert.That(result, Is.EqualTo(1));
        }

        tx.Complete();
      }
    }

    [Test]
    public void ComposeQuery()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var builder = session.Services.Get<QueryBuilder>();
        Assert.That(builder, Is.Not.Null);

        var binding = builder.CreateParameterBinding(typeof(int), context => 43);
        SqlSelect select;
        if (StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.Firebird)) {
          //require cast of parameters on sql level
          select = SqlDml.Select(SqlDml.Cast(binding.ParameterReference, SqlType.Int32));
        }
        else {
          select = SqlDml.Select(binding.ParameterReference);
        }

        var compiled = builder.CompileQuery(select);
        Assert.That(compiled, Is.Not.Null);

        var request = builder.CreateRequest(compiled, new[] { binding });
        Assert.That(request, Is.Not.Null);

        var command = builder.CreateCommand(request);
        Assert.That(command, Is.Not.Null);

        using (command) {
          var result = Convert.ToInt32(command.ExecuteScalar());
          Assert.That(result, Is.EqualTo(43));
        }

        tx.Complete();
      }
    }
  }
}
