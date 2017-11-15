// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2015.02.03

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Services;
using Xtensive.Sql;
using Xtensive.Sql.Model;
using Xtensive.Orm.Tests.Issues.IssueJira0571_MySqlKeyGenerationProblemModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0571_MySqlKeyGenerationProblemModel
{
  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.Default, Name="GeneratorForEntity1")]
  public class Entity1 : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string TextData { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0571_MySqlKeyGenerationProblem : AutoBuildTest
  {
    private DomainConfiguration recreateConfiguration;
    private DomainConfiguration performConfiguration;
    private Catalog extractedCatalog;

    [Test]
    public void NormalModeGeneration()
    {
      Require.AllFeaturesNotSupported(ProviderFeatures.Sequences);
      Require.AllFeaturesNotSupported(ProviderFeatures.AutoIncrementSettingsInMemory);
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        foreach (var keyGenerator in Domain.Configuration.KeyGenerators)
          CheckGeneratorsAfterDomainBuilded(keyGenerator, session, 0);
        for (int i = 0; i < 13; i++ ) {
          new Entity1();
        }
        foreach (var keyGenerator in Domain.Configuration.KeyGenerators)
          CheckGeneratorsAfterDomainBuilded(keyGenerator, session, 0);
        for (int i = 0; i < 120; i++) {
          new Entity1();
        }
        foreach (var keyGenerator in Domain.Configuration.KeyGenerators)
          CheckGeneratorsAfterDomainBuilded(keyGenerator, session, 0);
        for (int i = 0; i < 250; i++) {
          new Entity1();
        }
        foreach (var keyGenerator in Domain.Configuration.KeyGenerators)
          CheckGeneratorsAfterDomainBuilded(keyGenerator, session, 0);
      }
    }

    [Test]
    public void MySqlModeGeneration()
    {
      Require.AllFeaturesNotSupported(ProviderFeatures.Sequences);
      Require.AllFeaturesSupported(ProviderFeatures.AutoIncrementSettingsInMemory);

      using (var session = Domain.OpenSession()) {
        foreach (var keyGenerator in Domain.Configuration.KeyGenerators)
          CheckGeneratorsAfterDomainBuilded(keyGenerator, session, 0);
      }
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        for (int i = 0; i < 1; i++) {
          var entity = new Entity1();
        }
      }
      using (var session = Domain.OpenSession()) {
        foreach (var keyGenerator in Domain.Configuration.KeyGenerators)
          CheckGeneratorsAfterDomainBuilded(keyGenerator, session, 1);
      }
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        for (int i = 0; i < 120; i++) {
          var entity = new Entity1();
        }
      }

      using (var session = Domain.OpenSession()) {
        foreach (var keyGenerator in Domain.Configuration.KeyGenerators)
          CheckGeneratorsAfterDomainBuilded(keyGenerator, session, 1);
      }
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        for (int i = 0; i < 250; i++) {
          var entity = new Entity1();
        }
      }
      using (var session = Domain.OpenSession()) {
        foreach (var keyGenerator in Domain.Configuration.KeyGenerators)
          CheckGeneratorsAfterDomainBuilded(keyGenerator, session, 1);
      }
    }

    private ISqlCompileUnit GetQuery(Table generatorTable)
    {
      var select = SqlDml.Select();
      select.Columns.Add(SqlDml.Count());
      select.From = SqlDml.TableRef(generatorTable);
      return select;
    }

    private void CheckGeneratorsAfterDomainBuilded(KeyGeneratorConfiguration keyGeneratorConfiguration, Session session, int expectedValue)
    {
      if (Domain.Configuration.IsMultischema) {
        foreach (var schema in extractedCatalog.Schemas) {
          CheckSchemaGenerators(schema, keyGeneratorConfiguration.Name+ "-Generator", session, expectedValue);
        }
      }
      else {
        CheckSchemaGenerators(extractedCatalog.DefaultSchema, keyGeneratorConfiguration.Name + "-Generator", session, expectedValue);
      }
    }
    
    private void CheckSchemaGenerators(Schema schema, string generatorName, Session session, int expectedValue)
    {
      var generatorTable = schema.Tables[generatorName];
      if (generatorTable == null)
        return;
      var query = GetQuery(generatorTable);
      var queryBuilder = GetQueryBuilder(session);
      var queryCompilationResult = queryBuilder.CompileQuery(query);
      var command = queryBuilder.CreateCommand(queryBuilder.CreateRequest(queryCompilationResult, Enumerable.Empty<Services.QueryParameterBinding>()));
      var rowCount = command.ExecuteScalar();
      Assert.AreEqual(expectedValue, rowCount);
    }

    private QueryBuilder GetQueryBuilder(Session session)
    {
      return session.Services.Get<QueryBuilder>();
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      return recreateConfiguration;
    }

    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.MySql);
    }

    public override void TestFixtureSetUp()
    {
      recreateConfiguration = base.BuildConfiguration();
      recreateConfiguration.Types.Register(typeof (Entity1));
      recreateConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      recreateConfiguration.KeyGenerators.Add(new KeyGeneratorConfiguration("GeneratorForEntity1") {CacheSize = 128, Seed = 0});

      base.TestFixtureSetUp();
      var driver = TestSqlDriver.Create(Domain.Configuration.ConnectionInfo);
      using (var sqlConnection = driver.CreateConnection()) {
        sqlConnection.Open();
        extractedCatalog = driver.ExtractCatalog(sqlConnection);
        sqlConnection.Close();
      }
    }
  }
}
