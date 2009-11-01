using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using NUnit.Framework;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Database.Providers;
using SQL2000 = Xtensive.Sql.Dom.Mssql.v2000;
using SQL2005 = Xtensive.Sql.Dom.Mssql.v2005;

namespace Xtensive.Sql.Dom.Tests.VistaDb
{
  public class AssertUtilityVDB
  {
    public static void AssertArraysAreEqual(Array a1, Array a2)
    {
      if (a1 == a2)
        return;
      if (a1 == null || a2 == null)
        throw new AssertionException("One of arrays is null.");
      if (a1.Length != a2.Length)
        throw new AssertionException("Lengths are different.");
      for (int i = 0; i < a1.Length; i++)
        Assert.AreEqual(a1.GetValue(i), a2.GetValue(i));
    }

    public static void AssertCollectionsAreEqual(IEnumerable col1, IEnumerable col2)
    {
      if (col1 == col2)
        return;
      if (col2 == null || col1 == null)
        throw new AssertionException("One of arrays is null.");
      IEnumerator enumerator1 = col1.GetEnumerator();
      IEnumerator enumerator2 = col2.GetEnumerator();
      enumerator1.Reset();
      enumerator2.Reset();
      while (enumerator1.MoveNext())
      {
        if (!enumerator2.MoveNext())
          throw new AssertionException("Different count.");
        Assert.AreEqual(enumerator1.Current, enumerator2.Current);
      }
      if (enumerator2.MoveNext())
        throw new AssertionException("Different count.");
    }
  }

  public abstract class VDBExtractorTestBase
  {
    private SqlConnection sqlConnection;
    private IDbCommand dbCommand;
    private SqlCommand sqlCommand;
    private SqlDriver SqlDriver;


    public virtual string CleanUpScript
    {
      get {return null;}
    }
    
    protected SqlConnection CreateConnection()
    {
      SqlConnectionProvider provider = new SqlConnectionProvider();
      sqlConnection = (SqlConnection)provider.CreateConnection(@"vistadb://localhost/VDBTests.vdb3");
      SqlDriver = sqlConnection.Driver as SqlDriver;
      dbCommand = sqlConnection.RealConnection.CreateCommand();
      sqlCommand = new SqlCommand(sqlConnection);
      sqlConnection.Open();
      return sqlConnection;
    }

    protected void ExecuteQuery(string sqlQuery, SqlConnection connection)
    {
      DbCommand command = connection.RealConnection.CreateCommand();
      command.CommandText = sqlQuery;
      command.ExecuteNonQuery();
    }

    protected void ExecuteQuery(string sqlQuery)
    {
      if (string.IsNullOrEmpty(sqlQuery))
        return;
      using (SqlConnection connection = CreateConnection())
      {
        try { ExecuteQuery(sqlQuery, connection); }
        catch { }
      }
    }

    protected virtual Model ExtractModel()
    {
      SqlModelProvider smd = new SqlModelProvider(CreateConnection());
      return Model.Build(smd);
    }

    [TestFixtureSetUp]
    public virtual void Init()
    {
      try { ExecuteQuery(CleanUpScript);}
      catch {}
    }

    [TestFixtureTearDown]
    public virtual void Finalize()
    {
      ExecuteQuery(CleanUpScript);
    }
  }

  [TestFixture]
  public class VDBExtractor_TestSchemaExtraction : VDBExtractorTestBase
  {
    public override string CleanUpScript
    {
      get
      {
        return
          "\n drop table dbo.dataTypesTestTable;" +
            "\n drop table dbo.table1;" +
              "\n drop table dbo.table2;" +
                "\n drop table dbo.table3;" +
                  "\n drop table dbo.table31;";

      }
    }

    [Test]
    public virtual void Main()
    {
      ExecuteQuery( "create table dbo.table1(test int, test2 int, test3 int)");
      ExecuteQuery("\n create table dbo.table2(test int, test2 int, test3 int)" );
      ExecuteQuery("\n create table dbo.table3(test int, test2 int, test3 int)");
      ExecuteQuery("\n create table dbo.table31(test int, test2 int, test3 int)");
      SqlModelProvider smd = new SqlModelProvider(CreateConnection());
      Model model = Model.Build(smd);

      // Validating.
      Assert.IsNotNull(model.DefaultServer.DefaultCatalog.Schemas["dbo"]);
      Assert.IsNotNull(model.DefaultServer.DefaultCatalog.Schemas["dbo"].Tables["table1"]);
      Assert.IsNotNull(model.DefaultServer.DefaultCatalog.Schemas["dbo"].Tables["table1"].TableColumns["test"]);
      Assert.IsNotNull(model.DefaultServer.DefaultCatalog.Schemas["dbo"].Tables["table1"].TableColumns["test2"]);
      Assert.IsNotNull(model.DefaultServer.DefaultCatalog.Schemas["dbo"].Tables["table1"].TableColumns["test3"]);
      Assert.IsNotNull(model.DefaultServer.DefaultCatalog.Schemas["dbo"].Tables["table2"]);
      Assert.IsNotNull(model.DefaultServer.DefaultCatalog.Schemas["dbo"].Tables["table2"].TableColumns["test"]);
      Assert.IsNotNull(model.DefaultServer.DefaultCatalog.Schemas["dbo"].Tables["table2"].TableColumns["test2"]);
      Assert.IsNotNull(model.DefaultServer.DefaultCatalog.Schemas["dbo"].Tables["table2"].TableColumns["test3"]);
      Assert.IsNotNull(model.DefaultServer.DefaultCatalog.Schemas["dbo"].Tables["table3"]);
      Assert.IsNotNull(model.DefaultServer.DefaultCatalog.Schemas["dbo"].Tables["table3"].TableColumns["test"]);
      Assert.IsNotNull(model.DefaultServer.DefaultCatalog.Schemas["dbo"].Tables["table3"].TableColumns["test2"]);
      Assert.IsNotNull(model.DefaultServer.DefaultCatalog.Schemas["dbo"].Tables["table3"].TableColumns["test3"]);
//        Assert.AreEqual(model.DefaultServer.DefaultCatalog.Schemas["dbo"].Tables.Count, 11);
    }
  }

  [TestFixture]
  public class VDBExtractor_TestColumnTypeExtraction : VDBExtractorTestBase
  {
    public override string CleanUpScript
    {
      get { return "drop table dataTypesTestTable"; }
    }

    [Test]
    public void Main()
    {
      string createTableQuery =
        "CREATE TABLE dataTypesTestTable (" +
          "[int_l4] [int] NULL ," +
            "[varbinary_l50] [varbinary] NULL ," +
              "[bit_l1] [bit] NULL , " +
                "[char_10] [char] (10) ," +
                  "[datetime_l8] [datetime] NULL ," +
                    "[decimal_p18_s0] [decimal](18, 0) NULL ," +
                      "[decimal_p12_s11_l9] [decimal](12, 11) NULL ," +
                        "[float_p53] [float] NULL ," +
                          "[image_16] [image] NULL ," +
                            "[money_p19_s4_l8] [money] NULL ," +
                              "[nchar_l100] [nchar] (100)  ," +
                                "[ntext] [ntext] ," +
                                  "[numeric_p5_s5] [numeric](5, 5) NULL ," +
                                    "[nvarchar_l50] [nvarchar] (50)  ," +
                                      "[real_p24_s0_l4] [real] NULL ," +
                                        "[smalldatetime_l4] [smalldatetime] NULL ," +
                                          "[smallint_l2] [smallint] NULL ," +
                                            "[small_money_p10_s4_l4] [smallmoney] NULL ," +
                                              "[text_16] [text]  ," +
                                                "[timestamp_l8] [timestamp] NULL ," +
                                                  "[tinyint_1_p3_s0_l1] [tinyint] NULL ," +
                                                    "[uniqueidentifier_l16] [uniqueidentifier] NULL ," +
                                                      "[varbinary_l150] [varbinary] (150) NULL ," +
                                                        "[varchar_l50] [varchar] (50) )";
      ExecuteQuery(createTableQuery);
      Model model = ExtractModel();

      Table table = model.DefaultServer.DefaultCatalog.DefaultSchema.Tables["dataTypesTestTable"];
      Assert.IsTrue(table.TableColumns["int_l4"].DataType.DataType == SqlDataType.Int32);
      Assert.IsTrue(table.TableColumns["varbinary_l50"].DataType.DataType == SqlDataType.VarBinary);
      Assert.IsTrue(table.TableColumns["bit_l1"].DataType.DataType == SqlDataType.Boolean);
      Assert.IsTrue(table.TableColumns["char_10"].DataType.Size == 10);
      Assert.IsTrue(table.TableColumns["char_10"].DataType.DataType == SqlDataType.AnsiChar);
      Assert.IsTrue(table.TableColumns["datetime_l8"].DataType.DataType == SqlDataType.DateTime);
      Assert.IsTrue(table.TableColumns["decimal_p18_s0"].DataType.DataType == SqlDataType.Decimal);
      //Assert.IsTrue(table.TableColumns["decimal_p18_s0"].DataType.Precision == 18);
      //Assert.IsTrue(table.TableColumns["decimal_p18_s0"].DataType.Scale == 0);
      Assert.IsTrue(table.TableColumns["decimal_p12_s11_l9"].DataType.DataType == SqlDataType.Decimal);
      //Assert.IsTrue(table.TableColumns["decimal_p12_s11_l9"].DataType.Precision == 12);
      //Assert.IsTrue(table.TableColumns["decimal_p12_s11_l9"].DataType.Scale == 11);
      Assert.IsTrue(table.TableColumns["float_p53"].DataType.DataType == SqlDataType.Double);
      //Assert.IsTrue(table.TableColumns["float_p53"].DataType.Precision == 53);
      Assert.IsTrue(table.TableColumns["image_16"].DataType.DataType == SqlDataType.Image);
      Assert.IsTrue(table.TableColumns["money_p19_s4_l8"].DataType.DataType == SqlDataType.Money);
      //Assert.IsTrue(table.TableColumns["money_p19_s4_l8"].DataType.Precision == 19);
      //Assert.IsTrue(table.TableColumns["money_p19_s4_l8"].DataType.Scale == 4);
      Assert.IsTrue(table.TableColumns["nchar_l100"].DataType.DataType == SqlDataType.Char);
      Assert.IsTrue(table.TableColumns["nchar_l100"].DataType.Size == 100);
      Assert.IsTrue(table.TableColumns["ntext"].DataType.DataType == SqlDataType.Text);
      Assert.IsTrue(table.TableColumns["numeric_p5_s5"].DataType.DataType == SqlDataType.Decimal);
      //Assert.IsTrue(table.TableColumns["numeric_p5_s5"].DataType.Precision == 5);
      Assert.IsTrue(table.TableColumns["nvarchar_l50"].DataType.DataType == SqlDataType.VarChar);
      Assert.IsTrue(table.TableColumns["nvarchar_l50"].DataType.Size == 50);
      Assert.IsTrue(table.TableColumns["real_p24_s0_l4"].DataType.DataType == SqlDataType.Float);
      //Assert.IsTrue(table.TableColumns["real_p24_s0_l4"].DataType.Precision == 24);
      //Assert.IsTrue(table.TableColumns["real_p24_s0_l4"].DataType.Scale == 0);
      Assert.IsTrue(table.TableColumns["smalldatetime_l4"].DataType.DataType == SqlDataType.SmallDateTime);
      Assert.IsTrue(table.TableColumns["smallint_l2"].DataType.DataType == SqlDataType.Int16);
      Assert.IsTrue(table.TableColumns["small_money_p10_s4_l4"].DataType.DataType == SqlDataType.SmallMoney);
      Assert.IsTrue(table.TableColumns["text_16"].DataType.DataType == SqlDataType.AnsiText);
      Assert.IsTrue(table.TableColumns["timestamp_l8"].DataType.DataType == SqlDataType.TimeStamp);
      Assert.IsTrue(table.TableColumns["tinyint_1_p3_s0_l1"].DataType.DataType == SqlDataType.Byte);
      Assert.IsTrue(table.TableColumns["uniqueidentifier_l16"].DataType.DataType == SqlDataType.Guid);
      Assert.IsTrue(table.TableColumns["varbinary_l150"].DataType.DataType == SqlDataType.VarBinary);
      //Assert.IsTrue(table.TableColumns["varbinary_l150"].DataType.Size == 150);
      Assert.IsTrue(table.TableColumns["varchar_l50"].DataType.DataType == SqlDataType.AnsiVarChar);
      //Assert.IsTrue(table.TableColumns["varchar_l50"].DataType.Size == 50);
    }
  }

  [TestFixture]
  public class VDBExtractor_TestExtractingViews : VDBExtractorTestBase
  {
    public override string CleanUpScript
    {
      get
      {
        return "\n drop table dbo.table4;" +
          "\n drop view dbo.view1;" +
            "\n drop view dbo.view2;";
      }
    }

    [Test]
    public virtual void Main()
    {
      ExecuteQuery("CREATE TABLE table4(column1 int, column2 int)");

      ExecuteQuery(
        "CREATE VIEW view1 " +
          "\n as Select column1 From dbo.table4");

      ExecuteQuery(
        "CREATE VIEW view2 " +
          "\n as Select column1, column2 From dbo.table4");

      SqlConnection connection = CreateConnection();
      SqlModelProvider smd = new SqlModelProvider(connection);
      Model model = Model.Build(smd);
      Schema schema = model.DefaultServer.DefaultCatalog.Schemas["dbo"];

      Assert.IsNotNull(schema);
      Assert.IsNotNull(schema.Views["view1"]);
      Assert.IsNotNull(schema.Views["view2"]);
      Assert.IsNotNull(schema.Views["view1"].ViewColumns["column1"]);
      Assert.IsNotNull(schema.Views["view2"].ViewColumns["column1"]);
      Assert.IsNotNull(schema.Views["view2"].ViewColumns["column2"]);
    }
  }

  [TestFixture]
  public class VDBExtractor_TestExtractingForeignKeys : VDBExtractorTestBase
  {
    public override string CleanUpScript
    {
      get
      {
        return
          "\n drop table A1" +
            "\n drop table A2" +
              "\n drop table A3" +
                "\n drop table B1" +
                  "\n drop table B2" +
                    "\n drop table B3";
      }
    }

    [Test]
    public void Main()
    {
      ExecuteQuery("\n create table B1 (b_id int primary key)");
      ExecuteQuery("\n create table A1 (b_id int references B1(b_id))");
      ExecuteQuery("\n create table B2 (" +
        "\n   b_id_1 int, " +
          "\n   b_id_2 int, " +
            "\n   CONSTRAINT [B2_PK] PRIMARY KEY  CLUSTERED (b_id_1, b_id_2) )");
      ExecuteQuery("\n create table A2 (" +
        "\n   b_id_1 int,    " +
          "\n   b_id_2 int,    " +
            "\n   constraint [A2_FK] FOREIGN KEY (b_id_1, b_id_2) " +
              "\n                    REFERENCES B2 (b_id_1, b_id_2) ON DELETE CASCADE ON UPDATE NO ACTION)");
      ExecuteQuery("\n create table B3 (" +
        "\n   b_id_1 int," +
          "\n   b_id_2 int," +
            "\n   b_id_3 int," +
              "\n   CONSTRAINT [B3_PK] PRIMARY KEY  CLUSTERED (b_id_1, b_id_2, b_id_3) )"); 
      ExecuteQuery ("\n create table A3 (" +
        "\n   A_col1 int," +
          "\n   b_id_3 int," +
            "\n   b_id_1 int," +
              "\n   b_id_2 int," +
                "\n   constraint [A3_FK] FOREIGN KEY (b_id_1, b_id_2, b_id_3) " +
                  "\n                    REFERENCES B3 (b_id_1, b_id_2, b_id_3) ON DELETE NO ACTION ON UPDATE CASCADE)");


      Model model = ExtractModel();
      Schema schema = model.DefaultServer.DefaultCatalog.DefaultSchema;

      // Validating.
      ForeignKey fk1 = (ForeignKey)schema.Tables["A1"].TableConstraints[0];
      Assert.IsNotNull(fk1);
      Assert.IsTrue(fk1.Columns[0].Name == "b_id");
      Assert.IsTrue(fk1.ReferencedColumns[0].Name == "b_id");
      Assert.IsTrue(fk1.ReferencedColumns.Count == 1);
      Assert.IsTrue(fk1.Columns.Count == 1);

      ForeignKey fk2 = (ForeignKey)schema.Tables["A2"].TableConstraints[0];
      Assert.IsNotNull(fk1);
      Assert.IsTrue(fk2.Name == "A2_FK");
      Assert.IsTrue(fk2.Columns[0].Name == "b_id_1");
      Assert.IsTrue(fk2.ReferencedColumns[0].Name == "b_id_1");
      Assert.IsTrue(fk2.Columns[1].Name == "b_id_2");
      Assert.IsTrue(fk2.ReferencedColumns[1].Name == "b_id_2");
      Assert.IsTrue(fk2.ReferencedColumns.Count == 2);
      Assert.IsTrue(fk2.Columns.Count == 2);
      //Assert.IsTrue(fk2.OnDelete == ReferentialAction.Cascade);
      //Assert.IsTrue(fk2.OnUpdate == ReferentialAction.NoAction);

      ForeignKey fk3 = (ForeignKey)schema.Tables["A3"].TableConstraints[0];
      Assert.IsNotNull(fk3);
      Assert.IsTrue(fk3.Name == "A3_FK");
      Assert.IsTrue(fk3.Columns[0].Name == "b_id_1");
      Assert.IsTrue(fk3.ReferencedColumns[0].Name == "b_id_1");
      Assert.IsTrue(fk3.Columns[1].Name == "b_id_2");
      Assert.IsTrue(fk3.ReferencedColumns[1].Name == "b_id_2");
      Assert.IsTrue(fk3.Columns[2].Name == "b_id_3");
      Assert.IsTrue(fk3.ReferencedColumns[2].Name == "b_id_3");
      Assert.IsTrue(fk3.ReferencedColumns.Count == 3);
      Assert.IsTrue(fk3.Columns.Count == 3);
      //Assert.IsTrue(fk3.OnDelete == ReferentialAction.NoAction);
      //Assert.IsTrue(fk3.OnUpdate == ReferentialAction.Cascade);
    }
  }

  [TestFixture]
  public class VDBExtractor_TestExtractingUniqueConstraints : VDBExtractorTestBase
  {
    public override string CleanUpScript
    {
      get { return "drop table A"; }
    }

    [Test]
    public void Main()
    {
      ExecuteQuery(
        "   Create table A (" +
          "\n col_11 int, col_12 int, col_13 int," +
            "\n col_21 int, col_22 int, col_23 int," +
              "\n CONSTRAINT A_UNIQUE_1 UNIQUE(col_11,col_12,col_13)," +
                "\n CONSTRAINT A_UNIQUE_2 UNIQUE(col_21,col_22,col_23))");

      Model model = ExtractModel();
      Schema schema = model.DefaultServer.DefaultCatalog.DefaultSchema;

      // Validating.
      Database.UniqueConstraint A_UNIQUE_1 = (Database.UniqueConstraint)schema.Tables["A"].TableConstraints["A_UNIQUE_1"];
      Assert.IsNotNull(A_UNIQUE_1);
      Assert.IsTrue(A_UNIQUE_1.Columns[0].Name == "col_11");
      Assert.IsTrue(A_UNIQUE_1.Columns[1].Name == "col_12");
      Assert.IsTrue(A_UNIQUE_1.Columns[2].Name == "col_13");
      Assert.IsTrue(A_UNIQUE_1.Columns.Count == 3);

      Database.UniqueConstraint A_UNIQUE_2 = (Database.UniqueConstraint)schema.Tables["A"].TableConstraints["A_UNIQUE_2"];
      Assert.IsNotNull(A_UNIQUE_2);
      Assert.IsTrue(A_UNIQUE_2.Columns[0].Name == "col_21");
      Assert.IsTrue(A_UNIQUE_2.Columns[1].Name == "col_22");
      Assert.IsTrue(A_UNIQUE_2.Columns[2].Name == "col_23");
      Assert.IsTrue(A_UNIQUE_2.Columns.Count == 3);
    }
  }

  [TestFixture]
  public class VDBExtractor_TestIndexesExtracted : VDBExtractorTestBase
  {
    public override string CleanUpScript
    {
      get { return "drop table table1"; }
    }

    [Test]
    public virtual void Main()
    {
      ExecuteQuery(
        " create table table1 (" +
          "\n column1 int, " +
            "\n column2 int) ");
      ExecuteQuery(
        "\n create index        table1_index1_desc_asc   on table1 (column1 desc, column2 asc)");
      ExecuteQuery(
        "\n create unique index table1_index1_u_asc_desc on table1 (column1 asc, column2 desc)");
      Model model = ExtractModel();
      Schema schema = model.DefaultServer.DefaultCatalog.DefaultSchema;

      Assert.IsTrue(schema.Tables["table1"] != null);
      Assert.IsNotNull(schema.Tables["table1"].Indexes["table1_index1_desc_asc"]);
      Assert.IsTrue(schema.Tables["table1"].Indexes["table1_index1_desc_asc"].Columns.Count == 2);
      Assert.IsTrue(schema.Tables["table1"].Indexes["table1_index1_desc_asc"].Columns[0].Name == "column1");
      Assert.IsTrue(!schema.Tables["table1"].Indexes["table1_index1_desc_asc"].Columns[0].Ascending);
      Assert.IsTrue(schema.Tables["table1"].Indexes["table1_index1_desc_asc"].Columns[1].Ascending);

      Assert.IsNotNull(schema.Tables["table1"].Indexes["table1_index1_u_asc_desc"]);
      Assert.IsTrue(schema.Tables["table1"].Indexes["table1_index1_u_asc_desc"].Columns.Count == 2);
      Assert.IsTrue(schema.Tables["table1"].Indexes["table1_index1_u_asc_desc"].Columns[0].Ascending);
      Assert.IsTrue(!schema.Tables["table1"].Indexes["table1_index1_u_asc_desc"].Columns[1].Ascending);
    }
  }
}