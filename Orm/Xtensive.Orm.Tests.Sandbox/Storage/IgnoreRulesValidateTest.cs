// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.08.16

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Sql;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Xtensive.Sql.Tests;
using Model1 = Xtensive.Orm.Tests.Storage.IgnoreRulesValidateModel1;
using Model2 = Xtensive.Orm.Tests.Storage.IgnoreRulesValidateModel2;
using Model3 = Xtensive.Orm.Tests.Storage.IgnoreRulesValidateModel3;
using Model4 = Xtensive.Orm.Tests.Storage.IgnoreRulesValidateModel4;

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

    [Field]
    public IgnoredTable IgnoredColumn { get; set; }
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
  [Index("SomeIgnoredField", "Book", "Customer")]
  public class Order : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public string SomeIgnoredField { get; set; }

    [Field]
    [Association(PairTo = "Orders")]
    public Book Book { get; set; }

    [Field]
    [Association(PairTo = "Orders")]
    public Customer Customer { get; set; }
  }

  [HierarchyRoot]
  public class IgnoredTable : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public string SomeField { get; set; }

    [Field]
    public Book Book { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field]
    public Order Order { get; set; }

    [Field]
    [Association(PairTo = "IgnoredColumn")]
    public Author Author { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage.IgnoreRulesValidateModel2
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

namespace Xtensive.Orm.Tests.Storage.IgnoreRulesValidateModel3
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
}

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class IgnoreRulesValidateTest
  {
    private SqlDriver sqlDriver;
    private Key changedOrderKey;
    private bool isMultidatabaseTest;
    private bool isMultischemaTest;

    [TestFixtureSetUp]
    public void Setup()
    {
      sqlDriver = TestSqlDriver.Create(GetConnectionInfo());
    }

    [Test]
    public void PerformUpdateTest()
    {
      SetNoMulti();
      BuildDomainAndFillData();
      UpgradeDomain(DomainUpgradeMode.Perform);
      BuildDomainInValidateMode();
    }
    
    [Test]
    public void PerformSafelyUpdateTest()
    {
      SetNoMulti();
      BuildDomainAndFillData();
      UpgradeDomain(DomainUpgradeMode.PerformSafely);
      BuildDomainInValidateMode();
    }

    [Test]
    public void IgnoreSimpleColumnTest()
    {
      SetNoMulti();
      var initialDomain = BuildDomain(DomainUpgradeMode.Recreate, typeof (Model3.MyEntity1));
      initialDomain.Dispose();
      Catalog catalog = GetCatalog();
      CreateColumn(catalog, "dbo", "MyEntity2", "SimpleIgnoredColumn", SqlType.VarChar);
      IgnoreRuleCollection ignoreRuleCollection = new IgnoreRuleCollection();
      ignoreRuleCollection.IgnoreColumn("SimpleIgnoredColumn").WhenTable("MyEntity2");
      var validatedDomain = BuildDomain(DomainUpgradeMode.Validate, typeof (Model3.MyEntity1), ignoreRuleCollection);
      validatedDomain.Dispose();
    }

    [Test]
    public void IgnoreReferencedColumnTest()
    {
      SetNoMulti();
      var initialDomain = BuildDomain(DomainUpgradeMode.Recreate, typeof (Model3.MyEntity1));
      initialDomain.Dispose();
      Catalog catalog = GetCatalog();
      CreateColumn(catalog, "dbo", "MyEntity2", "ReferencedIgnoredColumn", SqlType.Int64);
      CreateForeignKey(catalog, "dbo", "MyEntity2", "ReferencedIgnoredColumn", "MyEntity1", "Id", "FK_MyEntity2_MyEntity1_MyEntity1ID");
      var ignoreRuleCollection = new IgnoreRuleCollection();
      ignoreRuleCollection.IgnoreColumn("ReferencedIgnoredColumn").WhenTable("MyEntity2").WhenSchema("dbo");
      var validatedDomain = BuildDomain(DomainUpgradeMode.Validate, typeof (Model3.MyEntity1), ignoreRuleCollection);
      validatedDomain.Dispose();
    }

    [Test]
    public void IgnoreSimpleTableTest()
    {
      SetNoMulti();
      var initDomain = BuildDomain(DomainUpgradeMode.Recreate, typeof (Model3.MyEntity1));
      initDomain.Dispose();

      Catalog catalog = GetCatalog();
      var addedColumnsNames = new[] {"Id", "FirstColumn"};
      var addedColumnsTypes = new[] {SqlType.Int32, SqlType.VarChar};
      CreateTable(catalog, "dbo", "MyIgnoredEntity", addedColumnsNames, addedColumnsTypes);
      CreatePrimaryKey(catalog, "dbo", "MyIgnoredEntity", "Id", "PK_MyIgnoredEntity_Id");
      var ignoreRules = new IgnoreRuleCollection();
      ignoreRules.IgnoreTable("MyIgnoredEntity");
      var validateDomain = BuildDomain(DomainUpgradeMode.Validate, typeof (Model3.MyEntity1), ignoreRules);
      validateDomain.Dispose();
    }

    [Test]
    public void IgnoreReferencedTableTest()
    {
      SetNoMulti();
      var initDomain = BuildDomain(DomainUpgradeMode.Recreate, typeof (Model3.MyEntity1));
      initDomain.Dispose();

      Catalog catalog = GetCatalog();
      var addedColumnsNames = new[] {"Id", "FirstColumn", "MyEntity2Id"};
      var addedColumnsTypes = new[] {SqlType.Int32, SqlType.VarChar, SqlType.Int64};
      CreateTable(catalog, "dbo", "MyIgnoredEntity", addedColumnsNames, addedColumnsTypes);
      CreatePrimaryKey(catalog, "dbo", "MyIgnoredEntity", "Id", "PK_MyIgnoredEntity_Id");
      CreateForeignKey(catalog, "dbo", "MyIgnoredEntity", "MyEntity2Id", "MyEntity2", "Id", "FK_MyIgnoredEntity_MyEntity2_Id");
      var ignoreRules = new IgnoreRuleCollection();
      ignoreRules.IgnoreTable("MyIgnoredEntity");
      var validateDomain = BuildDomain(DomainUpgradeMode.Validate, typeof (Model3.MyEntity1), ignoreRules);
      validateDomain.Dispose();
    }

    [Test]
    public void InsertIntoTableWithIgnoredColumnTest()
    {
      SetNoMulti();
      var initialDomain = BuildDomain(DomainUpgradeMode.Recreate, typeof (Model3.MyEntity1));
      initialDomain.Dispose();

      Catalog catalog = GetCatalog();
      CreateColumn(catalog, "dbo", "Myentity2", "SimpleIgnoredColumn", SqlType.VarChar);
      IgnoreRuleCollection ignoreRuleCollection = new IgnoreRuleCollection();
      ignoreRuleCollection.IgnoreColumn("SimpleIgnoredColumn").WhenTable("MyEntity2");
      using (var validatedDomain = BuildDomain(DomainUpgradeMode.Validate, typeof (Model3.MyEntity1), ignoreRuleCollection))
      using (var session = validatedDomain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new Model3.MyEntity2() { FirstColumn = "some test" };
        transaction.Complete();
      }
      ValidateMyEntity(catalog, "dbo");
    }

    [Test]
    [ExpectedException(typeof(CheckConstraintViolationException))]
    public void InsertIntoTableWithNotNullableIgnoredColumnTest()
    {
      SetNoMulti();
      var initialDomain = BuildDomain(DomainUpgradeMode.Recreate, typeof (Model3.MyEntity1));
      initialDomain.Dispose();

      Catalog catalog = GetCatalog();
      catalog.Schemas["dbo"].Tables["MyEntity2"].CreateColumn("SimpleIgnoredColumn", new SqlValueType(SqlType.VarChar, 30)).IsNullable = false;
      SqlAlterTable alter = SqlDdl.Alter(catalog.Schemas["dbo"].Tables["MyEntity2"],
        SqlDdl.AddColumn(catalog.Schemas["dbo"].Tables["MyEntity2"].TableColumns["SimpleIgnoredColumn"]));
      var commandText = sqlDriver.Compile(alter).GetCommandText();
      ExecuteNonQuery(commandText);
      IgnoreRuleCollection ignoreRuleCollection = new IgnoreRuleCollection();
      ignoreRuleCollection.IgnoreColumn("SimpleIgnoredColumn").WhenTable("MyEntity2");
      using (var validatedDomain = BuildDomain(DomainUpgradeMode.Validate, typeof (Model3.MyEntity1), ignoreRuleCollection))
      using (var session = validatedDomain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new Model3.MyEntity2 { FirstColumn = "some test" };
        transaction.Complete();
      }
    }

    [Test]
    [ExpectedException(typeof(SchemaSynchronizationException))]
    public void DropTableWithIgnoredColumnTest()
    {
      SetNoMulti();
      var initialDomain = BuildDomain(DomainUpgradeMode.Recreate, typeof (Model3.MyEntity1));
      initialDomain.Dispose();
      Catalog catalog = GetCatalog();
      CreateColumn(catalog, "dbo", "MyEntity2", "SimpleIgnoredColumn", SqlType.VarChar);
      IgnoreRuleCollection ignoreRuleCollection = new IgnoreRuleCollection();
      ignoreRuleCollection.IgnoreColumn("SimpleIgnoredColumn").WhenTable("MyEntity2");
      var performDomain = BuildDomain(DomainUpgradeMode.Perform, typeof (Model4.MyEntity1), ignoreRuleCollection);
      performDomain.Dispose();
    }

    [Test]
    [ExpectedException(typeof(SchemaSynchronizationException))]
    public void DropReferencedTableTest()
    {
      SetNoMulti();
      var initialDomain = BuildDomain(DomainUpgradeMode.Recreate, typeof (Model3.MyEntity1));
      initialDomain.Dispose();
      Catalog catalog = GetCatalog();
      CreateColumn(catalog, "dbo", "MyEntity2", "ReferencedIgnoredColumn", SqlType.Int64);
      CreateForeignKey(catalog, "dbo", "MyEntity2", "ReferencedIgnoredColumn", "MyEntity1", "Id", "FK_MyEntity2_MyEntity1_MyEntity1ID");
      var ignoreRuleCollection = new IgnoreRuleCollection();
      ignoreRuleCollection.IgnoreColumn("ReferencedIgnoredColumn").WhenTable("MyEntity2").WhenSchema("dbo");
      var performDomain = BuildDomain(DomainUpgradeMode.Perform, typeof(Model4.MyEntity1), ignoreRuleCollection);
      performDomain.Dispose();
    }

    private Domain BuildDomain(DomainUpgradeMode mode, Type sourceType, IgnoreRuleCollection ignoreRules = null)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = mode;
      configuration.Types.Register(sourceType.Assembly, sourceType.Namespace);
      if (ignoreRules!=null)
        configuration.IgnoreRules = ignoreRules;
      return Domain.Build(configuration);
    }

    private void BuildDomainAndFillData()
    {
      Domain domain;
      if (isMultidatabaseTest || isMultischemaTest)
        domain = BuildDomain(DomainUpgradeMode.Recreate, typeof (Model1.Customer), typeof (Model3.MyEntity1));
      else
        domain = BuildDomain(DomainUpgradeMode.Recreate, typeof (Model1.Customer));
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var author = new Model1.Author {FirstName = "Иван", LastName = "Гончаров", Birthday = new DateTime(1812, 6, 18)};
        var book = new Model1.Book {ISBN = "9780140440409", Title = "Обломов"};
        book.Authors.Add(author);
        var customer = new Model1.Customer {FirstName = "Алексей", LastName = "Кулаков", Birthday = new DateTime(1988, 8, 31)};
        var order = new Model1.Order {Book = book, Customer = customer, SomeIgnoredField = "Secret information for FBI :)"};
        if(isMultidatabaseTest || isMultischemaTest) {
          new Model3.MyEntity1 {FirstColumn = "first"};
          new Model3.MyEntity2 {FirstColumn = "first"};
        }
        transaction.Complete();
      }
    }

    private void UpgradeDomain(DomainUpgradeMode mode)
    {
      IgnoreRuleCollection ignoreRules = new IgnoreRuleCollection();
      ignoreRules.IgnoreTable("IgnoredTable");
      ignoreRules.IgnoreColumn("SomeIgnoredField").WhenTable("Order");
      ignoreRules.IgnoreColumn("IgnoredColumn.Id").WhenTable("Author");
      using (var domain = BuildDomain(mode, typeof (Model2.Customer), ignoreRules))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var currentCustomer = session.Query.All<Model2.Customer>().First(c => c.LastName=="Кулаков");
        var order = session.Query.All<Model2.Order>().First(o => o.Customer.LastName==currentCustomer.LastName);
        var newCustomer = new Model2.Customer {FirstName = "Fred", LastName = "Smith", Birthday = new DateTime(1998, 7, 9)};
        order.Customer = newCustomer;
        changedOrderKey = order.Key;
        transaction.Complete();
      }
    }
    
    private void BuildDomainInValidateMode()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Validate;
      configuration.Types.Register(typeof (Model1.Customer).Assembly, typeof (Model1.Customer).Namespace);
      using (var domain = Domain.Build(configuration))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var result = session.Query.All<Model1.Order>().First(o => o.Key==changedOrderKey);
        Assert.That(result.SomeIgnoredField, Is.EqualTo("Secret information for FBI :)"));
      }
    }

    private void BuildDomainInValidateMode(IgnoreRuleCollection ignoreRules)
    {
      using (var validateDomain = BuildDomain(DomainUpgradeMode.Validate, typeof(Model1.Customer), typeof(Model3.MyEntity1), ignoreRules))
      using (var session = validateDomain.OpenSession())
      using (var transaction = session.OpenTransaction())
      {
        var result = session.Query.All<Model1.Order>().First(o => o.Key == changedOrderKey);
        Assert.That(result.SomeIgnoredField, Is.EqualTo("Secret information for FBI :)"));
      }
    }

    private Catalog GetCatalog(string catalogName = null)
    {
      Catalog catalog;
      using (var sqlConnection = sqlDriver.CreateConnection()) {
        sqlConnection.Open();
        sqlConnection.BeginTransaction();
        if (catalogName==null)
          catalog = sqlDriver.ExtractCatalog(sqlConnection);
        else {
          var sqlExtractionTaskList = new List<SqlExtractionTask>();
          sqlExtractionTaskList.Add(new SqlExtractionTask(catalogName));
          catalog = sqlDriver.Extract(sqlConnection, sqlExtractionTaskList).Catalogs.First();
        }
        sqlConnection.Commit();
        sqlConnection.Close();
      }
      return catalog;
    }

    private ConnectionInfo GetConnectionInfo()
    {
      return TestConfiguration.Instance.GetConnectionInfo(TestConfiguration.Instance.Storage);
    }

    private void ExecuteNonQuery(string commandText)
    {
      using (var connection = sqlDriver.CreateConnection())
      {
        connection.Open();
        connection.CreateCommand(commandText).ExecuteNonQuery();
        connection.Close();
      }
    }

    private object ExecuteQuery(string commandText, int returnedColumnIndex)
    {
      var result = new object();
      using (var connection = sqlDriver.CreateConnection())
      {
        connection.Open();
        var reader = connection.CreateCommand(commandText).ExecuteReader();
        if (reader.Read())
          result = reader.GetValue(returnedColumnIndex);
        connection.Close();
      }
      return result;
    }

    private void ValidateMyEntity(Catalog catalog, string schema, bool useDatabasePrefix = false)
    {
      SqlTableRef myEntity2 = SqlDml.TableRef(catalog.Schemas[schema].Tables["MyEntity2"]);
      SqlSelect select = SqlDml.Select(myEntity2);
      select.Limit = 1;
      var compileConfiguration = new SqlCompilerConfiguration();
      compileConfiguration.DatabaseQualifiedObjects = useDatabasePrefix;
      var commandText = sqlDriver.Compile(select, compileConfiguration).GetCommandText();
      Assert.That(ExecuteQuery(commandText, 2), Is.EqualTo(DBNull.Value));
    }

    private void CreateForeignKey(Catalog catalog, string schema, string table, string column, string referencedTable,
      string referencedColumn, string foreignKeyName, bool useDatabasePrefix = false)
    {
      var fk = catalog.Schemas[schema].Tables[table].CreateForeignKey(foreignKeyName);
      fk.Columns.Add(catalog.Schemas[schema].Tables[table].TableColumns[column]);
      fk.ReferencedTable = catalog.Schemas[schema].Tables[referencedTable];
      fk.ReferencedColumns.Add(catalog.Schemas[schema].Tables[referencedTable].TableColumns[referencedColumn]);
      var alter = SqlDdl.Alter(catalog.Schemas[schema].Tables[table], SqlDdl.AddConstraint(fk));
      var sqlComplieConfig = new SqlCompilerConfiguration();
      sqlComplieConfig.DatabaseQualifiedObjects = useDatabasePrefix;
      string commandText = sqlDriver.Compile(alter, sqlComplieConfig).GetCommandText();
      ExecuteNonQuery(commandText);
    }

    private void CreatePrimaryKey(Catalog catalog, string schema, string table, string column, string primaryKeyName, bool useDatabasePrefix = false)
    {
      var pK = catalog.Schemas[schema].Tables[table].CreatePrimaryKey(primaryKeyName,
        catalog.Schemas[schema].Tables[table].TableColumns[column]);
      var alter = SqlDdl.Alter(catalog.Schemas[schema].Tables[table], SqlDdl.AddConstraint(pK));
      var sqlComplieConfig = new SqlCompilerConfiguration();
      sqlComplieConfig.DatabaseQualifiedObjects = useDatabasePrefix;
      string commandText = sqlDriver.Compile(alter, sqlComplieConfig).GetCommandText();
      ExecuteNonQuery(commandText);
    }

    private void CreateColumn(Catalog catalog, string schema, string table, string columnName, SqlType columnType, bool useDatabasePrefix = false)
    {
      var column = catalog.Schemas[schema].Tables[table].CreateColumn(columnName, new SqlValueType(columnType));
      column.IsNullable = true;
      SqlAlterTable alter = SqlDdl.Alter(catalog.Schemas[schema].Tables[table], SqlDdl.AddColumn(column));
      var sqlComplieConfig = new SqlCompilerConfiguration();
      sqlComplieConfig.DatabaseQualifiedObjects = useDatabasePrefix;
      string commandText = sqlDriver.Compile(alter, sqlComplieConfig).GetCommandText();
      ExecuteNonQuery(commandText);
    }

    private void CreateTable(Catalog catalog, string schema, string tableName, string[] columns, SqlType[] columnTypes, bool useDatabasePrefix = false)
    {
      var table = catalog.Schemas[schema].CreateTable(tableName);
      for (var i = 0; i < columns.Count(); i++) {
        new TableColumn(table, columns[i], new SqlValueType(columnTypes[i]));
      }
      SqlCreateTable create = SqlDdl.Create(table);
      var sqlComplieConfig = new SqlCompilerConfiguration();
      sqlComplieConfig.DatabaseQualifiedObjects = useDatabasePrefix;
      string commandText = sqlDriver.Compile(create, sqlComplieConfig).GetCommandText();
      ExecuteNonQuery(commandText);
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

    private void SetNoMulti()
    {
      isMultidatabaseTest = false;
      isMultischemaTest = false;
    }
  }
}
