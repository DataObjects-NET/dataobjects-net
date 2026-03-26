// Copyright (C) 2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Sql;
using Xtensive.Sql.Model;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class IgnoreRulesHandlerTest
  {
    private readonly struct TestSetting
    {
      public SchemaExtractionResult ExtractionResult { get; init; }
      public DomainConfiguration DomainConfiguration { get; init; }
      public MappingResolver MappingResolver { get; init; }
    }

    #region Ignore Table

    [Test]
    public void Test01()
    {
      var settings = CreateTestSettings();
      var original = settings.ExtractionResult;

      Assert.That(original.Catalogs[0].DefaultSchema.Tables.Any(t => t.Name == "prefix-table"), Is.True);

      var ignoreCollection = new IgnoreRuleCollection();
      _ = ignoreCollection.IgnoreTable("prefix-table");

      var processingResults = GetProcessedResults(settings, ignoreCollection);
      Assert.That(processingResults.Catalogs[0].DefaultSchema.Tables.Any(t => t.Name == "prefix-table"), Is.False);
    }

    [Test]
    public void Test02()
    {
      var settings = CreateTestSettings();
      var original = settings.ExtractionResult;

      Assert.That(original.Catalogs[0].Schemas["dbo"].Tables.Any(t => t.Name == "prefix-table"), Is.True);

      var ignoreCollection = new IgnoreRuleCollection();
      _ = ignoreCollection.IgnoreTable("prefix-table").WhenSchema("dbo");

      var processingResults = GetProcessedResults(settings, ignoreCollection);
      Assert.That(processingResults.Catalogs[0].Schemas["dbo"].Tables.Any(t => t.Name == "prefix-table"), Is.False);
    }

    [Test]
    public void Test03()
    {
      var settings = CreateTestSettings();
      var original = settings.ExtractionResult;

      Assert.That(original.Catalogs["prefix-db"].DefaultSchema.Tables.Any(t => t.Name == "prefix-table"), Is.True);

      var ignoreCollection = new IgnoreRuleCollection();
      _ = ignoreCollection.IgnoreTable("prefix-table").WhenDatabase("prefix-db");

      var processingResults = GetProcessedResults(settings, ignoreCollection);
      Assert.That(processingResults.Catalogs["prefix-db"].DefaultSchema.Tables.Any(t => t.Name == "prefix-table"), Is.False);
    }

    [Test]
    public void Test04()
    {
      var settings = CreateTestSettings();
      var original = settings.ExtractionResult;

      Assert.That(
        original.Catalogs["prefix-db-suffix"].Schemas["prefix-dbo"].Tables.Any(t => t.Name == "prefix-table"),
        Is.True);

      var ignoreCollection = new IgnoreRuleCollection();
      _ = ignoreCollection.IgnoreTable("prefix-table").WhenDatabase("prefix-db-suffix").WhenSchema("prefix-dbo");

      var processingResults = GetProcessedResults(settings, ignoreCollection);
      Assert.That(
        processingResults.Catalogs["prefix-db-suffix"].Schemas["prefix-dbo"].Tables.Any(t => t.Name == "prefix-table"),
        Is.False);
    }

    [Test]
    public void Test05()
    {
      var settings = CreateTestSettings();
      var original = settings.ExtractionResult;

      Assert.That(
        original.Catalogs[0].Schemas["dbo"].Tables.Any(t => t.Name.StartsWith("prefix-") || t.Name.EndsWith("-suffix")),
        Is.True);

      var ignoreCollection = new IgnoreRuleCollection();
      _ = ignoreCollection.IgnoreTable("prefix-*");
      _ = ignoreCollection.IgnoreTable("*-suffix");

      var processingResults = GetProcessedResults(settings, ignoreCollection);
      Assert.That(
        processingResults.Catalogs[0].DefaultSchema.Tables.Any(t => t.Name.StartsWith("prefix-") || t.Name.EndsWith("-suffix")),
        Is.False);
    }

    [Test]
    public void Test06()
    {
      var settings = CreateTestSettings();
      var original = settings.ExtractionResult;

      Assert.That(
        original.Catalogs[0].Schemas["dbo"].Tables.Any(t => t.Name.StartsWith("prefix-") || t.Name.EndsWith("-suffix")),
        Is.True);

      var ignoreCollection = new IgnoreRuleCollection();
      _ = ignoreCollection.IgnoreTable("prefix-*").WhenSchema("dbo");
      _ = ignoreCollection.IgnoreTable("*-suffix").WhenSchema("dbo");

      var processingResults = GetProcessedResults(settings, ignoreCollection);
      Assert.That(
        processingResults.Catalogs[0].Schemas["dbo"].Tables.Any(t => t.Name.StartsWith("prefix-") || t.Name.EndsWith("-suffix")),
        Is.False);
    }

    [Test]
    public void Test07()
    {
      var settings = CreateTestSettings();
      var original = settings.ExtractionResult;

      Assert.That(
        original.Catalogs["prefix-db"].DefaultSchema.Tables.Any(t => t.Name.StartsWith("prefix-") || t.Name.EndsWith("-suffix")),
        Is.True);

      var ignoreCollection = new IgnoreRuleCollection();
      _ = ignoreCollection.IgnoreTable("prefix-*").WhenDatabase("prefix-db");
      _ = ignoreCollection.IgnoreTable("*-suffix").WhenDatabase("prefix-db");

      var processingResults = GetProcessedResults(settings, ignoreCollection);
      Assert.That(
        processingResults.Catalogs["prefix-db"].DefaultSchema.Tables.Any(t => t.Name.StartsWith("prefix-") || t.Name.EndsWith("-suffix")),
        Is.False);
    }

    [Test]
    public void Test08()
    {
      var settings = CreateTestSettings();
      var original = settings.ExtractionResult;

      Assert.That(
        original.Catalogs["prefix-db-suffix"].Schemas["prefix-dbo"].Tables.Any(t => t.Name.StartsWith("prefix-") || t.Name.EndsWith("-suffix")),
        Is.True);

      var ignoreCollection = new IgnoreRuleCollection();
      _ = ignoreCollection.IgnoreTable("prefix-*").WhenDatabase("prefix-db-suffix").WhenSchema("prefix-dbo");
      _ = ignoreCollection.IgnoreTable("*-suffix").WhenDatabase("prefix-db-suffix").WhenSchema("prefix-dbo");

      var processingResults = GetProcessedResults(settings, ignoreCollection);
      Assert.That(
        processingResults.Catalogs["prefix-db-suffix"].Schemas["prefix-dbo"].Tables.Any(t => t.Name.StartsWith("prefix-") || t.Name.EndsWith("-suffix")),
        Is.False);
    }

    #endregion

    #region Ignore columns

    [Test]
    public void Test09()
    {
      var settings = CreateTestSettings();
      var original = settings.ExtractionResult;

      Assert.That(
        original.Catalogs["prefix-db"].Schemas["prefix-dbo"].Tables["prefix-table"].Columns.Any(c => c.Name.StartsWith("prefix-")),
        Is.True);
      Assert.That(
        original.Catalogs["db-suffix"].Schemas["dbo-suffix"].Tables["table-suffix"].Columns.Any(c => c.Name.EndsWith("-suffix")),
        Is.True);

      var ignoreCollection = new IgnoreRuleCollection();
      _ = ignoreCollection.IgnoreColumn("prefix-*").WhenTable("prefix-table").WhenSchema("prefix-dbo").WhenDatabase("prefix-db");
      _ = ignoreCollection.IgnoreColumn("*-suffix").WhenTable("table-suffix").WhenSchema("dbo-suffix").WhenDatabase("db-suffix");

      var processingResults = GetProcessedResults(settings, ignoreCollection);
      Assert.That(
        processingResults.Catalogs["prefix-db"].Schemas["prefix-dbo"].Tables["prefix-table"].Columns.Any(c => c.Name.StartsWith("prefix-")),
        Is.False);
      Assert.That(
        processingResults.Catalogs["db-suffix"].Schemas["dbo-suffix"].Tables["table-suffix"].Columns.Any(c => c.Name.EndsWith("-suffix")),
        Is.False);
    }

    [Test]
    public void Test10()
    {
      var settings = CreateTestSettings();
      var original = settings.ExtractionResult;

      Assert.That(
        original.Catalogs["prefix-db"].Schemas["prefix-dbo"].Tables
          .Where(t => t.Name.StartsWith("prefix-"))
          .SelectMany(t => t.Columns)
          .Any(c => c.Name.StartsWith("prefix-")),
        Is.True);
      Assert.That(
        original.Catalogs["db-suffix"].Schemas["dbo-suffix"].Tables
          .Where(t => t.Name.EndsWith("-suffix"))
          .SelectMany(t => t.Columns).Any(c => c.Name.EndsWith("-suffix")),
        Is.True);

      var ignoreCollection = new IgnoreRuleCollection();
      _ = ignoreCollection.IgnoreColumn("prefix-*").WhenTable("prefix-*").WhenSchema("prefix-dbo").WhenDatabase("prefix-db");
      _ = ignoreCollection.IgnoreColumn("*-suffix").WhenTable("*-suffix").WhenSchema("dbo-suffix").WhenDatabase("db-suffix");

      var processingResults = GetProcessedResults(settings, ignoreCollection);
      Assert.That(
        processingResults.Catalogs["prefix-db"].Schemas["prefix-dbo"].Tables
          .Where(t =>t.Name.StartsWith("prefix-"))
          .SelectMany(t=>t.Columns)
          .Any(c => c.Name.StartsWith("prefix-")),
        Is.False);
      Assert.That(
        processingResults.Catalogs["db-suffix"].Schemas["dbo-suffix"].Tables
          .Where(t => t.Name.EndsWith("-suffix"))
          .SelectMany(t =>t.Columns).Any(c => c.Name.EndsWith("-suffix")),
        Is.False);
    }

    [Test]
    public void Test11()
    {
      var settings = CreateTestSettings();
      var original = settings.ExtractionResult;

      Assert.That(
        original.Catalogs["prefix-db"].Schemas["prefix-dbo"].Tables
          .Where(t => t.Name.StartsWith("prefix-")).SelectMany(t => t.Columns)
          .Any(c => c.Name == "prefix-column"),
        Is.True);
      Assert.That(
        original.Catalogs["db-suffix"].Schemas["dbo-suffix"].Tables
          .Where(t => t.Name.EndsWith("-suffix")).SelectMany(t => t.Columns)
          .Any(c => c.Name == "column-suffix"),
        Is.True);

      var ignoreCollection = new IgnoreRuleCollection();
      _ = ignoreCollection.IgnoreColumn("prefix-column").WhenTable("prefix-*").WhenSchema("prefix-dbo").WhenDatabase("prefix-db");
      _ = ignoreCollection.IgnoreColumn("column-suffix").WhenTable("*-suffix").WhenSchema("dbo-suffix").WhenDatabase("db-suffix");

      var processingResults = GetProcessedResults(settings, ignoreCollection);
      Assert.That(
        processingResults.Catalogs["prefix-db"].Schemas["prefix-dbo"].Tables
          .Where(t => t.Name.StartsWith("prefix-")).SelectMany(t => t.Columns)
          .Any(c => c.Name == "prefix-column"),
        Is.False);
      Assert.That(
        processingResults.Catalogs["db-suffix"].Schemas["dbo-suffix"].Tables
          .Where(t => t.Name.EndsWith("-suffix")).SelectMany(t => t.Columns)
          .Any(c => c.Name == "column-suffix"),
        Is.False);
    }

    [Test]
    public void Test12()
    {
      var settings = CreateTestSettings();
      var original = settings.ExtractionResult;

      Assert.That(
        original.Catalogs["prefix-db"].Schemas["prefix-dbo"].Tables["prefix-table"].Columns.Any(c => c.Name == "prefix-column"),
        Is.True);
      Assert.That(
        original.Catalogs["db-suffix"].Schemas["dbo-suffix"].Tables["table-suffix"].Columns.Any(c => c.Name == "column-suffix"),
        Is.True);

      var ignoreCollection = new IgnoreRuleCollection();
      _ = ignoreCollection.IgnoreColumn("prefix-column").WhenTable("prefix-table").WhenSchema("prefix-dbo").WhenDatabase("prefix-db");
      _ = ignoreCollection.IgnoreColumn("column-suffix").WhenTable("table-suffix").WhenSchema("dbo-suffix").WhenDatabase("db-suffix");

      var processingResults = GetProcessedResults(settings, ignoreCollection);

      Assert.That(
        processingResults.Catalogs["prefix-db"].Schemas["prefix-dbo"].Tables["prefix-table"].Columns.Any(c => c.Name == "prefix-column"),
        Is.False);
      Assert.That(
        processingResults.Catalogs["db-suffix"].Schemas["dbo-suffix"].Tables["table-suffix"].Columns.Any(c => c.Name == "column-suffix"),
        Is.False);
    }

    [Test]
    public void Test13()
    {
      var settings = CreateTestSettings();
      var original = settings.ExtractionResult;

      Assert.That(
        original.Catalogs[0].DefaultSchema.Tables
          .Where(t => t.Name.StartsWith("prefix-"))
          .SelectMany(t => t.Columns)
          .Any(c => c.Name.StartsWith("prefix-")),
        Is.True);

      var ignoreCollection = new IgnoreRuleCollection();
      _ = ignoreCollection.IgnoreColumn("prefix-*").WhenTable("prefix-*");

      var processingResults = GetProcessedResults(settings, ignoreCollection);
      Assert.That(
        processingResults.Catalogs[0].DefaultSchema.Tables
          .Where(t=>t.Name.StartsWith("prefix-"))
          .SelectMany(t=>t.Columns)
          .Any(c => c.Name.StartsWith("prefix-")),
        Is.False);
    }

    [Test]
    public void Test14()
    {
      var settings = CreateTestSettings();
      var original = settings.ExtractionResult;

      Assert.That(
        original.Catalogs[0].DefaultSchema.Tables["prefix-table"].Columns
          .Any(c => c.Name.StartsWith("prefix-")),
        Is.True);

      var ignoreCollection = new IgnoreRuleCollection();
      _ = ignoreCollection.IgnoreColumn("prefix-*").WhenTable("prefix-table");
      _ = ignoreCollection.IgnoreColumn("*-suffix").WhenTable("table-suffix");

      var processingResults = GetProcessedResults(settings, ignoreCollection);
      Assert.That(
        processingResults.Catalogs[0].DefaultSchema.Tables["prefix-table"].Columns
          .Any(c => c.Name.StartsWith("prefix-")),
        Is.False);
    }

    [Test]
    public void Test15()
    {
      var settings = CreateTestSettings();
      var original = settings.ExtractionResult;

      Assert.That(
        original.Catalogs[0].DefaultSchema.Tables
          .Where(t => t.Name.StartsWith("prefix-"))
          .SelectMany(t => t.Columns)
          .Any(c => c.Name == "prefix-column"),
        Is.True);

      var ignoreCollection = new IgnoreRuleCollection();
      _ = ignoreCollection.IgnoreColumn("prefix-column").WhenTable("prefix-*");
      _ = ignoreCollection.IgnoreColumn("column-suffix").WhenTable("*-suffix");

      var processingResults = GetProcessedResults(settings, ignoreCollection);
      Assert.That(
        processingResults.Catalogs[0].DefaultSchema.Tables
          .Where(t => t.Name.StartsWith("prefix-"))
          .SelectMany(t => t.Columns)
          .Any(c => c.Name == "prefix-column"),
        Is.False);
    }

    [Test]
    public void Test16()
    {
      var settings = CreateTestSettings();
      var original = settings.ExtractionResult;

      Assert.That(
        original.Catalogs[0].DefaultSchema.Tables
          .Where(t => t.Name == "prefix-table")
          .SelectMany(t => t.Columns)
          .Any(c => c.Name == "prefix-column"),
        Is.True);

      var ignoreCollection = new IgnoreRuleCollection();
      _ = ignoreCollection.IgnoreColumn("prefix-column").WhenTable("prefix-table");

      var processingResults = GetProcessedResults(settings, ignoreCollection);
      Assert.That(
        processingResults.Catalogs[0].DefaultSchema.Tables
          .Where(t => t.Name == "prefix-table")
          .SelectMany(t => t.Columns)
          .Any(c => c.Name == "prefix-column"),
        Is.False);
    }

    #endregion

    #region Ignore indexes

    [Test]
    public void Test17()
    {
      var settings = CreateTestSettings();
      var original = settings.ExtractionResult;

      Assert.That(
        original.Catalogs["prefix-db"].Schemas["prefix-dbo"].Tables["prefix-table"].Indexes.Any(c => c.Name.StartsWith("ix_pref-")),
        Is.True);
      Assert.That(
        original.Catalogs["db-suffix"].Schemas["dbo-suffix"].Tables["table-suffix"].Indexes.Any(c => c.Name.EndsWith("-suf")),
        Is.True);

      var ignoreCollection = new IgnoreRuleCollection();
      _ = ignoreCollection.IgnoreIndex("ix_pref-*").WhenTable("prefix-table").WhenSchema("prefix-dbo").WhenDatabase("prefix-db");
      _ = ignoreCollection.IgnoreIndex("*-suf").WhenTable("table-suffix").WhenSchema("dbo-suffix").WhenDatabase("db-suffix");

      var processingResults = GetProcessedResults(settings, ignoreCollection);
      Assert.That(
        processingResults.Catalogs["prefix-db"].Schemas["prefix-dbo"].Tables["prefix-table"].Indexes.Any(c => c.Name.StartsWith("ix_pref-")),
        Is.False);
      Assert.That(
        processingResults.Catalogs["db-suffix"].Schemas["dbo-suffix"].Tables["table-suffix"].Indexes.Any(c => c.Name.EndsWith("-suf")),
        Is.False);
    }

    [Test]
    public void Test18()
    {
      var settings = CreateTestSettings();
      var original = settings.ExtractionResult;

      Assert.That(
        original.Catalogs["prefix-db"].Schemas["prefix-dbo"].Tables
          .Where(t => t.Name.StartsWith("prefix-"))
          .SelectMany(t => t.Indexes)
          .Any(c => c.Name.StartsWith("ix_pref-")),
        Is.True);
      Assert.That(
        original.Catalogs["db-suffix"].Schemas["dbo-suffix"].Tables
          .Where(t => t.Name.EndsWith("-suffix"))
          .SelectMany(t => t.Indexes)
          .Any(c => c.Name.EndsWith("-suf")),
        Is.True);

      var ignoreCollection = new IgnoreRuleCollection();
      _ = ignoreCollection.IgnoreIndex("ix_pref-*").WhenTable("prefix-*").WhenSchema("prefix-dbo").WhenDatabase("prefix-db");
      _ = ignoreCollection.IgnoreIndex("*-suf").WhenTable("*-suffix").WhenSchema("dbo-suffix").WhenDatabase("db-suffix");

      var processingResults = GetProcessedResults(settings, ignoreCollection);
      Assert.That(
        processingResults.Catalogs["prefix-db"].Schemas["prefix-dbo"].Tables
          .Where(t => t.Name.StartsWith("prefix-"))
          .SelectMany(t => t.Indexes)
          .Any(c => c.Name.StartsWith("ix_pref-")),
        Is.False);
      Assert.That(
        processingResults.Catalogs["db-suffix"].Schemas["dbo-suffix"].Tables
          .Where(t => t.Name.EndsWith("-suffix"))
          .SelectMany(t => t.Indexes)
          .Any(c => c.Name.EndsWith("-suf")),
        Is.False);
    }

    [Test]
    public void Test19()
    {
      var settings = CreateTestSettings();
      var original = settings.ExtractionResult;

      Assert.That(
        original.Catalogs["prefix-db"].Schemas["prefix-dbo"].Tables
          .Where(t => t.Name.StartsWith("prefix-")).SelectMany(t => t.Indexes)
          .Any(c => c.Name == "ix_pref-index"),
        Is.True);
      Assert.That(
        original.Catalogs["db-suffix"].Schemas["dbo-suffix"].Tables
          .Where(t => t.Name.EndsWith("-suffix")).SelectMany(t => t.Indexes)
          .Any(c => c.Name == "ix_index-suf"),
        Is.True);

      var ignoreCollection = new IgnoreRuleCollection();
      _ = ignoreCollection.IgnoreIndex("ix_pref-index").WhenTable("prefix-*").WhenSchema("prefix-dbo").WhenDatabase("prefix-db");
      _ = ignoreCollection.IgnoreIndex("ix_index-suf").WhenTable("*-suffix").WhenSchema("dbo-suffix").WhenDatabase("db-suffix");

      var processingResults = GetProcessedResults(settings, ignoreCollection);
      Assert.That(
        processingResults.Catalogs["prefix-db"].Schemas["prefix-dbo"].Tables
          .Where(t => t.Name.StartsWith("prefix-")).SelectMany(t => t.Indexes)
          .Any(c => c.Name == "ix_pref-index"),
        Is.False);
      Assert.That(
        processingResults.Catalogs["db-suffix"].Schemas["dbo-suffix"].Tables
          .Where(t => t.Name.EndsWith("-suffix")).SelectMany(t => t.Indexes)
          .Any(c => c.Name == "ix_index-suf"),
        Is.False);
    }

    [Test]
    public void Test20()
    {
      var settings = CreateTestSettings();
      var original = settings.ExtractionResult;

      Assert.That(
        original.Catalogs["prefix-db"].Schemas["prefix-dbo"].Tables["prefix-table"].Indexes.Any(c => c.Name == "ix_pref-index"),
        Is.True);
      Assert.That(
        original.Catalogs["db-suffix"].Schemas["dbo-suffix"].Tables["table-suffix"].Indexes.Any(c => c.Name == "ix_index-suf"),
        Is.True);

      var ignoreCollection = new IgnoreRuleCollection();
      _ = ignoreCollection.IgnoreIndex("ix_pref-index").WhenTable("prefix-table").WhenSchema("prefix-dbo").WhenDatabase("prefix-db");
      _ = ignoreCollection.IgnoreIndex("ix_index-suf").WhenTable("table-suffix").WhenSchema("dbo-suffix").WhenDatabase("db-suffix");

      var processingResults = GetProcessedResults(settings, ignoreCollection);

      Assert.That(
        processingResults.Catalogs["prefix-db"].Schemas["prefix-dbo"].Tables["prefix-table"].Indexes.Any(c => c.Name == "ix_pref-index"),
        Is.False);
      Assert.That(
        processingResults.Catalogs["db-suffix"].Schemas["dbo-suffix"].Tables["table-suffix"].Indexes.Any(c => c.Name == "ix_index-suf"),
        Is.False);
    }

    [Test]
    public void Test21()
    {
      var settings = CreateTestSettings();
      var original = settings.ExtractionResult;

      Assert.That(
        original.Catalogs[0].DefaultSchema.Tables
          .Where(t => t.Name.StartsWith("prefix-"))
          .SelectMany(t => t.Indexes)
          .Any(c => c.Name.StartsWith("ix_pref-")),
        Is.True);

      var ignoreCollection = new IgnoreRuleCollection();
      _ = ignoreCollection.IgnoreIndex("ix_pref-*").WhenTable("prefix-*");

      var processingResults = GetProcessedResults(settings, ignoreCollection);
      Assert.That(
        processingResults.Catalogs[0].DefaultSchema.Tables
          .Where(t => t.Name.StartsWith("prefix-"))
          .SelectMany(t => t.Indexes)
          .Any(c => c.Name.StartsWith("ix_pref-")),
        Is.False);
    }

    [Test]
    public void Test22()
    {
      var settings = CreateTestSettings();
      var original = settings.ExtractionResult;

      Assert.That(
        original.Catalogs[0].DefaultSchema.Tables["prefix-table"].Indexes
          .Any(c => c.Name.StartsWith("ix_pref-")),
        Is.True);

      var ignoreCollection = new IgnoreRuleCollection();
      _ = ignoreCollection.IgnoreIndex("ix_pref-*").WhenTable("prefix-table");

      var processingResults = GetProcessedResults(settings, ignoreCollection);
      Assert.That(
        processingResults.Catalogs[0].DefaultSchema.Tables["prefix-table"].Indexes
          .Any(c => c.Name.StartsWith("ix_pref-")),
        Is.False);
    }

    [Test]
    public void Test23()
    {
      var settings = CreateTestSettings();
      var original = settings.ExtractionResult;

      Assert.That(
        original.Catalogs[0].DefaultSchema.Tables
          .Where(t => t.Name.StartsWith("prefix-"))
          .SelectMany(t => t.Indexes)
          .Any(c => c.Name == "ix_pref-index-suf"),
        Is.True);

      var ignoreCollection = new IgnoreRuleCollection();
      _ = ignoreCollection.IgnoreIndex("ix_pref-index-suf").WhenTable("prefix-*");

      var processingResults = GetProcessedResults(settings, ignoreCollection);
      Assert.That(
        processingResults.Catalogs[0].DefaultSchema.Tables
          .Where(t => t.Name.StartsWith("prefix-"))
          .SelectMany(t => t.Indexes)
          .Any(c => c.Name == "ix_pref-index-suf"),
        Is.False);
    }

    [Test]
    public void Test24()
    {
      var settings = CreateTestSettings();
      var original = settings.ExtractionResult;

      Assert.That(
        original.Catalogs[0].DefaultSchema.Tables
          .Where(t => t.Name == "prefix-table")
          .SelectMany(t => t.Indexes)
          .Any(c => c.Name == "ix_pref-index-suf"),
        Is.True);


      var ignoreCollection = new IgnoreRuleCollection();
      _ = ignoreCollection.IgnoreIndex("ix_pref-index-suf").WhenTable("prefix-table");

      var processingResults = GetProcessedResults(settings, ignoreCollection);
      Assert.That(
        processingResults.Catalogs[0].DefaultSchema.Tables
          .Where(t => t.Name == "prefix-table")
          .SelectMany(t => t.Indexes)
          .Any(c => c.Name == "ix_pref-index-suf"),
        Is.False);
    }

    #endregion

    private SchemaExtractionResult GetProcessedResults(in TestSetting settings, IgnoreRuleCollection rules)
    {
      var configuration = settings.DomainConfiguration;
      configuration.IgnoreRules = rules;
      return new IgnoreRulesHandler(settings.ExtractionResult, settings.DomainConfiguration, settings.MappingResolver)
        .Handle();
    }

    private static TestSetting CreateTestSettings()
    {
      var nodeConfiguration = new NodeConfiguration();
      var configuration = DomainConfigurationFactory.Create();
      configuration.DefaultDatabase = "DO-Tests";
      configuration.DefaultSchema = "dbo";
      configuration.Databases.Add(new DatabaseConfiguration("DO-Tests") { RealName = "DO-Tests" });
      configuration.Databases.Add(new DatabaseConfiguration("prefix-db") { RealName = "prefix-db" });
      configuration.Databases.Add(new DatabaseConfiguration("prefix-db-suffix") { RealName = "prefix-db-suffix" });
      configuration.Databases.Add(new DatabaseConfiguration("db-suffix") { RealName = "db-suffix" });

      var extResult = new SqlExtractionResult();
      extResult.Catalogs.Add(CreateDefaultCatalogAndSchema());
      extResult.Catalogs.Add(CreateCatalogTemplate("prefix-db"));
      extResult.Catalogs.Add(CreateCatalogTemplate("prefix-db-suffix"));
      extResult.Catalogs.Add(CreateCatalogTemplate("db-suffix"));

      var resolver = MappingResolver.Create(configuration, nodeConfiguration, new Sql.Info.DefaultSchemaInfo("DO-Tests", "dbo"));

      return new TestSetting {
        ExtractionResult = new SchemaExtractionResult(extResult),
        DomainConfiguration = configuration,
        MappingResolver = resolver
      };
    }

    private static Catalog CreateDefaultCatalogAndSchema()
    {
      var catalog = new Catalog("DO-Tests");
      var schema = catalog.CreateSchema("dbo");
      catalog.DefaultSchema = schema;

      var table = schema.CreateTable("prefix-table");
      var column = table.CreateColumn("Id", new SqlValueType(SqlType.Int64));
      column.IsNullable = false;
      _ = table.CreatePrimaryKey("pk_prefix-table-suffix", column);

      column = table.CreateColumn("Name", new SqlValueType(SqlType.VarChar, 100));
      column.IsNullable = false;

      _ = table.CreateColumn("prefix-column", new SqlValueType(SqlType.Int64));
      _ = table.CreateColumn("prefix-column-suffix", new SqlValueType(SqlType.Int64));
      _ = table.CreateColumn("column-suffix", new SqlValueType(SqlType.Int64));

      //normal index
      var index = table.CreateIndex("ix_normal-index");
      column = table.CreateColumn("normal-idx-column1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("normal-idx-column2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("normal-idx-incl-column1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("normal-idx-incl-column2", new SqlValueType(SqlType.VarChar, 100)));

      //ignored index1
      index = table.CreateIndex("ix_pref-index");
      column = table.CreateColumn("pref-idx-col1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("pref-idx-col2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-incl-col1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-incl-col2", new SqlValueType(SqlType.VarChar, 100)));

      // ignored index2
      index = table.CreateIndex("ix_pref-index-suf");
      column = table.CreateColumn("pref-idx-suf-col1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("pref-idx-suf-col2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-suf-incl-col1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-suf-incl-col2", new SqlValueType(SqlType.VarChar, 100)));

      // ignored index3
      index = table.CreateIndex("ix_index-suf");
      column = table.CreateColumn("idx-suf-col1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("idx-suf-col2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("idx-suf-incl-col1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("idx-suf-incl-col2", new SqlValueType(SqlType.VarChar, 100)));

      return catalog;
    }

    private static Catalog CreateCatalogTemplate(string name)
    {
      var catalog = new Catalog(name);
      var schema = catalog.CreateSchema("prefix-dbo");

      #region catalog1-schema1-table1

      var table = schema.CreateTable("prefix-table");
      var column = table.CreateColumn("Id", new SqlValueType(SqlType.Int64));
      column.IsNullable = false;
      _ = table.CreatePrimaryKey("pk_prefix-table-suffix", column);

      column = table.CreateColumn("Name", new SqlValueType(SqlType.VarChar, 100));
      column.IsNullable = false;

      _ = table.CreateColumn("prefix-column", new SqlValueType(SqlType.Int64));
      _ = table.CreateColumn("prefix-column-suffix", new SqlValueType(SqlType.Int64));
      _ = table.CreateColumn("column-suffix", new SqlValueType(SqlType.Int64));

      //normal index
      var index = table.CreateIndex("ix_normal-index");
      column = table.CreateColumn("normal-idx-column1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("normal-idx-column2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("normal-idx-incl-column1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("normal-idx-incl-column2", new SqlValueType(SqlType.VarChar, 100)));

      //ignored index1
      index = table.CreateIndex("ix_pref-index");
      column = table.CreateColumn("pref-idx-col1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("pref-idx-col2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-incl-col1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-incl-col2", new SqlValueType(SqlType.VarChar, 100)));

      // ignored index2
      index = table.CreateIndex("ix_pref-index-suf");
      column = table.CreateColumn("pref-idx-suf-col1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("pref-idx-suf-col2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-suf-incl-col1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-suf-incl-col2", new SqlValueType(SqlType.VarChar, 100)));

      // ignored index3
      index = table.CreateIndex("ix_index-suf");
      column = table.CreateColumn("idx-suf-col1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("idx-suf-col2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("idx-suf-incl-col1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("idx-suf-incl-col2", new SqlValueType(SqlType.VarChar, 100)));

      #endregion

      #region catalog1-schema1-table2

      table = schema.CreateTable("prefix-table-suffix");
      column = table.CreateColumn("Id", new SqlValueType(SqlType.Int64));
      column.IsNullable = false;
      _ = table.CreatePrimaryKey("pk_prefix-table-suffix", column);

      column = table.CreateColumn("Name", new SqlValueType(SqlType.VarChar, 100));
      column.IsNullable = false;

      _ = table.CreateColumn("prefix-column", new SqlValueType(SqlType.Int64));
      _ = table.CreateColumn("prefix-column-suffix", new SqlValueType(SqlType.Int64));
      _ = table.CreateColumn("column-suffix", new SqlValueType(SqlType.Int64));

      //normal index
      index = table.CreateIndex("ix_normal-index");
      column = table.CreateColumn("normal-idx-column1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("normal-idx-column2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("normal-idx-incl-column1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("normal-idx-incl-column2", new SqlValueType(SqlType.VarChar, 100)));

      //ignored index1
      index = table.CreateIndex("ix_pref-index");
      column = table.CreateColumn("pref-idx-col1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("pref-idx-col2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-incl-col1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-incl-col2", new SqlValueType(SqlType.VarChar, 100)));

      // ignored index2
      index = table.CreateIndex("ix_pref-index-suf");
      column = table.CreateColumn("pref-idx-suf-col1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("pref-idx-suf-col2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-suf-incl-col1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-suf-incl-col2", new SqlValueType(SqlType.VarChar, 100)));

      // ignored index3
      index = table.CreateIndex("ix_index-suf");
      column = table.CreateColumn("idx-suf-col1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("idx-suf-col2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("idx-suf-incl-col1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("idx-suf-incl-col2", new SqlValueType(SqlType.VarChar, 100)));

      #endregion

      #region catalog1-schema1-table3

      table = schema.CreateTable("table-suffix");
      column = table.CreateColumn("Id", new SqlValueType(SqlType.Int64));
      column.IsNullable = false;
      _ = table.CreatePrimaryKey("pk_prefix-table-suffix", column);

      column = table.CreateColumn("Name", new SqlValueType(SqlType.VarChar, 100));
      column.IsNullable = false;

      _ = table.CreateColumn("prefix-column", new SqlValueType(SqlType.Int64));
      _ = table.CreateColumn("prefix-column-suffix", new SqlValueType(SqlType.Int64));
      _ = table.CreateColumn("column-suffix", new SqlValueType(SqlType.Int64));

      //normal index
      index = table.CreateIndex("ix_normal-index");
      column = table.CreateColumn("normal-idx-column1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("normal-idx-column2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("normal-idx-incl-column1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("normal-idx-incl-column2", new SqlValueType(SqlType.VarChar, 100)));

      //ignored index1
      index = table.CreateIndex("ix_pref-index");
      column = table.CreateColumn("pref-idx-col1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("pref-idx-col2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-incl-col1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-incl-col2", new SqlValueType(SqlType.VarChar, 100)));

      // ignored index2
      index = table.CreateIndex("ix_pref-index-suf");
      column = table.CreateColumn("pref-idx-suf-col1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("pref-idx-suf-col2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-suf-incl-col1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-suf-incl-col2", new SqlValueType(SqlType.VarChar, 100)));

      // ignored index3
      index = table.CreateIndex("ix_index-suf");
      column = table.CreateColumn("idx-suf-col1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("idx-suf-col2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("idx-suf-incl-col1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("idx-suf-incl-col2", new SqlValueType(SqlType.VarChar, 100)));

      #endregion

      schema = catalog.CreateSchema("dbo");
      catalog.DefaultSchema = schema;

      #region catalog1-schema2-table1

      table = schema.CreateTable("prefix-table");
      column = table.CreateColumn("Id", new SqlValueType(SqlType.Int64));
      column.IsNullable = false;
      _ = table.CreatePrimaryKey("pk_prefix-table-suffix", column);

      column = table.CreateColumn("Name", new SqlValueType(SqlType.VarChar, 100));
      column.IsNullable = false;

      _ = table.CreateColumn("prefix-column", new SqlValueType(SqlType.Int64));
      _ = table.CreateColumn("prefix-column-suffix", new SqlValueType(SqlType.Int64));
      _ = table.CreateColumn("column-suffix", new SqlValueType(SqlType.Int64));

      //normal index
      index = table.CreateIndex("ix_normal-index");
      column = table.CreateColumn("normal-idx-column1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("normal-idx-column2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("normal-idx-incl-column1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("normal-idx-incl-column2", new SqlValueType(SqlType.VarChar, 100)));

      //ignored index1
      index = table.CreateIndex("ix_pref-index");
      column = table.CreateColumn("pref-idx-col1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("pref-idx-col2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-incl-col1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-incl-col2", new SqlValueType(SqlType.VarChar, 100)));

      // ignored index2
      index = table.CreateIndex("ix_pref-index-suf");
      column = table.CreateColumn("pref-idx-suf-col1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("pref-idx-suf-col2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-suf-incl-col1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-suf-incl-col2", new SqlValueType(SqlType.VarChar, 100)));

      // ignored index3
      index = table.CreateIndex("ix_index-suf");
      column = table.CreateColumn("idx-suf-col1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("idx-suf-col2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("idx-suf-incl-col1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("idx-suf-incl-col2", new SqlValueType(SqlType.VarChar, 100)));

      #endregion

      #region catalog1-schema2-table2

      table = schema.CreateTable("prefix-table-suffix");
      column = table.CreateColumn("Id", new SqlValueType(SqlType.Int64));
      column.IsNullable = false;
      _ = table.CreatePrimaryKey("pk_prefix-table-suffix", column);

      column = table.CreateColumn("Name", new SqlValueType(SqlType.VarChar, 100));
      column.IsNullable = false;

      _ = table.CreateColumn("prefix-column", new SqlValueType(SqlType.Int64));
      _ = table.CreateColumn("prefix-column-suffix", new SqlValueType(SqlType.Int64));
      _ = table.CreateColumn("column-suffix", new SqlValueType(SqlType.Int64));

      //normal index
      index = table.CreateIndex("ix_normal-index");
      column = table.CreateColumn("normal-idx-column1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("normal-idx-column2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("normal-idx-incl-column1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("normal-idx-incl-column2", new SqlValueType(SqlType.VarChar, 100)));

      //ignored index1
      index = table.CreateIndex("ix_pref-index");
      column = table.CreateColumn("pref-idx-col1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("pref-idx-col2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-incl-col1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-incl-col2", new SqlValueType(SqlType.VarChar, 100)));

      // ignored index2
      index = table.CreateIndex("ix_pref-index-suf");
      column = table.CreateColumn("pref-idx-suf-col1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("pref-idx-suf-col2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-suf-incl-col1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-suf-incl-col2", new SqlValueType(SqlType.VarChar, 100)));

      // ignored index3
      index = table.CreateIndex("ix_index-suf");
      column = table.CreateColumn("idx-suf-col1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("idx-suf-col2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("idx-suf-incl-col1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("idx-suf-incl-col2", new SqlValueType(SqlType.VarChar, 100)));

      #endregion

      #region catalog1-schema2-table3

      table = schema.CreateTable("table-suffix");
      column = table.CreateColumn("Id", new SqlValueType(SqlType.Int64));
      column.IsNullable = false;
      _ = table.CreatePrimaryKey("pk_prefix-table-suffix", column);

      column = table.CreateColumn("Name", new SqlValueType(SqlType.VarChar, 100));
      column.IsNullable = false;

      _ = table.CreateColumn("prefix-column", new SqlValueType(SqlType.Int64));
      _ = table.CreateColumn("prefix-column-suffix", new SqlValueType(SqlType.Int64));
      _ = table.CreateColumn("column-suffix", new SqlValueType(SqlType.Int64));

      //normal index
      index = table.CreateIndex("ix_normal-index");
      column = table.CreateColumn("normal-idx-column1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("normal-idx-column2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("normal-idx-incl-column1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("normal-idx-incl-column2", new SqlValueType(SqlType.VarChar, 100)));

      //ignored index1
      index = table.CreateIndex("ix_pref-index");
      column = table.CreateColumn("pref-idx-col1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("pref-idx-col2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-incl-col1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-incl-col2", new SqlValueType(SqlType.VarChar, 100)));

      // ignored index2
      index = table.CreateIndex("ix_pref-index-suf");
      column = table.CreateColumn("pref-idx-suf-col1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("pref-idx-suf-col2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-suf-incl-col1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-suf-incl-col2", new SqlValueType(SqlType.VarChar, 100)));

      // ignored index3
      index = table.CreateIndex("ix_index-suf");
      column = table.CreateColumn("idx-suf-col1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("idx-suf-col2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("idx-suf-incl-col1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("idx-suf-incl-col2", new SqlValueType(SqlType.VarChar, 100)));

      #endregion

      schema = catalog.CreateSchema("dbo-suffix");

      #region catalog1-schema3-table1

      table = schema.CreateTable("prefix-table");
      column = table.CreateColumn("Id", new SqlValueType(SqlType.Int64));
      column.IsNullable = false;
      _ = table.CreatePrimaryKey("pk_prefix-table-suffix", column);

      column = table.CreateColumn("Name", new SqlValueType(SqlType.VarChar, 100));
      column.IsNullable = false;

      _ = table.CreateColumn("prefix-column", new SqlValueType(SqlType.Int64));
      _ = table.CreateColumn("prefix-column-suffix", new SqlValueType(SqlType.Int64));
      _ = table.CreateColumn("column-suffix", new SqlValueType(SqlType.Int64));

      //normal index
      index = table.CreateIndex("ix_normal-index");
      column = table.CreateColumn("normal-idx-column1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("normal-idx-column2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("normal-idx-incl-column1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("normal-idx-incl-column2", new SqlValueType(SqlType.VarChar, 100)));

      //ignored index1
      index = table.CreateIndex("ix_pref-index");
      column = table.CreateColumn("pref-idx-col1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("pref-idx-col2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-incl-col1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-incl-col2", new SqlValueType(SqlType.VarChar, 100)));

      // ignored index2
      index = table.CreateIndex("ix_pref-index-suf");
      column = table.CreateColumn("pref-idx-suf-col1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("pref-idx-suf-col2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-suf-incl-col1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-suf-incl-col2", new SqlValueType(SqlType.VarChar, 100)));

      // ignored index3
      index = table.CreateIndex("ix_index-suf");
      column = table.CreateColumn("idx-suf-col1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("idx-suf-col2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("idx-suf-incl-col1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("idx-suf-incl-col2", new SqlValueType(SqlType.VarChar, 100)));

      #endregion

      #region catalog1-schema3-table2

      table = schema.CreateTable("prefix-table-suffix");
      column = table.CreateColumn("Id", new SqlValueType(SqlType.Int64));
      column.IsNullable = false;
      _ = table.CreatePrimaryKey("pk_prefix-table-suffix", column);

      column = table.CreateColumn("Name", new SqlValueType(SqlType.VarChar, 100));
      column.IsNullable = false;

      _ = table.CreateColumn("prefix-column", new SqlValueType(SqlType.Int64));
      _ = table.CreateColumn("prefix-column-suffix", new SqlValueType(SqlType.Int64));
      _ = table.CreateColumn("column-suffix", new SqlValueType(SqlType.Int64));

      //normal index
      index = table.CreateIndex("ix_normal-index");
      column = table.CreateColumn("normal-idx-column1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("normal-idx-column2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("normal-idx-incl-column1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("normal-idx-incl-column2", new SqlValueType(SqlType.VarChar, 100)));

      //ignored index1
      index = table.CreateIndex("ix_pref-index");
      column = table.CreateColumn("pref-idx-col1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("pref-idx-col2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-incl-col1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-incl-col2", new SqlValueType(SqlType.VarChar, 100)));

      // ignored index2
      index = table.CreateIndex("ix_pref-index-suf");
      column = table.CreateColumn("pref-idx-suf-col1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("pref-idx-suf-col2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-suf-incl-col1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-suf-incl-col2", new SqlValueType(SqlType.VarChar, 100)));

      // ignored index3
      index = table.CreateIndex("ix_index-suf");
      column = table.CreateColumn("idx-suf-col1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("idx-suf-col2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("idx-suf-incl-col1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("idx-suf-incl-col2", new SqlValueType(SqlType.VarChar, 100)));

      #endregion

      #region catalog1-schema3-table3

      table = schema.CreateTable("table-suffix");
      column = table.CreateColumn("Id", new SqlValueType(SqlType.Int64));
      column.IsNullable = false;
      _ = table.CreatePrimaryKey("pk_prefix-table-suffix", column);

      column = table.CreateColumn("Name", new SqlValueType(SqlType.VarChar, 100));
      column.IsNullable = false;

      _ = table.CreateColumn("prefix-column", new SqlValueType(SqlType.Int64));
      _ = table.CreateColumn("prefix-column-suffix", new SqlValueType(SqlType.Int64));
      _ = table.CreateColumn("column-suffix", new SqlValueType(SqlType.Int64));

      //normal index
      index = table.CreateIndex("ix_normal-index");
      column = table.CreateColumn("normal-idx-column1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("normal-idx-column2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("normal-idx-incl-column1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("normal-idx-incl-column2", new SqlValueType(SqlType.VarChar, 100)));

      //ignored index1
      index = table.CreateIndex("ix_pref-index");
      column = table.CreateColumn("pref-idx-col1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("pref-idx-col2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-incl-col1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-incl-col2", new SqlValueType(SqlType.VarChar, 100)));

      // ignored index2
      index = table.CreateIndex("ix_pref-index-suf");
      column = table.CreateColumn("pref-idx-suf-col1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("pref-idx-suf-col2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-suf-incl-col1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("pref-idx-suf-incl-col2", new SqlValueType(SqlType.VarChar, 100)));

      // ignored index3
      index = table.CreateIndex("ix_index-suf");
      column = table.CreateColumn("idx-suf-col1", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      column = table.CreateColumn("idx-suf-col2", new SqlValueType(SqlType.Int64));
      _ = index.CreateIndexColumn(column);

      index.NonkeyColumns.Add(table.CreateColumn("idx-suf-incl-col1", new SqlValueType(SqlType.VarChar, 100)));
      index.NonkeyColumns.Add(table.CreateColumn("idx-suf-incl-col2", new SqlValueType(SqlType.VarChar, 100)));

      #endregion

      return catalog;
    }
  }
}
