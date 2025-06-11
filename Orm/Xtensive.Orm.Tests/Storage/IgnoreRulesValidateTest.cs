// Copyright (C) 2013-2021 Xtensive LLC.
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
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Model1 = Xtensive.Orm.Tests.Storage.IgnoreRulesValidateModel1;
using ignorablePart = Xtensive.Orm.Tests.Storage.IgnoreRulesValidateModel2;
using Model2 = Xtensive.Orm.Tests.Storage.IgnoreRulesValidateModel3;
using Model3 = Xtensive.Orm.Tests.Storage.IgnoreRulesValidateModel4;
using Model4 = Xtensive.Orm.Tests.Storage.IgnoreRulesValidateModel5;

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

    [Field]
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

      IndexDef indexDef;
      if (orderType.Indexes.TryGetValue("IndexName", out indexDef)) {
        var fieldsBefore = indexDef.KeyFields.ToList();
        indexDef.KeyFields.Clear();
        indexDef.KeyFields.Add("SomeIgnoredField", Direction.Positive);
        foreach (var keyValuePair in fieldsBefore)
          indexDef.KeyFields.Add(keyValuePair.Key, keyValuePair.Value);
      }

      var ignoredTable = model.Types[typeof (IgnoredTable)];
      if (ignoredTable==null)
        return;

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

    [Field]
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

    [Field]
    public string FirstColumn { get; set; }
  }
  [HierarchyRoot]
  public class MyEntity2 : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public string FirstColumn { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage.IgnoreRulesValidateModel5
{
  [HierarchyRoot]
  public class MyEntity1 : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public string FirstColumn { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class IgnoreRulesValidateTest
  {
    private const string defaultSchema = WellKnownSchemas.Schema1;
    private const string Schema1 = WellKnownSchemas.Schema2;
    private const string Schema2 = WellKnownSchemas.Schema3;

    private SqlDriver sqlDriver;
    private Key changedOrderKey;
    private bool createConstraintsWithTable;
    private bool isMultidatabaseTest;
    private bool isMultischemaTest;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
      sqlDriver = TestSqlDriver.Create(GetConnectionInfo());
      if(StorageProviderInfo.Instance.Provider == StorageProvider.Sqlite) {
        createConstraintsWithTable = true;
      }
    }

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
    public void IgnoreSimpleColumnTest()
    {
      BuildSimpleDomain(DomainUpgradeMode.Recreate, null, typeof(Model3.MyEntity1)).Dispose();

      var catalog = GetCatalog();
      var schema = catalog.DefaultSchema.Name;
      CreateColumn(catalog, schema, "MyEntity2", "SimpleIgnoredColumn", GetTypeForInteger(SqlType.Int32));
      var ignoreRuleCollection = new IgnoreRuleCollection();
      _ = ignoreRuleCollection.IgnoreColumn("SimpleIgnoredColumn").WhenTable("MyEntity2");

      BuildSimpleDomain(DomainUpgradeMode.Validate, ignoreRuleCollection, typeof(Model3.MyEntity1)).Dispose();
    }

    [Test]
    public void IgnoreReferencedColumnTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ForeignKeyConstraints);

      BuildSimpleDomain(DomainUpgradeMode.Recreate, null, typeof(Model3.MyEntity1)).Dispose();

      var catalog = GetCatalog();
      var schema = catalog.DefaultSchema.Name;
      CreateColumn(catalog, schema, "MyEntity2", "ReferencedIgnoredColumn", GetTypeForInteger(SqlType.Int64));
      CreateForeignKeyInDb(catalog, schema, "MyEntity2", "ReferencedIgnoredColumn", "MyEntity1", "Id", "FK_MyEntity1_MyEntity1ID");
      var ignoreRuleCollection = new IgnoreRuleCollection();
      _ = ignoreRuleCollection.IgnoreColumn("ReferencedIgnoredColumn").WhenTable("MyEntity2").WhenSchema(schema);

      BuildSimpleDomain(DomainUpgradeMode.Validate, ignoreRuleCollection, typeof(Model3.MyEntity1)).Dispose();
    }

    [Test]
    public void IgnoreSimpleTableTest()
    {
      BuildSimpleDomain(DomainUpgradeMode.Recreate, null, typeof(Model3.MyEntity1)).Dispose();

      var catalog = GetCatalog();
      var schema = catalog.DefaultSchema.Name;
      var addedColumnsNames = new[] { "Id", "FirstColumn" };
      var addedColumnsTypes = new[] { GetTypeForInteger(SqlType.Int32), GetTypeForInteger(SqlType.Int64) };

      if (createConstraintsWithTable) {
        var delayedOp = CreateTableDelayed(catalog, schema, "MyIgnoredEntity", addedColumnsNames, addedColumnsTypes);
        CreatePrimaryKeyLocally(catalog, schema, "MyIgnoredEntity", "Id", "PK_MyIgnoredEntity_Id");
        delayedOp();
      }
      else {
        CreateTable(catalog, schema, "MyIgnoredEntity", addedColumnsNames, addedColumnsTypes);
        CreatePrimaryKeyInDb(catalog, schema, "MyIgnoredEntity", "Id", "PK_MyIgnoredEntity_Id");
      }
      var ignoreRules = new IgnoreRuleCollection();
      _ = ignoreRules.IgnoreTable("MyIgnoredEntity");

      BuildSimpleDomain(DomainUpgradeMode.Validate, ignoreRules, typeof(Model3.MyEntity1)).Dispose();
    }

    [Test]
    public void IgnoreReferencedTableTest()
    {
      BuildSimpleDomain(DomainUpgradeMode.Recreate, null, typeof(Model3.MyEntity1)).Dispose();

      var catalog = GetCatalog();
      var schema = catalog.DefaultSchema.Name;
      var addedColumnsNames = new[] { "Id", "FirstColumn", "MyEntity2Id" };
      var addedColumnsTypes = new[] { GetTypeForInteger(SqlType.Int32), GetTypeForInteger(SqlType.Int64), GetTypeForInteger(SqlType.Int64) };

      if (createConstraintsWithTable) {
        var delayedOp = CreateTableDelayed(catalog, schema, "MyIgnoredEntity", addedColumnsNames, addedColumnsTypes);
        CreatePrimaryKeyLocally(catalog, schema, "MyIgnoredEntity", "Id", "PK_MyIgnoredEntity_Id");
        CreateForeignKeyLocally(catalog, schema, "MyIgnoredEntity", "MyEntity2Id", "MyEntity2", "Id", "FK_MyIgnoredEntityMyEntity2Id");
        delayedOp();
      }
      else {
        CreateTable(catalog, schema, "MyIgnoredEntity", addedColumnsNames, addedColumnsTypes);
        CreatePrimaryKeyInDb(catalog, schema, "MyIgnoredEntity", "Id", "PK_MyIgnoredEntity_Id");
        CreateForeignKeyInDb(catalog, schema, "MyIgnoredEntity", "MyEntity2Id", "MyEntity2", "Id", "FK_MyIgnoredEntityMyEntity2Id");
      }

      var ignoreRules = new IgnoreRuleCollection();
      _ = ignoreRules.IgnoreTable("MyIgnoredEntity");

      BuildSimpleDomain(DomainUpgradeMode.Validate, ignoreRules, typeof(Model3.MyEntity1)).Dispose();
    }

    [Test]
    public void InsertIntoTableWithIgnoredColumnTest()
    {
      BuildSimpleDomain(DomainUpgradeMode.Recreate, null, typeof(Model3.MyEntity1)).Dispose();

      var catalog = GetCatalog();
      var schema = catalog.DefaultSchema.Name;
      CreateColumn(catalog, schema, "Myentity2", "SimpleIgnoredColumn", GetTypeForInteger(SqlType.Int64));
      var ignoreRuleCollection = new IgnoreRuleCollection();
      _ = ignoreRuleCollection.IgnoreColumn("SimpleIgnoredColumn").WhenTable("MyEntity2");

      using (var validatedDomain = BuildSimpleDomain(DomainUpgradeMode.Validate, ignoreRuleCollection, typeof(Model3.MyEntity1)))
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

      BuildSimpleDomain(DomainUpgradeMode.Recreate, null, typeof(Model3.MyEntity1)).Dispose();

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
        using (var validatedDomain = BuildSimpleDomain(DomainUpgradeMode.Validate, ignoreRuleCollection, typeof(Model3.MyEntity1)))
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
      BuildSimpleDomain(DomainUpgradeMode.Recreate, null, typeof(Model3.MyEntity1)).Dispose();

      var catalog = GetCatalog();
      var schema = catalog.DefaultSchema.Name;
      CreateColumn(catalog, schema, "MyEntity2", "SimpleIgnoredColumn", GetTypeForInteger(SqlType.Int64));

      var ignoreRuleCollection = new IgnoreRuleCollection();
      _ = ignoreRuleCollection.IgnoreColumn("SimpleIgnoredColumn").WhenTable("MyEntity2");

      _ = Assert.Throws<SchemaSynchronizationException>(() => {
        BuildSimpleDomain(DomainUpgradeMode.Perform, ignoreRuleCollection, typeof(Model4.MyEntity1)).Dispose();
      });
    }

    [Test]
    public void DropReferencedTableTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ForeignKeyConstraints);

      BuildSimpleDomain(DomainUpgradeMode.Recreate, null, typeof(Model3.MyEntity1)).Dispose();

      var catalog = GetCatalog();
      var schema = catalog.DefaultSchema.Name;
      CreateColumn(catalog, schema, "MyEntity2", "ReferencedIgnoredColumn", GetTypeForInteger(SqlType.Int64));
      CreateForeignKeyInDb(catalog, schema, "MyEntity2", "ReferencedIgnoredColumn", "MyEntity1", "Id", "FK_MyEntity1_MyEntity1ID");

      var ignoreRuleCollection = new IgnoreRuleCollection();
      _ = ignoreRuleCollection.IgnoreColumn("ReferencedIgnoredColumn").WhenTable("MyEntity2").WhenSchema(schema);

      _ = Assert.Throws<SchemaSynchronizationException>(() => {
        BuildSimpleDomain(DomainUpgradeMode.Perform, ignoreRuleCollection, typeof(Model4.MyEntity1)).Dispose();
      });
    }

    [Test]
    public void IgnoreColumnsByMaskValidateTest()
    {
      BuildSimpleDomain(DomainUpgradeMode.Recreate, null, typeof(Model3.MyEntity1)).Dispose();

      var catalog = GetCatalog();
      var schema = catalog.DefaultSchema.Name;
      CreateColumn(catalog, schema, "MyEntity2", "IgnoreFirstColumn", GetTypeForInteger(SqlType.Int64));
      CreateColumn(catalog, schema, "MyEntity2", "IgnoreSecondColumn", GetTypeForInteger(SqlType.Int64));
      CreateColumn(catalog, schema, "MyEntity2", "IgnoreThirdColumn", GetTypeForInteger(SqlType.Int64));
      var ignoreRules = new IgnoreRuleCollection();
      _ = ignoreRules.IgnoreColumn("Ignore*");

      BuildSimpleDomain(DomainUpgradeMode.Validate, ignoreRules, typeof(Model3.MyEntity1)).Dispose();
    }

    [Test]
    public void IgnoreTablesByMaskValidateTest()
    {
      BuildSimpleDomain(DomainUpgradeMode.Recreate, null, typeof(Model3.MyEntity1)).Dispose();

      var catalog = GetCatalog();
      var schema = catalog.DefaultSchema.Name;
      var addedColumnsNames = new[] { "Id", "FirstColumn", "SecondColumn" };
      var addedColumnsTypes = new[] { GetTypeForInteger(SqlType.Int64), GetTypeForInteger(SqlType.Int64), GetTypeForInteger(SqlType.Int64) };

      if (createConstraintsWithTable) {
        var delayedOp = CreateTableDelayed(catalog, schema, "IgnoredFirstTable", addedColumnsNames, addedColumnsTypes);
        CreatePrimaryKeyLocally(catalog, schema, "IgnoredFirstTable", "Id", "PK_IgnoredFirstTable_Id");
        delayedOp();

        delayedOp = CreateTableDelayed(catalog, schema, "IgnoredSecondTable", addedColumnsNames, addedColumnsTypes);
        CreatePrimaryKeyLocally(catalog, schema, "IgnoredSecondTable", "Id", "PK_IgnoredSecondTable_Id");
        delayedOp();
      }
      else {
        CreateTable(catalog, schema, "IgnoredFirstTable", addedColumnsNames, addedColumnsTypes);
        CreatePrimaryKeyInDb(catalog, schema, "IgnoredFirstTable", "Id", "PK_IgnoredFirstTable_Id");

        CreateTable(catalog, schema, "IgnoredSecondTable", addedColumnsNames, addedColumnsTypes);
        CreatePrimaryKeyInDb(catalog, schema, "IgnoredSecondTable", "Id", "PK_IgnoredSecondTable_Id");
      }
      var ignoreRules = new IgnoreRuleCollection();
      _ = ignoreRules.IgnoreTable("Ignored*");

      BuildSimpleDomain(DomainUpgradeMode.Validate, ignoreRules, typeof(Model3.MyEntity1)).Dispose();
    }

    [Test]
    public void IgnoreAllColumnsInTableByMaskValidateTest()
    {
      BuildSimpleDomain(DomainUpgradeMode.Recreate, null, typeof(Model3.MyEntity1)).Dispose();

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

      BuildSimpleDomain(DomainUpgradeMode.Validate, ignoreRules, typeof(Model3.MyEntity1)).Dispose();
    }

    [Test]
    public void UpgradeDomainWithIgnoreRuleByMaskInPerformModeTest()
    {
      BuildSimpleDomain(DomainUpgradeMode.Recreate, null, typeof(Model3.MyEntity1)).Dispose();

      var catalog = GetCatalog();
      var schema = catalog.Schemas[catalog.DefaultSchema.Name] ?? catalog.Schemas[0];

      CreateColumn(catalog, schema.Name, "MyEntity2", "IgnoredFirstColumn", GetTypeForInteger(SqlType.Int64));
      CreateColumn(catalog, schema.Name, "MyEntity2", "IgnoredSecondColumn", GetTypeForInteger(SqlType.Int64));

      var ignoreRules = new IgnoreRuleCollection();
      _ = ignoreRules.IgnoreColumn("Ignored*").WhenTable("MyEntity2");

      using (var domain = BuildSimpleDomain(DomainUpgradeMode.Perform, ignoreRules, typeof(Model3.MyEntity1)))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new Model3.MyEntity1 { FirstColumn = "Some text" };
        _ = new Model3.MyEntity2 { FirstColumn = "Second some test" };
        transaction.Complete();
      }
      var myEntity2 = SqlDml.TableRef(schema.Tables["MyEntity2"]);
      var select = SqlDml.Select(myEntity2);
      var compileConfiguration = new SqlCompilerConfiguration();
      compileConfiguration.DatabaseQualifiedObjects = false;
      var commandText = sqlDriver.Compile(select, compileConfiguration).GetCommandText();
      Assert.That(ExecuteQuery(commandText, 2), Is.EqualTo(DBNull.Value));
      Assert.That(ExecuteQuery(commandText, 3), Is.EqualTo(DBNull.Value));
    }

    [Test]
    public void UpgradeDomainWithIgnoreRuleByMaskInPerformSafelyModeTest()
    {
      BuildSimpleDomain(DomainUpgradeMode.Recreate, null, typeof(Model3.MyEntity1)).Dispose();

      var catalog = GetCatalog();
      var schema = catalog.Schemas[catalog.DefaultSchema.Name] ?? catalog.Schemas[0];
      CreateColumn(catalog, schema.Name, "MyEntity2", "IgnoredFirstColumn", GetTypeForInteger(SqlType.Int64));
      CreateColumn(catalog, schema.Name, "MyEntity2", "IgnoredSecondColumn", GetTypeForInteger(SqlType.Int64));

      var ignoreRules = new IgnoreRuleCollection();
      _ = ignoreRules.IgnoreColumn("Ignored*");

      using (var domain = BuildSimpleDomain(DomainUpgradeMode.PerformSafely, ignoreRules, typeof(Model3.MyEntity1)))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new Model3.MyEntity1 { FirstColumn = "Some text" };
        _ = new Model3.MyEntity2 { FirstColumn = "Second some test" };
        transaction.Complete();
      }
      var myEntity2 = SqlDml.TableRef(schema.Tables["MyEntity2"]);
      var select = SqlDml.Select(myEntity2);
      var compileConfiguration = new SqlCompilerConfiguration();
      compileConfiguration.DatabaseQualifiedObjects = false;
      var commandText = sqlDriver.Compile(select, compileConfiguration).GetCommandText();
      Assert.That(ExecuteQuery(commandText, 2), Is.EqualTo(DBNull.Value));
      Assert.That(ExecuteQuery(commandText, 3), Is.EqualTo(DBNull.Value));
    }

    [Test]
    public void MultischemaValidateTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multischema);

      SetMultischema();

      var catalog = GetCatalog();
      CreateSchema(catalog, Schema1);
      CreateSchema(catalog, Schema2);

      BuildComplexDomain(DomainUpgradeMode.Recreate, null, new[] { typeof(Model1.Customer) }, new[] { typeof(Model3.MyEntity1) }).Dispose();

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

      BuildComplexDomain(DomainUpgradeMode.Validate, ignoreRules, new[] { typeof(Model1.Customer) }, new[] { typeof(Model3.MyEntity1) }).Dispose();
    }

    [Test]
    public void MultidatabaseValidateTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multidatabase);

      SetMultidatabase();

      BuildComplexDomain(DomainUpgradeMode.Recreate, null, new[] { typeof(Model1.Customer) }, new[] { typeof(Model3.MyEntity1) }).Dispose();

      var secondCatalog = GetCatalog(Multimapping.MultidatabaseTest.Database2Name);
      CreateColumn(secondCatalog, defaultSchema, "MyEntity2", "ReferencedIgnoredColumn", GetTypeForInteger(SqlType.Int64), true);
      CreateForeignKeyInDb(secondCatalog, defaultSchema, "MyEntity2", "ReferencedIgnoredColumn", "MyEntity1", "Id", "FK_MyEntity2_MyEntity1_MyEntity1ID", true);

      var addedColumnsNames = new[] { "Id", "FirstColumn", "MyEntity2Id" };
      var addedColumnsTypes = new[] { GetTypeForInteger(SqlType.Int32), GetTypeForInteger(SqlType.Int64), GetTypeForInteger(SqlType.Int64) };
      CreateTable(secondCatalog, defaultSchema, "MyIgnoredEntity", addedColumnsNames, addedColumnsTypes, true);
      CreatePrimaryKeyInDb(secondCatalog, defaultSchema, "MyIgnoredEntity", "Id", "PK_MyIgnoreTable_Id", true);
      CreateForeignKeyInDb(secondCatalog, defaultSchema, "MyIgnoredEntity", "MyEntity2Id", "MyEntity2", "Id", "FK_MyIgnoredEntity_MyEntity2_Id", true);

      var ignoreRules = new IgnoreRuleCollection();
      _ = ignoreRules.IgnoreColumn("ReferencedIgnoredColumn").WhenSchema(defaultSchema).WhenDatabase(Multimapping.MultidatabaseTest.Database2Name);
      _ = ignoreRules.IgnoreTable("MyIgnoredEntity").WhenDatabase(Multimapping.MultidatabaseTest.Database2Name);
      _ = ignoreRules.IgnoreTable("IgnoredTable").WhenDatabase(Multimapping.MultidatabaseTest.Database1Name);
      _ = ignoreRules.IgnoreColumn("SomeIgnoredField").WhenTable("Order").WhenDatabase(Multimapping.MultidatabaseTest.Database1Name);
      _ = ignoreRules.IgnoreColumn("IgnoredColumn.Id").WhenTable("Author").WhenDatabase(Multimapping.MultidatabaseTest.Database1Name);

      BuildComplexDomain(DomainUpgradeMode.Validate, ignoreRules, new[] { typeof(Model1.Customer) }, new[] { typeof(Model3.MyEntity1) }).Dispose();
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
      CreateColumn(secondCatalog, defaultSchema, "MyEntity2", "ReferencedIgnoredColumn", GetTypeForInteger(SqlType.Int64), true);
      CreateForeignKeyInDb(secondCatalog, defaultSchema, "MyEntity2", "ReferencedIgnoredColumn", "MyEntity1", "Id", "FK_MyEntity2_MyEntity1_MyEntity1ID", true);

      var addedColumnsNames = new[] { "Id", "FirstColumn", "MyEntity2Id" };
      var addedColumnsTypes = new[] { GetTypeForInteger(SqlType.Int32), GetTypeForInteger(SqlType.Int64), GetTypeForInteger(SqlType.Int64) };
      CreateTable(secondCatalog, defaultSchema, "MyIgnoredEntity", addedColumnsNames, addedColumnsTypes, true);
      CreatePrimaryKeyInDb(secondCatalog, defaultSchema, "MyIgnoredEntity", "Id", "PK_MyIgnoreTable_Id", true);
      CreateForeignKeyInDb(secondCatalog, defaultSchema, "MyIgnoredEntity", "MyEntity2Id", "MyEntity2", "Id", "FK_MyIgnoredEntity_MyEntity2_Id", true);

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
      ValidateMyEntity(secondCatalog, defaultSchema, true);
    }

    [Test]
    public void MultidatabaseUpgradeInPerformSafelyModeTest()
    {
      SetMultidatabase();
      Require.AllFeaturesSupported(ProviderFeatures.Multidatabase);
      isMultidatabaseTest = true;
      BuildDomainAndFillData();

      var secondCatalog = GetCatalog(Multimapping.MultidatabaseTest.Database2Name);
      CreateColumn(secondCatalog, defaultSchema, "MyEntity2", "ReferencedIgnoredColumn", GetTypeForInteger(SqlType.Int64), true);
      CreateForeignKeyInDb(secondCatalog, defaultSchema, "MyEntity2", "ReferencedIgnoredColumn", "MyEntity1", "Id", "FK_MyEntity2_MyEntity1_MyEntity1ID", true);
      var addedColumnsNames = new[] { "Id", "FirstColumn", "MyEntity2Id" };
      var addedColumnsTypes = new[] { GetTypeForInteger(SqlType.Int32), GetTypeForInteger(SqlType.Int64), GetTypeForInteger(SqlType.Int64) };
      CreateTable(secondCatalog, defaultSchema, "MyIgnoredEntity", addedColumnsNames, addedColumnsTypes, true);
      CreatePrimaryKeyInDb(secondCatalog, defaultSchema, "MyIgnoredEntity", "Id", "PK_MyIgnoreTable_Id", true);
      CreateForeignKeyInDb(secondCatalog, defaultSchema, "MyIgnoredEntity", "MyEntity2Id", "MyEntity2", "Id", "FK_MyIgnoredEntity_MyEntity2_Id", true);

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
      ValidateMyEntity(secondCatalog, defaultSchema, true);
    }

    private Domain BuildSimpleDomain(DomainUpgradeMode mode, IgnoreRuleCollection ignoreRules, Type sourceType, params Type[] additionalSourceTypes)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = mode;
      configuration.Types.RegisterCaching(sourceType.Assembly, sourceType.Namespace);

      if (additionalSourceTypes != null) {
        additionalSourceTypes.ForEach((t) => configuration.Types.RegisterCaching(t.Assembly, t.Namespace));
      }
      if (ignoreRules != null) {
        configuration.IgnoreRules = ignoreRules;
      }

      return Domain.Build(configuration);
    }

    private Domain BuildComplexDomain(DomainUpgradeMode mode, IgnoreRuleCollection ignoreRules, Type[] firstPartTypes, Type[] secondPartTypes)
    {
      var config = DomainConfigurationFactory.Create();
      config.UpgradeMode = mode;
      if (isMultischemaTest) {
        foreach (var type in firstPartTypes) {
          config.MappingRules.Map(type.Assembly, type.Namespace).ToSchema(Schema1);
        }

        foreach (var type in secondPartTypes) {
          config.MappingRules.Map(type.Assembly, type.Namespace).ToSchema(Schema2);
        }
        config.DefaultSchema = defaultSchema;
      }
      else if (isMultidatabaseTest) {
        foreach (var type in firstPartTypes) {
          config.MappingRules.Map(type.Assembly, type.Namespace).ToDatabase(Multimapping.MultidatabaseTest.Database1Name);
        }

        foreach (var type in secondPartTypes) {
          config.MappingRules.Map(type.Assembly, type.Namespace).ToDatabase(Multimapping.MultidatabaseTest.Database2Name);
        }
        config.DefaultSchema = defaultSchema;
        config.DefaultDatabase = GetConnectionInfo().ConnectionUrl.GetDatabase();
      }

      foreach (var type in firstPartTypes.Union(secondPartTypes)) {
        config.Types.RegisterCaching(type.Assembly, type.Namespace);
      }

      if (ignoreRules != null) {
        config.IgnoreRules = ignoreRules;
      }

      return Domain.Build(config);
    }

    private void BuildDomainAndFillData()
    {
      var domain = isMultidatabaseTest || isMultischemaTest
        ? BuildComplexDomain(DomainUpgradeMode.Recreate, null,
            new[] { typeof(Model1.Customer), typeof(ignorablePart.IgnoredTable) }, new[] { typeof(Model3.MyEntity1) })
        : BuildSimpleDomain(DomainUpgradeMode.Recreate, null, typeof(Model1.Customer), typeof(ignorablePart.IgnoredTable));

      using (domain)
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var author = new Model1.Author {
          FirstName = "Ivan",
          LastName = "Goncharov",
          Birthday = new DateTime(1812, 6, 18)
        };
        var book = new Model1.Book { ISBN = "9780140440409", Title = "Oblomov" };
        _ = book.Authors.Add(author);
        var customer = new Model1.Customer {
          FirstName = "Alexey",
          LastName = "Kulakov",
          Birthday = new DateTime(1988, 8, 31)
        };
        var order = new Model1.Order { Book = book, Customer = customer };
        order["SomeIgnoredField"] = "Secret information for FBI :)";

        if (isMultidatabaseTest || isMultischemaTest) {
          _ = new Model3.MyEntity1 { FirstColumn = "first" };
          _ = new Model3.MyEntity2 { FirstColumn = "first" };
        }
        transaction.Complete();
      }
    }

    private void UpgradeDomain(DomainUpgradeMode mode)
    {
      IgnoreRuleCollection ignoreRules = new IgnoreRuleCollection();
      _ = ignoreRules.IgnoreTable("IgnoredTable");
      _ = ignoreRules.IgnoreColumn("SomeIgnoredField").WhenTable("Order");
      _ = ignoreRules.IgnoreColumn("IgnoredColumn.Id").WhenTable("Author");

      using (var domain = BuildSimpleDomain(mode, ignoreRules, typeof(Model1.Customer)))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var currentCustomer = session.Query.All<Model1.Customer>().First(c => c.LastName == "Kulakov");
        var order = session.Query.All<Model1.Order>().First(o => o.Customer.LastName == currentCustomer.LastName);
        var newCustomer = new Model1.Customer { FirstName = "Fred", LastName = "Smith", Birthday = new DateTime(1998, 7, 9) };
        order.Customer = newCustomer;
        changedOrderKey = order.Key;
        transaction.Complete();
      }
    }

    private void UpgradeDomain(DomainUpgradeMode mode, IgnoreRuleCollection ignoreRules)
    {
      using (var performDomain = BuildComplexDomain(mode, ignoreRules, new[] { typeof(Model1.Customer) }, new[] { typeof(Model3.MyEntity1) }))
      using (var session = performDomain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var currentCustomer = session.Query.All<Model1.Customer>().First(c => c.LastName == "Kulakov");
        var order = session.Query.All<Model1.Order>().First(o => o.Customer.LastName == currentCustomer.LastName);
        var newCustomer = new Model1.Customer { FirstName = "Fred", LastName = "Smith", Birthday = new DateTime(1998, 7, 9) };
        order.Customer = newCustomer;
        changedOrderKey = order.Key;
        var currentEntity = session.Query.All<Model3.MyEntity2>().First(ent => ent.FirstColumn == "first");
        currentEntity.FirstColumn = "second";
        transaction.Complete();
      }
    }

    private void BuildDomainInValidateMode()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Validate;
      configuration.Types.RegisterCaching(typeof(Model1.Customer).Assembly, typeof(Model1.Customer).Namespace);
      configuration.Types.RegisterCaching(typeof(ignorablePart.IgnoredTable).Assembly, typeof(ignorablePart.IgnoredTable).Namespace);
      using (var domain = Domain.Build(configuration))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var result = session.Query.All<Model1.Order>().First(o => o.Key == changedOrderKey);
        Assert.That(result["SomeIgnoredField"], Is.EqualTo("Secret information for FBI :)"));
      }
    }

    private void BuildDomainInValidateMode(IgnoreRuleCollection ignoreRules)
    {
      var validateDomain = BuildComplexDomain(DomainUpgradeMode.Validate, ignoreRules,
        new[] { typeof(Model1.Customer), typeof(ignorablePart.IgnoredTable) }, new[] { typeof(Model3.MyEntity1) });

      using (validateDomain)
      using (var session = validateDomain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var result = session.Query.All<Model1.Order>().First(o => o.Key == changedOrderKey);
        Assert.That(result["SomeIgnoredField"], Is.EqualTo("Secret information for FBI :)"));
      }
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

    private ConnectionInfo GetConnectionInfo() => DomainConfigurationFactory.Create().ConnectionInfo;

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

    private void ValidateMyEntity(Catalog catalog, string schema, bool useDatabasePrefix = false)
    {
      var schemaToValidete = catalog.Schemas[schema] ?? catalog.Schemas[0];

      var myEntity2 = SqlDml.TableRef(schemaToValidete.Tables["MyEntity2"]);
      var select = SqlDml.Select(myEntity2);
      var commandText = sqlDriver.Compile(select, BuildCompilerConfig(useDatabasePrefix)).GetCommandText();
      Assert.That(ExecuteQuery(commandText, 2), Is.EqualTo(DBNull.Value));
    }

    private void CreateForeignKeyInDb(Catalog catalog, string schema, string table, string column, string referencedTable,
      string referencedColumn, string foreignKeyName, bool useDatabasePrefix = false)
    {
      var schemaToAlter = catalog.Schemas[schema] ?? catalog.Schemas[0];

      var foreignKey = schemaToAlter.Tables[table].CreateForeignKey(foreignKeyName);
      foreignKey.Columns.Add(schemaToAlter.Tables[table].TableColumns[column]);
      foreignKey.ReferencedTable = schemaToAlter.Tables[referencedTable];
      foreignKey.ReferencedColumns.Add(schemaToAlter.Tables[referencedTable].TableColumns[referencedColumn]);
      var alter = SqlDdl.Alter(schemaToAlter.Tables[table], SqlDdl.AddConstraint(foreignKey));
      var commandText = sqlDriver.Compile(alter, BuildCompilerConfig(useDatabasePrefix)).GetCommandText();
      ExecuteNonQuery(commandText);
    }

    private void CreateForeignKeyLocally(Catalog catalog, string schema, string table, string column, string referencedTable,
      string referencedColumn, string foreignKeyName, bool useDatabasePrefix = false)
    {
      var schemaToAlter = catalog.Schemas[schema] ?? catalog.Schemas[0];
      var foreignKey = schemaToAlter.Tables[table].CreateForeignKey(foreignKeyName);
      foreignKey.Columns.Add(schemaToAlter.Tables[table].TableColumns[column]);
      foreignKey.ReferencedTable = schemaToAlter.Tables[referencedTable];
      foreignKey.ReferencedColumns.Add(schemaToAlter.Tables[referencedTable].TableColumns[referencedColumn]);
    }

    private void CreatePrimaryKeyInDb(Catalog catalog, string schema, string table, string column, string primaryKeyName, bool useDatabasePrefix = false)
    {
      var schemaToAlter = catalog.Schemas[schema] ?? catalog.Schemas[0];

      var primaryKey = schemaToAlter.Tables[table].CreatePrimaryKey(primaryKeyName,
        schemaToAlter.Tables[table].TableColumns[column]);
      var alter = SqlDdl.Alter(schemaToAlter.Tables[table], SqlDdl.AddConstraint(primaryKey));
      var commandText = sqlDriver.Compile(alter, BuildCompilerConfig(useDatabasePrefix)).GetCommandText();
      ExecuteNonQuery(commandText);
    }

    private void CreatePrimaryKeyLocally(Catalog catalog, string schema, string table, string column, string primaryKeyName)
    {
      var schemaToAlter = catalog.Schemas[schema] ?? catalog.Schemas[0];
      _ = schemaToAlter.Tables[table].CreatePrimaryKey(primaryKeyName,
        schemaToAlter.Tables[table].TableColumns[column]);
    }

    private void CreateColumn(Catalog catalog, string schema, string table, string columnName, SqlValueType columnType, bool useDatabasePrefix = false)
    {
      var schemaToAlter = catalog.Schemas[schema] ?? catalog.Schemas[0];
      var column = schemaToAlter.Tables[table].CreateColumn(columnName, columnType);
      column.IsNullable = true;
      var alter = SqlDdl.Alter(schemaToAlter.Tables[table], SqlDdl.AddColumn(column));
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

    private System.Action CreateTableDelayed(Catalog catalog, string schema, string tableName, string[] columns, SqlValueType[] columnTypes, bool useDatabasePrefix = false)
    {
      var schemaToAlter = catalog.Schemas[schema] ?? catalog.Schemas[0];
      var table = schemaToAlter.CreateTable(tableName);

      for (var i = 0; i < columns.Length; i++) {
        _ = new TableColumn(table, columns[i], columnTypes[i]);
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

    private static SqlCompilerConfiguration BuildCompilerConfig(bool useDatabasePrefix)
    {
      return new SqlCompilerConfiguration {
        DatabaseQualifiedObjects = useDatabasePrefix
      };
    }

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

      if (sqlType == SqlType.Int16) {
        return new SqlValueType(SqlType.Decimal, ShortPrecision, 0);
      }
      if (sqlType == SqlType.Int32) {
        return new SqlValueType(SqlType.Decimal, IntPrecision, 0);
      }
      if (sqlType == SqlType.Int64) {
        return new SqlValueType(SqlType.Decimal, LongPrecision, 0);
      }
      return new SqlValueType(sqlType);
    }
  }
}
