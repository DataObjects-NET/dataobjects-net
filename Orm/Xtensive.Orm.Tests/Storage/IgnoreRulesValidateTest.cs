// Copyright (C) 2013-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2013.08.16

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Sql;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Model;
using Model1 = Xtensive.Orm.Tests.Storage.IgnoreRulesValidateModel1;
using ignorablePart = Xtensive.Orm.Tests.Storage.IgnoreRulesValidateModel2;
using Model2 = Xtensive.Orm.Tests.Storage.IgnoreRulesValidateModel3;
using Model3 = Xtensive.Orm.Tests.Storage.IgnoreRulesValidateModel4;
using Model4 = Xtensive.Orm.Tests.Storage.IgnoreRulesValidateModel5;
using Model5 = Xtensive.Orm.Tests.Storage.IgnoreRulesValidateModel6;
using Model6 = Xtensive.Orm.Tests.Storage.IgnoreRulesValidateModel7;

namespace Xtensive.Orm.Tests.Storage.IgnoreRulesValidateModel1
{
  public abstract class Person : Entity
  {
    [Field]
    public string FirstName { get; set; }

    [Field]
    public string LastName { get; set; }

    [Field]
    public DateTime Birthday { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Person
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public EntitySet<Order> Orders { get; set; }

  }

  [HierarchyRoot]
  public class Author : Person
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public EntitySet<Book> Books { get; set; }
  }

  [HierarchyRoot]
  public class Book : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field(Length = 50)]
    public string Title { get; set; }

    [Field(Length = 13)]
    public string ISBN { get; set; }

    [Field]
    [Association(PairTo = "Books")]
    public EntitySet<Author> Authors { get; set; }

    [Field]
    public EntitySet<Order> Orders { get; set; }
  }

  [HierarchyRoot]
  [Index("Book", "Customer")]
  public class Order : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    [Association(PairTo = "Orders")]
    public Book Book { get; set; }

    [Field]
    [Association(PairTo = "Orders")]
    public Customer Customer { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage.IgnoreRulesValidateModel2
{
  [HierarchyRoot]
  public class IgnoredTable : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public string SomeField { get; set; }

    [Field]
    public Model1.Book Book { get; set; }

    [Field]
    public Model1.Customer Customer { get; set; }

    [Field]
    public Model1.Order Order { get; set; }

    [Field]
    [Association(PairTo = "IgnoredColumn")]
    public Model1.Author Author { get; set; }
  }

  public class FieldInjector : IModule
  {

    public void OnBuilt(Domain domain)
    {
    }

    public void OnDefinitionsBuilt(Building.BuildingContext context, DomainModelDef model)
    {
      var orderType = model.Types[typeof (Model1.Order)];
      var field = orderType.DefineField("SomeIgnoredField", typeof (string));
      field.Length = 100;

      if (orderType.Indexes.TryGetValue("IndexName", out var indexDef)) {
        var fieldsBefore = indexDef.KeyFields.ToList();
        indexDef.KeyFields.Clear();
        indexDef.KeyFields.Add("SomeIgnoredField", Direction.Positive);
        foreach (var keyValuePair in fieldsBefore) {
          indexDef.KeyFields.Add(keyValuePair.Key, keyValuePair.Value);
        }
      }

      var ignoredTable = model.Types[typeof (IgnoredTable)];
      if (ignoredTable == null) {
        return;
      }

      var authorType = model.Types[typeof (Model1.Author)];
      field = authorType.DefineField("IgnoredColumn", ignoredTable.UnderlyingType);
      field.IsEntity = true;
    }
  }
}

namespace Xtensive.Orm.Tests.Storage.IgnoreRulesValidateModel3
{
  public abstract class Person : Entity
  {
    [Field]
    public string FirstName { get; set; }

    [Field]
    public string LastName { get; set; }

    [Field]
    public DateTime Birthday { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Person
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public EntitySet<Order> Orders { get; set; }

  }

  [HierarchyRoot]
  public class Author : Person
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public EntitySet<Book> Books { get; set; }
  }

  [HierarchyRoot]
  public class Book : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field(Length = 50)]
    public string Title { get; set; }

    [Field(Length = 13)]
    public string ISBN { get; set; }

    [Field]
    [Association(PairTo = "Books")]
    public EntitySet<Author> Authors { get; set; }

    [Field]
    public EntitySet<Order> Orders { get; set; }
  }

  [HierarchyRoot]
  public class Order : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    [Association(PairTo = "Orders")]
    public Book Book { get; set; }

    [Field]
    [Association(PairTo = "Orders")]
    public Customer Customer { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage.IgnoreRulesValidateModel4
{
  [HierarchyRoot]
  public class MyEntity1 : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field(Length = 50)]
    public string FirstColumn { get; set; }

    [Field]
    public int IndexedValue { get; set; }
  }

  [HierarchyRoot]
  public class MyEntity2 : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field(Length = 50)]
    public string FirstColumn { get; set; }

    [Field]
    public int IndexedValue { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage.IgnoreRulesValidateModel5
{
  [HierarchyRoot]
  public class MyEntity1 : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field(Length = 50)]
    public string FirstColumn { get; set; }

    [Field]
    public int IndexedValue { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage.IgnoreRulesValidateModel6
{
  [HierarchyRoot]
  public class MyEntity1 : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field(Length = 50)]
    public string FirstColumn { get; set; }

    [Field]
    public int IndexedValue { get; set; }
  }

  [HierarchyRoot]
  public class MyEntity2 : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public int IndexedValue { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage.IgnoreRulesValidateModel7
{
  [HierarchyRoot]
  public class MyEntity1 : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field(Length = 50)]
    public string FirstColumn { get; set; }

    [Field]
    public int IndexedValue { get; set; }
  }

  [HierarchyRoot]
  public class MyEntity2 : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field(Length = 50)]
    public string FirstColumn { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class IgnoreRulesValidateTest
  {
    private const string DefaultSchema = WellKnownSchemas.Schema1;
    private const string Schema1 = WellKnownSchemas.Schema2;
    private const string Schema2 = WellKnownSchemas.Schema3;

    private readonly Type[] model1Types = new[] { typeof(Model1.Author), typeof(Model1.Book), typeof(Model1.Customer), typeof(Model1.Order), typeof(Model1.Person) };
    private readonly Type[] model2Types = new[] { typeof(Model2.Author), typeof(Model2.Book), typeof(Model2.Customer), typeof(Model2.Order), typeof(Model2.Person) };
    private readonly Type[] model3Types = new[] { typeof(Model3.MyEntity1), typeof(Model3.MyEntity2) };
    private readonly Type[] model4Types = new[] { typeof(Model4.MyEntity1) };
    private readonly Type[] model5Types = new[] { typeof(Model5.MyEntity1), typeof(Model5.MyEntity2) };
    private readonly Type[] model6Types = new[] { typeof(Model6.MyEntity1), typeof(Model6.MyEntity2) };

    private readonly Type[] model1PlusIgnorable = new[] {
      typeof(Model1.Author), typeof(Model1.Book), typeof(Model1.Customer), typeof(Model1.Order), typeof(Model1.Person),
      typeof(ignorablePart.IgnoredTable), typeof(ignorablePart.FieldInjector) };

    private readonly bool createConstraintsWithTable = StorageProviderInfo.Instance.Provider == StorageProvider.Sqlite;
    private readonly bool noExceptionOnIndexKeyColumnDrop = StorageProviderInfo.Instance.Provider.In(StorageProvider.PostgreSql, StorageProvider.MySql);
    private readonly SqlDriver sqlDriver = TestSqlDriver.Create(GetConnectionInfo());

    private Key changedOrderKey;
    private bool isMultidatabaseTest;
    private bool isMultischemaTest;

    [SetUp]
    public void SetUp() => ClearMultidatabaseAndMultischemaFlags();

    [Test]
    public void PerformUpdateTest()
    {
      BuildDomainAndFillData();
      UpgradeDomain(DomainUpgradeMode.Perform);
      BuildDomainInValidateMode();
    }

    [Test]
    public void PerformSafelyUpdateTest()
    {
      BuildDomainAndFillData();
      UpgradeDomain(DomainUpgradeMode.PerformSafely);
      BuildDomainInValidateMode();
    }

    [Test]
    public void IgnoreColumnTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, null, model3Types).Dispose();

      var catalog = GetCatalog();
      var schemaName = catalog.DefaultSchema.Name;
      CreateColumn(catalog, schemaName, "MyEntity2", "SimpleIgnoredColumn", GetTypeForInteger(SqlType.Int32));

      CreateColumn(catalog, schemaName, "MyEntity2", "IgnoreAFirstColumn", GetTypeForInteger(SqlType.Int64));
      CreateColumn(catalog, schemaName, "MyEntity2", "IgnoreASecondColumn", GetTypeForInteger(SqlType.Int64));
      CreateColumn(catalog, schemaName, "MyEntity2", "IgnoreAThirdColumn", GetTypeForInteger(SqlType.Int64));

      CreateColumn(catalog, schemaName, "MyEntity1", "IgnoreBFirstColumn", GetTypeForInteger(SqlType.Int64));
      CreateColumn(catalog, schemaName, "MyEntity1", "IgnoreBSecondColumn", GetTypeForInteger(SqlType.Int64));
      CreateColumn(catalog, schemaName, "MyEntity1", "IgnoreBThirdColumn", GetTypeForInteger(SqlType.Int64));

      CreateColumn(catalog, schemaName, "MyEntity2", "IgnoreBFirstColumn", GetTypeForInteger(SqlType.Int64));
      CreateColumn(catalog, schemaName, "MyEntity2", "IgnoreBSecondColumn", GetTypeForInteger(SqlType.Int64));
      CreateColumn(catalog, schemaName, "MyEntity2", "IgnoreBThirdColumn", GetTypeForInteger(SqlType.Int64));


      var ignoreRuleCollection = new IgnoreRuleCollection();
      _ = ignoreRuleCollection.IgnoreColumn("SimpleIgnoredColumn").WhenTable("MyEntity2");
      _ = ignoreRuleCollection.IgnoreColumn("IgnoreA*");
      _ = ignoreRuleCollection.IgnoreColumn("IgnoreB*").WhenTable("MyEntity*");

      if (StorageProviderInfo.Instance.CheckAllFeaturesSupported(ProviderFeatures.ForeignKeyConstraints)) {
        CreateColumn(catalog, schemaName, "MyEntity2", "ReferencedIgnoredColumn", GetTypeForInteger(SqlType.Int64));
        CreateForeignKeyInDb(catalog, schemaName, "MyEntity2", "ReferencedIgnoredColumn", "MyEntity1", "Id", "FK_MyEntity1_MyEntity1ID");
        _ = ignoreRuleCollection.IgnoreColumn("ReferencedIgnoredColumn").WhenTable("MyEntity2").WhenSchema(schemaName);
      }

      BuildDomain(DomainUpgradeMode.Perform, ignoreRuleCollection, model3Types).Dispose();

      catalog = GetCatalog();
      var schema = catalog.DefaultSchema;

      var table = schema.Tables["MyEntity2"];
      Assert.That(table.Columns.Any(c => c.Name == "SimpleIgnoredColumn"), Is.True);
      Assert.That(table.Columns.Count(c => c.Name.StartsWith("IgnoreA")), Is.EqualTo(3));
      Assert.That(
        schema.Tables.Where(t => t.Name.StartsWith("MyEntity", StringComparison.OrdinalIgnoreCase))
          .SelectMany(t => t.Columns).Count(c => c.Name.StartsWith("IgnoreB")),
        Is.EqualTo(6));

      if (StorageProviderInfo.Instance.CheckAllFeaturesSupported(ProviderFeatures.ForeignKeyConstraints)) {
        Assert.That(table.Columns.Any(c => c.Name == "ReferencedIgnoredColumn"), Is.True);
        Assert.That(table.TableConstraints.Any(tc => tc.Name == "FK_MyEntity1_MyEntity1ID"), Is.True);
      }
    }

    [Test]
    public void IgnoreAllColumnsInTableTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, null, model3Types).Dispose();

      var catalog = GetCatalog();
      var schema = catalog.DefaultSchema.Name;
      var addedColumnsNames = new[] { "Id", "FirstColumn", "SecondColumn" };
      var addedColumnsTypes = new[] { GetTypeForInteger(SqlType.Int64), GetTypeForInteger(SqlType.Int64), GetTypeForInteger(SqlType.Int64) };

      if (createConstraintsWithTable) {
        var delayedOp = CreateTableDelayed(catalog, schema, "IgnoredTable", addedColumnsNames, addedColumnsTypes);
        CreatePrimaryKeyLocally(catalog, schema, "IgnoredTable", "Id", "PK_IgnoredTable_Id");
        delayedOp();
      }
      else {
        CreateTable(catalog, schema, "IgnoredTable", addedColumnsNames, addedColumnsTypes);
        CreatePrimaryKeyInDb(catalog, schema, "IgnoredTable", "Id", "PK_IgnoredTable_Id");
      }
      var ignoreRules = new IgnoreRuleCollection();
      _ = ignoreRules.IgnoreColumn("*").WhenTable("IgnoredTable");

      BuildDomain(DomainUpgradeMode.Validate, ignoreRules, model3Types).Dispose();
    }

    [Test]
    public void IgnoreIndexTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, null, model1Types).Dispose();

      var schema = GetCatalog().DefaultSchema;

      var keyColumns = new string[] { "ISBN" };
      CreateIndex(schema, "Book", "IX_GIgnored_Index1", keyColumns, null);

      keyColumns = new string[] { "Title" };
      CreateIndex(schema, "Book", "IX_GIgnored_Index2", keyColumns, null);

      keyColumns = new string[] { "ISBN" };
      var includedColimns = new string[] { "Title" };

      CreateIndex(schema, "Book", "IX_Ignored_Index", keyColumns, includedColimns);


      var ingnoreRuleCollection = new IgnoreRuleCollection();
      _ = ingnoreRuleCollection.IgnoreIndex("IX_Ignored_Index").WhenTable("Book");
      _ = ingnoreRuleCollection.IgnoreIndex("IX_GIgnored*").WhenTable("B*");

      BuildDomain(DomainUpgradeMode.Perform, ingnoreRuleCollection, model1Types).Dispose();

      Assert.That(GetCatalog().DefaultSchema.Tables["Book"].Indexes.Any(i => i.Name == "IX_Ignored_Index"), Is.True);
      Assert.That(GetCatalog().DefaultSchema.Tables["Book"].Indexes
          .Count(i => i.Name.StartsWith("IX_GIgnored_Index", StringComparison.OrdinalIgnoreCase)),
        Is.EqualTo(2));
    }

    [Test]
    public void IgnoreTableTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, null, model3Types).Dispose();

      var catalog = GetCatalog();
      var schema = catalog.DefaultSchema.Name;

      if (createConstraintsWithTable) {
        var addedColumnsNames = new[] { "Id", "FirstColumn" };
        var addedColumnsTypes = new[] { GetTypeForInteger(SqlType.Int32), GetTypeForInteger(SqlType.Int64) };

        var delayedOp = CreateTableDelayed(catalog, schema, "MyIgnoredEntity1", addedColumnsNames, addedColumnsTypes);
        CreatePrimaryKeyLocally(catalog, schema, "MyIgnoredEntity1", "Id", "PK_MyIgnoredEntity1_Id");
        delayedOp();


        addedColumnsNames = new[] { "Id", "FirstColumn", "MyEntity2Id" };
        addedColumnsTypes = new[] { GetTypeForInteger(SqlType.Int32), GetTypeForInteger(SqlType.Int64), GetTypeForInteger(SqlType.Int64) };

        delayedOp = CreateTableDelayed(catalog, schema, "MyIgnoredEntity2", addedColumnsNames, addedColumnsTypes);
        CreatePrimaryKeyLocally(catalog, schema, "MyIgnoredEntity2", "Id", "PK_MyIgnoredEntity2_Id");
        CreateForeignKeyLocally(catalog, schema, "MyIgnoredEntity2", "MyEntity2Id", "MyEntity2", "Id", "FK_MyIgnoredEntity2MyEntity2Id");
        delayedOp();

        addedColumnsNames = new[] { "Id", "FirstColumn", "SecondColumn" };
        addedColumnsTypes = new[] { GetTypeForInteger(SqlType.Int64), GetTypeForInteger(SqlType.Int64), GetTypeForInteger(SqlType.Int64) };

        delayedOp = CreateTableDelayed(catalog, schema, "IgnoredFirstTable", addedColumnsNames, addedColumnsTypes);
        CreatePrimaryKeyLocally(catalog, schema, "IgnoredFirstTable", "Id", "PK_IgnoredFirstTable_Id");
        delayedOp();

        delayedOp = CreateTableDelayed(catalog, schema, "IgnoredSecondTable", addedColumnsNames, addedColumnsTypes);
        CreatePrimaryKeyLocally(catalog, schema, "IgnoredSecondTable", "Id", "PK_IgnoredSecondTable_Id");
        delayedOp();
      }
      else {
        var addedColumnsNames = new[] { "Id", "FirstColumn" };
        var addedColumnsTypes = new[] { GetTypeForInteger(SqlType.Int32), GetTypeForInteger(SqlType.Int64) };

        CreateTable(catalog, schema, "MyIgnoredEntity1", addedColumnsNames, addedColumnsTypes);
        CreatePrimaryKeyInDb(catalog, schema, "MyIgnoredEntity1", "Id", "PK_MyIgnoredEntity1_Id");

        addedColumnsNames = new[] { "Id", "FirstColumn", "MyEntity2Id" };
        addedColumnsTypes = new[] { GetTypeForInteger(SqlType.Int32), GetTypeForInteger(SqlType.Int64), GetTypeForInteger(SqlType.Int64) };

        CreateTable(catalog, schema, "MyIgnoredEntity2", addedColumnsNames, addedColumnsTypes);
        CreatePrimaryKeyInDb(catalog, schema, "MyIgnoredEntity2", "Id", "PK_MyIgnoredEntity2_Id");
        CreateForeignKeyInDb(catalog, schema, "MyIgnoredEntity2", "MyEntity2Id", "MyEntity2", "Id", "FK_MyIgnoredEntity2MyEntity2Id");


        addedColumnsNames = new[] { "Id", "FirstColumn", "SecondColumn" };
        addedColumnsTypes = new[] { GetTypeForInteger(SqlType.Int64), GetTypeForInteger(SqlType.Int64), GetTypeForInteger(SqlType.Int64) };

        CreateTable(catalog, schema, "IgnoredFirstTable", addedColumnsNames, addedColumnsTypes);
        CreatePrimaryKeyInDb(catalog, schema, "IgnoredFirstTable", "Id", "PK_IgnoredFirstTable_Id");

        CreateTable(catalog, schema, "IgnoredSecondTable", addedColumnsNames, addedColumnsTypes);
        CreatePrimaryKeyInDb(catalog, schema, "IgnoredSecondTable", "Id", "PK_IgnoredSecondTable_Id");
      }

      var ignoreRules = new IgnoreRuleCollection();
      _ = ignoreRules.IgnoreTable("MyIgnoredEntity1");
      _ = ignoreRules.IgnoreTable("MyIgnoredEntity2");
      _ = ignoreRules.IgnoreTable("Ignored*");

      BuildDomain(DomainUpgradeMode.Validate, ignoreRules, model3Types).Dispose();
    }

    [Test]
    public void InsertIntoTableWithIgnoredColumnTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, null, model3Types).Dispose();

      var catalog = GetCatalog();
      var schema = catalog.DefaultSchema.Name;
      CreateColumn(catalog, schema, "Myentity2", "SimpleIgnoredColumn", GetTypeForInteger(SqlType.Int64));
      var ignoreRuleCollection = new IgnoreRuleCollection();
      _ = ignoreRuleCollection.IgnoreColumn("SimpleIgnoredColumn").WhenTable("MyEntity2");

      using (var validatedDomain = BuildDomain(DomainUpgradeMode.Validate, ignoreRuleCollection, model3Types))
      using (var session = validatedDomain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new Model3.MyEntity2 { FirstColumn = "some test" };
        transaction.Complete();
      }
      ValidateMyEntity(catalog, schema);
    }

    [Test]
    public void InsertIntoTableWithNotNullableIgnoredColumnTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite);

      BuildDomain(DomainUpgradeMode.Recreate, null, model3Types).Dispose();

      var catalog = GetCatalog();
      var schema = catalog.Schemas[catalog.DefaultSchema.Name] ?? catalog.Schemas[0];

      schema.Tables["MyEntity2"].CreateColumn("SimpleIgnoredColumn", GetTypeForInteger(SqlType.Int64)).IsNullable = false;

      var alter = SqlDdl.Alter(schema.Tables["MyEntity2"],
        SqlDdl.AddColumn(schema.Tables["MyEntity2"].TableColumns["SimpleIgnoredColumn"]));
      var commandText = sqlDriver.Compile(alter).GetCommandText();
      ExecuteNonQuery(commandText);

      var ignoreRuleCollection = new IgnoreRuleCollection();
      _ = ignoreRuleCollection.IgnoreColumn("SimpleIgnoredColumn").WhenTable("MyEntity2").WhenSchema(schema.Name);

      _ = Assert.Throws(Is.InstanceOf(typeof(StorageException)), () => {
        using (var validatedDomain = BuildDomain(DomainUpgradeMode.Validate, ignoreRuleCollection, model3Types))
        using (var session = validatedDomain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          _ = new Model3.MyEntity2 { FirstColumn = "some test" };
          transaction.Complete();
        }
      });
    }

    [Test]
    public void DropTableWithIgnoredColumnTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, null, model3Types).Dispose();

      var catalog = GetCatalog();
      var schema = catalog.DefaultSchema.Name;
      CreateColumn(catalog, schema, "MyEntity2", "SimpleIgnoredColumn", GetTypeForInteger(SqlType.Int64));

      var ignoreRuleCollection = new IgnoreRuleCollection();
      _ = ignoreRuleCollection.IgnoreColumn("SimpleIgnoredColumn").WhenTable("MyEntity2");

      _ = Assert.Throws<SchemaSynchronizationException>(
        () => BuildDomain(DomainUpgradeMode.Perform, ignoreRuleCollection, model4Types).Dispose());
    }

    [Test]
    public void DropTableWithIgnoredIndexTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, null, model3Types).Dispose();

      var schema = GetCatalog().DefaultSchema;
      var keyColumns = new string[] { "IndexedValue" };
      var includedColimns = new string[] { "FirstColumn" };

      CreateIndex(schema, "MyEntity2", "IX_Ignored_Index", keyColumns, includedColimns);

      var ignoreRuleCollection = new IgnoreRuleCollection();
      _ = ignoreRuleCollection.IgnoreIndex("IX_Ignored_Index").WhenTable("MyEntity2");

      _ = Assert.Throws<SchemaSynchronizationException>(
        () => BuildDomain(DomainUpgradeMode.Perform, ignoreRuleCollection, model4Types).Dispose());
    }

    [Test]
    public void DropIncludedColumnOfIgnoredIndexTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.IncludedColumns);

      BuildDomain(DomainUpgradeMode.Recreate, null, model3Types).Dispose();

      var schema = GetCatalog().DefaultSchema;
      var keyColumns = new string[] { "IndexedValue" };
      var includedColimns = new string[] { "FirstColumn" };

      CreateIndex(schema, "MyEntity2", "IX_Ignored_Index", keyColumns, includedColimns);

      var ignoreRuleCollection = new IgnoreRuleCollection();
      _ = ignoreRuleCollection.IgnoreIndex("IX_Ignored_Index").WhenTable("MyEntity2");

      _ = Assert.Throws<StorageException>(
        () => BuildDomain(DomainUpgradeMode.Perform, ignoreRuleCollection, model5Types).Dispose());
    }

    [Test]
    public void DropKeyColumnOfIgnoredIndexTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, null, model3Types).Dispose();

      var schema = GetCatalog().DefaultSchema;
      var keyColumns = new string[] { "IndexedValue", "FirstColumn" };

      CreateIndex(schema, "MyEntity2", "IX_Ignored_Index", keyColumns, null);

      var ignoreRuleCollection = new IgnoreRuleCollection();
      _ = ignoreRuleCollection.IgnoreIndex("IX_Ignored_Index").WhenTable("MyEntity2");
      if (noExceptionOnIndexKeyColumnDrop) {
        BuildDomain(DomainUpgradeMode.Perform, ignoreRuleCollection, model6Types).Dispose();
      }
      else {
        if (createConstraintsWithTable) {
          _ = Assert.Throws<SchemaSynchronizationException>(
            () => BuildDomain(DomainUpgradeMode.Perform, ignoreRuleCollection, model6Types).Dispose());
        }
        else {
          _ = Assert.Throws<StorageException>(
            () => BuildDomain(DomainUpgradeMode.Perform, ignoreRuleCollection, model6Types).Dispose());
        }
      }
    }

    [Test]
    public void DropReferencedTableTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ForeignKeyConstraints);

      BuildDomain(DomainUpgradeMode.Recreate, null, model3Types).Dispose();

      var catalog = GetCatalog();
      var schema = catalog.DefaultSchema.Name;
      CreateColumn(catalog, schema, "MyEntity2", "ReferencedIgnoredColumn", GetTypeForInteger(SqlType.Int64));
      CreateForeignKeyInDb(catalog, schema, "MyEntity2", "ReferencedIgnoredColumn", "MyEntity1", "Id", "FK_MyEntity1_MyEntity1ID");

      var ignoreRuleCollection = new IgnoreRuleCollection();
      _ = ignoreRuleCollection.IgnoreColumn("ReferencedIgnoredColumn").WhenTable("MyEntity2").WhenSchema(schema);

      _ = Assert.Throws<SchemaSynchronizationException>(() => {
        BuildDomain(DomainUpgradeMode.Perform, ignoreRuleCollection, model4Types ).Dispose();
      });
    }

    [Test]
    public void UpgradeDomainWithIgnoreRuleByMaskInPerformModeTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, null, model3Types).Dispose();

      var catalog = GetCatalog();
      var schema = catalog.Schemas[catalog.DefaultSchema.Name] ?? catalog.Schemas[0];

      CreateColumn(catalog, schema.Name, "MyEntity2", "IgnoredFirstColumn", GetTypeForInteger(SqlType.Int64));
      CreateColumn(catalog, schema.Name, "MyEntity2", "IgnoredSecondColumn", GetTypeForInteger(SqlType.Int64));

      var ignoreRules = new IgnoreRuleCollection();
      _ = ignoreRules.IgnoreColumn("Ignored*").WhenTable("MyEntity2");

      using (var domain = BuildDomain(DomainUpgradeMode.Perform, ignoreRules, model3Types))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new Model3.MyEntity1 { FirstColumn = "Some text" };
        _ = new Model3.MyEntity2 { FirstColumn = "Second some test" };
        transaction.Complete();
      }
      var myEntity2 = SqlDml.TableRef(schema.Tables["MyEntity2"]);
      var select = SqlDml.Select(myEntity2);
      
      var commandText = sqlDriver.Compile(select, BuildCompilerConfig(false)).GetCommandText();
      Assert.That(ExecuteQuery(commandText, 3), Is.EqualTo(DBNull.Value));
      Assert.That(ExecuteQuery(commandText, 4), Is.EqualTo(DBNull.Value));
    }

    [Test]
    public void UpgradeDomainWithIgnoreRuleByMaskInPerformSafelyModeTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, null, model3Types).Dispose();

      var catalog = GetCatalog();
      var schema = catalog.Schemas[catalog.DefaultSchema.Name] ?? catalog.Schemas[0];
      CreateColumn(catalog, schema.Name, "MyEntity2", "IgnoredFirstColumn", GetTypeForInteger(SqlType.Int64));
      CreateColumn(catalog, schema.Name, "MyEntity2", "IgnoredSecondColumn", GetTypeForInteger(SqlType.Int64));

      var ignoreRules = new IgnoreRuleCollection();
      _ = ignoreRules.IgnoreColumn("Ignored*");

      using (var domain = BuildDomain(DomainUpgradeMode.PerformSafely, ignoreRules, model3Types))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new Model3.MyEntity1 { FirstColumn = "Some text" };
        _ = new Model3.MyEntity2 { FirstColumn = "Second some test" };
        transaction.Complete();
      }
      var myEntity2 = SqlDml.TableRef(schema.Tables["MyEntity2"]);
      var select = SqlDml.Select(myEntity2);
      var commandText = sqlDriver.Compile(select, BuildCompilerConfig(false)).GetCommandText();
      Assert.That(ExecuteQuery(commandText, 3), Is.EqualTo(DBNull.Value));
      Assert.That(ExecuteQuery(commandText, 4), Is.EqualTo(DBNull.Value));
    }

    [Test]
    public void MultischemaValidateTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multischema);

      SetMultischema();

      var catalog = GetCatalog();
      CreateSchema(catalog, Schema1);
      CreateSchema(catalog, Schema2);

      BuildDomain(DomainUpgradeMode.Recreate, null, model1Types, model3Types).Dispose();

      catalog = GetCatalog();
      CreateColumn(catalog, Schema2, "MyEntity2", "ReferencedIgnoredColumn", GetTypeForInteger(SqlType.Int64));
      CreateForeignKeyInDb(catalog, Schema2, "MyEntity2", "ReferencedIgnoredColumn", "MyEntity1", "Id", "FK_MyEnt2MyEnt1MyEnt1Id");

      var addedColumnsNames = new[] { "Id", "FirstColumn", "MyEntity2Id" };
      var addedColumnsTypes = new[] { GetTypeForInteger(SqlType.Int32), GetTypeForInteger(SqlType.Int64), GetTypeForInteger(SqlType.Int64) };
      CreateTable(catalog, Schema2, "MyIgnoredEntity", addedColumnsNames, addedColumnsTypes);
      CreatePrimaryKeyInDb(catalog, Schema2, "MyIgnoredEntity", "Id", "PK_MyIgnoreTable_Id");
      CreateForeignKeyInDb(catalog, Schema2, "MyIgnoredEntity", "MyEntity2Id", "MyEntity2", "Id", "FK_MyIgnoredEntityMyEntity2Id");

      var ignoreRules = new IgnoreRuleCollection();
      _ = ignoreRules.IgnoreColumn("ReferencedIgnoredColumn").WhenSchema(Schema2);
      _ = ignoreRules.IgnoreTable("MyIgnoredEntity").WhenSchema(Schema2);
      _ = ignoreRules.IgnoreTable("IgnoredTable").WhenSchema(Schema1);
      _ = ignoreRules.IgnoreColumn("SomeIgnoredField").WhenTable("Order").WhenSchema(Schema1);
      _ = ignoreRules.IgnoreColumn("IgnoredColumn.Id").WhenTable("Author").WhenSchema(Schema1);

      BuildDomain(DomainUpgradeMode.Validate, ignoreRules, model1Types, model3Types).Dispose();
    }

    [Test]
    public void MultidatabaseValidateTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multidatabase);

      SetMultidatabase();

      BuildDomain(DomainUpgradeMode.Recreate, null, model1Types, model3Types).Dispose();

      var secondCatalog = GetCatalog(Multimapping.MultidatabaseTest.Database2Name);
      CreateColumn(secondCatalog, DefaultSchema, "MyEntity2", "ReferencedIgnoredColumn", GetTypeForInteger(SqlType.Int64), true);
      CreateForeignKeyInDb(secondCatalog, DefaultSchema, "MyEntity2", "ReferencedIgnoredColumn", "MyEntity1", "Id", "FK_MyEntity2_MyEntity1_MyEntity1ID", true);

      var addedColumnsNames = new[] { "Id", "FirstColumn", "MyEntity2Id" };
      var addedColumnsTypes = new[] { GetTypeForInteger(SqlType.Int32), GetTypeForInteger(SqlType.Int64), GetTypeForInteger(SqlType.Int64) };
      CreateTable(secondCatalog, DefaultSchema, "MyIgnoredEntity", addedColumnsNames, addedColumnsTypes, true);
      CreatePrimaryKeyInDb(secondCatalog, DefaultSchema, "MyIgnoredEntity", "Id", "PK_MyIgnoreTable_Id", true);
      CreateForeignKeyInDb(secondCatalog, DefaultSchema, "MyIgnoredEntity", "MyEntity2Id", "MyEntity2", "Id", "FK_MyIgnoredEntity_MyEntity2_Id", true);

      var ignoreRules = new IgnoreRuleCollection();
      _ = ignoreRules.IgnoreColumn("ReferencedIgnoredColumn").WhenSchema(DefaultSchema).WhenDatabase(Multimapping.MultidatabaseTest.Database2Name);
      _ = ignoreRules.IgnoreTable("MyIgnoredEntity").WhenDatabase(Multimapping.MultidatabaseTest.Database2Name);
      _ = ignoreRules.IgnoreTable("IgnoredTable").WhenDatabase(Multimapping.MultidatabaseTest.Database1Name);
      _ = ignoreRules.IgnoreColumn("SomeIgnoredField").WhenTable("Order").WhenDatabase(Multimapping.MultidatabaseTest.Database1Name);
      _ = ignoreRules.IgnoreColumn("IgnoredColumn.Id").WhenTable("Author").WhenDatabase(Multimapping.MultidatabaseTest.Database1Name);

      BuildDomain(DomainUpgradeMode.Validate, ignoreRules, model1Types, model3Types).Dispose();
    }

    [Test]
    public void MultischemaUpgrageInPerformModeTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multischema);

      SetMultischema();

      var catalog = GetCatalog();
      CreateSchema(catalog, Schema1);
      CreateSchema(catalog, Schema2);
      BuildDomainAndFillData();

      catalog = GetCatalog();
      CreateColumn(catalog, Schema2, "MyEntity2", "ReferencedIgnoredColumn", GetTypeForInteger(SqlType.Int64));
      CreateForeignKeyInDb(catalog, Schema2, "MyEntity2", "ReferencedIgnoredColumn", "MyEntity1", "Id", "FK_MyEnt2MyEnt1MyEnt1Id");

      var addedColumnsNames = new[] { "Id", "FirstColumn", "MyEntity2Id" };
      var addedColumnsTypes = new[] { GetTypeForInteger(SqlType.Int32), GetTypeForInteger(SqlType.Int64), GetTypeForInteger(SqlType.Int64) };
      CreateTable(catalog, Schema2, "MyIgnoredEntity", addedColumnsNames, addedColumnsTypes);
      CreatePrimaryKeyInDb(catalog, Schema2, "MyIgnoredEntity", "Id", "PK_MyIgnoreTable_Id");
      CreateForeignKeyInDb(catalog, Schema2, "MyIgnoredEntity", "MyEntity2Id", "MyEntity2", "Id", "FK_MyIgnoredEntityMyEntity2Id");

      var ignoreRules = new IgnoreRuleCollection();
      _ = ignoreRules.IgnoreColumn("ReferencedIgnoredColumn").WhenSchema(Schema2);
      _ = ignoreRules.IgnoreTable("MyIgnoredEntity").WhenSchema(Schema2);
      _ = ignoreRules.IgnoreTable("IgnoredTable").WhenSchema(Schema1);
      _ = ignoreRules.IgnoreColumn("SomeIgnoredField").WhenTable("Order").WhenSchema(Schema1);
      _ = ignoreRules.IgnoreColumn("IgnoredColumn.Id").WhenTable("Author").WhenSchema(Schema1);
      UpgradeDomain(DomainUpgradeMode.Perform, ignoreRules);

      ignoreRules = new IgnoreRuleCollection();
      _ = ignoreRules.IgnoreColumn("ReferencedIgnoredColumn").WhenSchema(Schema2);
      _ = ignoreRules.IgnoreTable("MyIgnoredEntity").WhenSchema(Schema2);
      BuildDomainInValidateMode(ignoreRules);
      ValidateMyEntity(catalog, Schema2);
    }

    [Test]
    public void MultischemaUpgrageInPerformSafelyModeTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multischema);

      SetMultischema();

      var catalog = GetCatalog();
      CreateSchema(catalog, Schema1);
      CreateSchema(catalog, Schema2);
      BuildDomainAndFillData();

      catalog = GetCatalog();
      CreateColumn(catalog, Schema2, "MyEntity2", "ReferencedIgnoredColumn", GetTypeForInteger(SqlType.Int64));
      CreateForeignKeyInDb(catalog, Schema2, "MyEntity2", "ReferencedIgnoredColumn", "MyEntity1", "Id", "FK_MyEnt2MyEnt1MyEnt1Id");

      var addedColumnsNames = new[] { "Id", "FirstColumn", "MyEntity2Id" };
      var addedColumnsTypes = new[] { GetTypeForInteger(SqlType.Int32), GetTypeForInteger(SqlType.Int64), GetTypeForInteger(SqlType.Int64) };
      CreateTable(catalog, Schema2, "MyIgnoredEntity", addedColumnsNames, addedColumnsTypes);
      CreatePrimaryKeyInDb(catalog, Schema2, "MyIgnoredEntity", "Id", "PK_MyIgnoreTable_Id");
      CreateForeignKeyInDb(catalog, Schema2, "MyIgnoredEntity", "MyEntity2Id", "MyEntity2", "Id", "FK_MyIgnoredEntityMyEntity2Id");

      var ignoreRules = new IgnoreRuleCollection();
      _ = ignoreRules.IgnoreColumn("ReferencedIgnoredColumn").WhenSchema(Schema2);
      _ = ignoreRules.IgnoreTable("MyIgnoredEntity").WhenSchema(Schema2);
      _ = ignoreRules.IgnoreTable("IgnoredTable").WhenSchema(Schema1);
      _ = ignoreRules.IgnoreColumn("SomeIgnoredField").WhenTable("Order").WhenSchema(Schema1);
      _ = ignoreRules.IgnoreColumn("IgnoredColumn.Id").WhenTable("Author").WhenSchema(Schema1);
      UpgradeDomain(DomainUpgradeMode.PerformSafely, ignoreRules);

      ignoreRules = new IgnoreRuleCollection();
      _ = ignoreRules.IgnoreColumn("ReferencedIgnoredColumn").WhenSchema(Schema2);
      _ = ignoreRules.IgnoreTable("MyIgnoredEntity").WhenSchema(Schema2);
      BuildDomainInValidateMode(ignoreRules);
      ValidateMyEntity(catalog, Schema2);
    }

    [Test]
    public void MultidatabaseUpgradeInPerformModeTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multidatabase);

      SetMultidatabase();

      BuildDomainAndFillData();

      var secondCatalog = GetCatalog(Multimapping.MultidatabaseTest.Database2Name);
      CreateColumn(secondCatalog, DefaultSchema, "MyEntity2", "ReferencedIgnoredColumn", GetTypeForInteger(SqlType.Int64), true);
      CreateForeignKeyInDb(secondCatalog, DefaultSchema, "MyEntity2", "ReferencedIgnoredColumn", "MyEntity1", "Id", "FK_MyEntity2_MyEntity1_MyEntity1ID", true);

      var addedColumnsNames = new[] { "Id", "FirstColumn", "MyEntity2Id" };
      var addedColumnsTypes = new[] { GetTypeForInteger(SqlType.Int32), GetTypeForInteger(SqlType.Int64), GetTypeForInteger(SqlType.Int64) };
      CreateTable(secondCatalog, DefaultSchema, "MyIgnoredEntity", addedColumnsNames, addedColumnsTypes, true);
      CreatePrimaryKeyInDb(secondCatalog, DefaultSchema, "MyIgnoredEntity", "Id", "PK_MyIgnoreTable_Id", true);
      CreateForeignKeyInDb(secondCatalog, DefaultSchema, "MyIgnoredEntity", "MyEntity2Id", "MyEntity2", "Id", "FK_MyIgnoredEntity_MyEntity2_Id", true);

      var ignoreRules = new IgnoreRuleCollection();
      _ = ignoreRules.IgnoreColumn("ReferencedIgnoredColumn").WhenDatabase(Multimapping.MultidatabaseTest.Database2Name);
      _ = ignoreRules.IgnoreTable("MyIgnoredEntity").WhenDatabase(Multimapping.MultidatabaseTest.Database2Name);
      _ = ignoreRules.IgnoreTable("IgnoredTable").WhenDatabase(Multimapping.MultidatabaseTest.Database1Name);
      _ = ignoreRules.IgnoreColumn("SomeIgnoredField").WhenTable("Order").WhenDatabase(Multimapping.MultidatabaseTest.Database1Name);
      _ = ignoreRules.IgnoreColumn("IgnoredColumn.Id").WhenTable("Author").WhenDatabase(Multimapping.MultidatabaseTest.Database1Name);
      UpgradeDomain(DomainUpgradeMode.Perform, ignoreRules);

      ignoreRules = new IgnoreRuleCollection();
      _ = ignoreRules.IgnoreColumn("ReferencedIgnoredColumn").WhenDatabase(Multimapping.MultidatabaseTest.Database2Name);
      _ = ignoreRules.IgnoreTable("MyIgnoredEntity").WhenDatabase(Multimapping.MultidatabaseTest.Database2Name);
      BuildDomainInValidateMode(ignoreRules);
      ValidateMyEntity(secondCatalog, DefaultSchema, true);
    }

    [Test]
    public void MultidatabaseUpgradeInPerformSafelyModeTest()
    {
      SetMultidatabase();
      Require.AllFeaturesSupported(ProviderFeatures.Multidatabase);
      isMultidatabaseTest = true;
      BuildDomainAndFillData();

      var secondCatalog = GetCatalog(Multimapping.MultidatabaseTest.Database2Name);
      CreateColumn(secondCatalog, DefaultSchema, "MyEntity2", "ReferencedIgnoredColumn", GetTypeForInteger(SqlType.Int64), true);
      CreateForeignKeyInDb(secondCatalog, DefaultSchema, "MyEntity2", "ReferencedIgnoredColumn", "MyEntity1", "Id", "FK_MyEntity2_MyEntity1_MyEntity1ID", true);
      var addedColumnsNames = new[] { "Id", "FirstColumn", "MyEntity2Id" };
      var addedColumnsTypes = new[] { GetTypeForInteger(SqlType.Int32), GetTypeForInteger(SqlType.Int64), GetTypeForInteger(SqlType.Int64) };
      CreateTable(secondCatalog, DefaultSchema, "MyIgnoredEntity", addedColumnsNames, addedColumnsTypes, true);
      CreatePrimaryKeyInDb(secondCatalog, DefaultSchema, "MyIgnoredEntity", "Id", "PK_MyIgnoreTable_Id", true);
      CreateForeignKeyInDb(secondCatalog, DefaultSchema, "MyIgnoredEntity", "MyEntity2Id", "MyEntity2", "Id", "FK_MyIgnoredEntity_MyEntity2_Id", true);

      var ignoreRules = new IgnoreRuleCollection();
      _ = ignoreRules.IgnoreColumn("ReferencedIgnoredColumn").WhenDatabase(Multimapping.MultidatabaseTest.Database2Name);
      _ = ignoreRules.IgnoreTable("MyIgnoredEntity").WhenDatabase(Multimapping.MultidatabaseTest.Database2Name);
      _ = ignoreRules.IgnoreTable("IgnoredTable").WhenDatabase(Multimapping.MultidatabaseTest.Database1Name);
      _ = ignoreRules.IgnoreColumn("SomeIgnoredField").WhenTable("Order").WhenDatabase(Multimapping.MultidatabaseTest.Database1Name);
      _ = ignoreRules.IgnoreColumn("IgnoredColumn.Id").WhenTable("Author").WhenDatabase(Multimapping.MultidatabaseTest.Database1Name);
      UpgradeDomain(DomainUpgradeMode.PerformSafely, ignoreRules);

      ignoreRules = new IgnoreRuleCollection();
      _ = ignoreRules.IgnoreColumn("ReferencedIgnoredColumn").WhenDatabase(Multimapping.MultidatabaseTest.Database2Name);
      _ = ignoreRules.IgnoreTable("MyIgnoredEntity").WhenDatabase(Multimapping.MultidatabaseTest.Database2Name);
      BuildDomainInValidateMode(ignoreRules);
      ValidateMyEntity(secondCatalog, DefaultSchema, true);
    }

    private void BuildDomainAndFillData()
    {
      var t2Set = isMultidatabaseTest || isMultischemaTest ? model3Types : null;

      using var domain = BuildDomain(DomainUpgradeMode.Recreate, null, model1PlusIgnorable, t2Set);
      using var session = domain.OpenSession();
      using var transaction = session.OpenTransaction();

      var author = new Model1.Author {
        FirstName = "Ivan", LastName = "Goncharov", Birthday = new DateTime(1812, 6, 18)
      };
      var book = new Model1.Book { ISBN = "9780140440409", Title = "Oblomov" };
      _ = book.Authors.Add(author);

      var customer = new Model1.Customer {
        FirstName = "Alexey", LastName = "Kulakov", Birthday = new DateTime(1988, 8, 31)
      };
      var order = new Model1.Order { Book = book, Customer = customer };
      order["SomeIgnoredField"] = "Secret information for FBI :)";

      if (isMultidatabaseTest || isMultischemaTest) {
        _ = new Model3.MyEntity1 { FirstColumn = "first" };
        _ = new Model3.MyEntity2 { FirstColumn = "first" };
      }
      transaction.Complete();
    }

    private void UpgradeDomain(DomainUpgradeMode mode)
    {
      var ignoreRules = new IgnoreRuleCollection();
      _ = ignoreRules.IgnoreTable("IgnoredTable");
      _ = ignoreRules.IgnoreColumn("SomeIgnoredField").WhenTable("Order");
      _ = ignoreRules.IgnoreColumn("IgnoredColumn.Id").WhenTable("Author");

      using var domain = BuildDomain(mode, ignoreRules, model1Types);
      using var session = domain.OpenSession();
      using var transaction = session.OpenTransaction();

      var currentCustomer = session.Query.All<Model1.Customer>().First(c => c.LastName == "Kulakov");
      var order = session.Query.All<Model1.Order>().First(o => o.Customer.LastName == currentCustomer.LastName);
      var newCustomer = new Model1.Customer { FirstName = "Fred", LastName = "Smith", Birthday = new DateTime(1998, 7, 9) };
      order.Customer = newCustomer;
      changedOrderKey = order.Key;
      transaction.Complete();
    }

    private void UpgradeDomain(DomainUpgradeMode mode, IgnoreRuleCollection ignoreRules)
    {
      using var performDomain = BuildDomain(mode, ignoreRules, model1Types, model3Types);
      using var session = performDomain.OpenSession();
      using var transaction = session.OpenTransaction();

      var currentCustomer = session.Query.All<Model1.Customer>().First(c => c.LastName == "Kulakov");
      var order = session.Query.All<Model1.Order>().First(o => o.Customer.LastName == currentCustomer.LastName);
      var newCustomer = new Model1.Customer { FirstName = "Fred", LastName = "Smith", Birthday = new DateTime(1998, 7, 9) };
      order.Customer = newCustomer;
      changedOrderKey = order.Key;
      var currentEntity = session.Query.All<Model3.MyEntity2>().First(ent => ent.FirstColumn == "first");
      currentEntity.FirstColumn = "second";
      transaction.Complete();
    }

    private void BuildDomainInValidateMode()
    {
      using var domain = BuildDomain(DomainUpgradeMode.Validate, null, model1PlusIgnorable);
      using var session = domain.OpenSession();
      using var transaction = session.OpenTransaction();

      var result = session.Query.All<Model1.Order>().First(o => o.Key == changedOrderKey);
      Assert.That(result["SomeIgnoredField"], Is.EqualTo("Secret information for FBI :)"));
    }

    private void BuildDomainInValidateMode(IgnoreRuleCollection ignoreRules)
    {
      using var domain = BuildDomain(DomainUpgradeMode.Validate, ignoreRules, model1PlusIgnorable, model3Types);
      using var session = domain.OpenSession();
      using var transaction = session.OpenTransaction();

      var result = session.Query.All<Model1.Order>().First(o => o.Key == changedOrderKey);
      Assert.That(result["SomeIgnoredField"], Is.EqualTo("Secret information for FBI :)"));
    }

    private Domain BuildDomain(DomainUpgradeMode mode, IgnoreRuleCollection ignoreRules,
      Type[] firstPartTypes, Type[] secondPartTypes = null)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = mode;
      foreach (var t1 in firstPartTypes) {
        configuration.Types.Register(t1);
      }

      if (isMultischemaTest || isMultidatabaseTest) {
        foreach (var t2 in secondPartTypes) {
          configuration.Types.Register(t2);
        }

        var part1Namespaces = firstPartTypes.Select(t => t.Namespace).Distinct();
        var part2Namespaces = secondPartTypes.Select(t=>t.Namespace).Distinct();

        if (isMultischemaTest) {
          foreach (var ns in part1Namespaces) {
            configuration.MappingRules.Map(firstPartTypes[0].Assembly, ns).ToSchema(Schema1);
          }
          foreach(var ns in part2Namespaces) {
            configuration.MappingRules.Map(secondPartTypes[0].Assembly, ns).ToSchema(Schema2);
          }
          
          configuration.DefaultSchema = DefaultSchema;
        }
        else {
          foreach (var ns in part1Namespaces) {
            configuration.MappingRules.Map(firstPartTypes[0].Assembly, ns).ToDatabase(Multimapping.MultidatabaseTest.Database1Name);
          }
          foreach (var ns in part2Namespaces) {
            configuration.MappingRules.Map(secondPartTypes[0].Assembly, ns).ToDatabase(Multimapping.MultidatabaseTest.Database2Name);
          }
          configuration.DefaultSchema = DefaultSchema;
          configuration.DefaultDatabase = GetConnectionInfo().ConnectionUrl.GetDatabase();
        }
      }

      if (ignoreRules != null) {
        configuration.IgnoreRules = ignoreRules;
      }

      return Domain.Build(configuration);
    }

    private Catalog GetCatalog(string catalogName = null)
    {
      Catalog catalog;
      using (var sqlConnection = sqlDriver.CreateConnection()) {
        sqlConnection.Open();
        sqlConnection.BeginTransaction();
        if (catalogName == null) {
          catalog = sqlDriver.ExtractCatalog(sqlConnection);
        }
        else {
          var sqlExtractionTaskList = new List<SqlExtractionTask> {
            new SqlExtractionTask(catalogName)
          };
          catalog = sqlDriver.Extract(sqlConnection, sqlExtractionTaskList).Catalogs.First();
        }
        sqlConnection.Commit();
        sqlConnection.Close();
      }
      return catalog;
    }

    private static ConnectionInfo GetConnectionInfo() => DomainConfigurationFactory.Create().ConnectionInfo;

    private void ExecuteNonQuery(string commandText)
    {
      using (var connection = sqlDriver.CreateConnection()) {
        connection.Open();
        using (var command = connection.CreateCommand(commandText)) {
          _ = command.ExecuteNonQuery();
        }
        connection.Close();
      }
    }

    private object ExecuteQuery(string commandText, int returnedColumnIndex)
    {
      var result = new object();
      using (var connection = sqlDriver.CreateConnection()) {
        connection.Open();
        using (var command = connection.CreateCommand(commandText))
        using (var reader = command.ExecuteReader()) {
          if (reader.Read()) {
            result = reader.GetValue(returnedColumnIndex);
          }
        }
        connection.Close();
      }
      return result;
    }

    private void ValidateMyEntity(Catalog catalog, string schemaName, bool useDatabasePrefix = false)
    {
      var schemaToValidete = catalog.Schemas[schemaName] ?? catalog.Schemas[0];

      var myEntity2 = SqlDml.TableRef(schemaToValidete.Tables["MyEntity2"]);
      var select = SqlDml.Select(myEntity2);
      var commandText = sqlDriver.Compile(select, BuildCompilerConfig(useDatabasePrefix)).GetCommandText();
      Assert.That(ExecuteQuery(commandText, 3), Is.EqualTo(DBNull.Value));
    }

    private void CreateForeignKeyInDb(Catalog catalog, string schemaName, string tableName, string columnName,
      string referencedTableName, string referencedColumn, string foreignKeyName, bool useDatabasePrefix = false)
    {
      var schemaToAlter = catalog.Schemas[schemaName] ?? catalog.Schemas[0];

      var referencingTable = schemaToAlter.Tables[tableName];
      var referencedTable = schemaToAlter.Tables[referencedTableName];

      var foreignKey = referencingTable.CreateForeignKey(foreignKeyName);
      foreignKey.Columns.Add(referencingTable.TableColumns[columnName]);
      foreignKey.ReferencedTable = referencedTable;
      foreignKey.ReferencedColumns.Add(referencedTable.TableColumns[referencedColumn]);
      var alter = SqlDdl.Alter(referencingTable, SqlDdl.AddConstraint(foreignKey));
      var commandText = sqlDriver.Compile(alter, BuildCompilerConfig(useDatabasePrefix)).GetCommandText();
      ExecuteNonQuery(commandText);
    }

    private void CreateForeignKeyLocally(Catalog catalog, string schemaName, string tableName, string columnName,
      string referencedTableName, string referencedColumnName, string foreignKeyName, bool useDatabasePrefix = false)
    {
      var schemaToAlter = catalog.Schemas[schemaName] ?? catalog.Schemas[0];

      var referencingTable = schemaToAlter.Tables[tableName];
      var referencedTable = schemaToAlter.Tables[referencedTableName];

      var foreignKey = referencingTable.CreateForeignKey(foreignKeyName);
      foreignKey.Columns.Add(referencingTable.TableColumns[columnName]);
      foreignKey.ReferencedTable = referencedTable;
      foreignKey.ReferencedColumns.Add(referencedTable.TableColumns[referencedColumnName]);
    }

    private void CreatePrimaryKeyInDb(
      Catalog catalog, string schemaName, string tableName, string columnName, string primaryKeyName, bool useDatabasePrefix = false)
    {
      var schemaToAlter = catalog.Schemas[schemaName] ?? catalog.Schemas[0];

      var table = schemaToAlter.Tables[tableName];
      var primaryKey = table.CreatePrimaryKey(primaryKeyName, table.TableColumns[columnName]);
      var alter = SqlDdl.Alter(table, SqlDdl.AddConstraint(primaryKey));
      var commandText = sqlDriver.Compile(alter, BuildCompilerConfig(useDatabasePrefix)).GetCommandText();
      ExecuteNonQuery(commandText);
    }

    private void CreatePrimaryKeyLocally(
      Catalog catalog, string schemaName, string tableName, string columnName, string primaryKeyName)
    {
      var schemaToAlter = catalog.Schemas[schemaName] ?? catalog.Schemas[0];
      _ = schemaToAlter.Tables[tableName].CreatePrimaryKey(primaryKeyName,
        schemaToAlter.Tables[tableName].TableColumns[columnName]);
    }

    private void CreateIndex(Schema schema, string tableName, string indexName, string[] keyColumns, string[] includedColumns)
    {
      var table = schema.Tables[tableName];
      var index = table.CreateIndex(indexName);

      foreach (var keyColumn in keyColumns) {
        _ = index.CreateIndexColumn(table.TableColumns[keyColumn]);
      }

      if (includedColumns != null) {
        foreach(var includedColumn in includedColumns) {
          index.NonkeyColumns.Add(table.TableColumns[includedColumn]);
        }
      }

      var create = SqlDdl.Create(index);
      var commandText = sqlDriver.Compile(create, BuildCompilerConfig(false)).GetCommandText();
      ExecuteNonQuery(commandText);
    }

    private void CreateColumn(
      Catalog catalog, string schemaName, string tableName, string columnName, SqlValueType columnType, bool useDatabasePrefix = false)
    {
      var schemaToAlter = catalog.Schemas[schemaName] ?? catalog.Schemas[0];
      var table = schemaToAlter.Tables[tableName];
      var column = table.CreateColumn(columnName, columnType);
      column.IsNullable = true;
      var alter = SqlDdl.Alter(table, SqlDdl.AddColumn(column));
      var commandText = sqlDriver.Compile(alter, BuildCompilerConfig(useDatabasePrefix)).GetCommandText();
      ExecuteNonQuery(commandText);
    }

    private void CreateTable(Catalog catalog, string schema, string tableName, string[] columns, SqlValueType[] columnTypes, bool useDatabasePrefix = false)
    {
      var schemaToAlter = catalog.Schemas[schema] ?? catalog.Schemas[0];
      var table = schemaToAlter.CreateTable(tableName);

      for (var i = 0; i < columns.Length; i++) {
        _ = new TableColumn(table, columns[i], columnTypes[i]);
      }

      var create = SqlDdl.Create(table);
      var commandText = sqlDriver.Compile(create, BuildCompilerConfig(useDatabasePrefix)).GetCommandText();
      ExecuteNonQuery(commandText);
    }

    private System.Action CreateTableDelayed(
      Catalog catalog, string schemaName, string tableName, string[] columnNames, SqlValueType[] columnTypes, bool useDatabasePrefix = false)
    {
      var schemaToAlter = catalog.Schemas[schemaName] ?? catalog.Schemas[0];
      var table = schemaToAlter.CreateTable(tableName);

      for (var i = 0; i < columnNames.Length; i++) {
        _ = new TableColumn(table, columnNames[i], columnTypes[i]);
      }

      var create = SqlDdl.Create(table);
      var commandText = sqlDriver.Compile(create, BuildCompilerConfig(useDatabasePrefix)).GetCommandText();

      return () => ExecuteNonQuery(commandText);
    }

    private void CreateSchema(Catalog catalog, string schemaName)
    {
      if (catalog.Schemas[schemaName] == null) {
        var schema = catalog.CreateSchema(schemaName);
        var schemaCreate = SqlDdl.Create(schema);
        var commandText = sqlDriver.Compile(schemaCreate).GetCommandText();
        ExecuteNonQuery(commandText);
      }
    }

    private static SqlCompilerConfiguration BuildCompilerConfig(bool useDatabasePrefix) =>
      new SqlCompilerConfiguration {
        DatabaseQualifiedObjects = useDatabasePrefix
      };

    private void SetMultidatabase()
    {
      isMultidatabaseTest = true;
      isMultischemaTest = false;
    }

    private void SetMultischema()
    {
      isMultidatabaseTest = false;
      isMultischemaTest = true;
    }

    private void ClearMultidatabaseAndMultischemaFlags()
    {
      isMultidatabaseTest = false;
      isMultischemaTest = false;
    }

    private SqlValueType GetTypeForInteger(SqlType sqlType)
    {
      if (!StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.Oracle)) {
        return new SqlValueType(sqlType);
      }

      const int ShortPrecision = 5;
      const int IntPrecision = 10;
      const int LongPrecision = 20;

      return sqlType == SqlType.Int16
        ? new SqlValueType(SqlType.Decimal, ShortPrecision, 0)
        : sqlType == SqlType.Int32
          ? new SqlValueType(SqlType.Decimal, IntPrecision, 0)
          : sqlType == SqlType.Int64
            ? new SqlValueType(SqlType.Decimal, LongPrecision, 0)
            : new SqlValueType(sqlType);
    }
  }
}
