// Copyright (C) 2009-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Text;
using NUnit.Framework;
using Xtensive.Sql;

namespace Xtensive.Orm.Tests.Sql.SqlServer
{
  public class MSSQLExtractorTest : ExtractorTestBase
  {
    protected override bool CheckContstraintExtracted => false;

    protected override void CheckRequirements() => Require.ProviderIs(StorageProvider.SqlServer);

    #region Base test class members
    protected override string GetTypesExtractionPrepareScript(string tableName)
    {
      var dataTypes = Driver.ServerInfo.DataTypes;
      var sb = new StringBuilder();
      _ = sb.Append($"CREATE TABLE {tableName} (");
      sb.AppendLine($"{TypeToColumnName[SqlType.Boolean]} [bit] NULL,");
      sb.AppendLine($"{TypeToColumnName[SqlType.Int16]}   [smallint] NULL,");
      sb.AppendLine($"{TypeToColumnName[SqlType.Int32]}   [int] NULL,");
      sb.AppendLine($"{TypeToColumnName[SqlType.Int64]}   [bigint] NULL,");
      sb.AppendLine($"{TypeToColumnName[SqlType.UInt8]}   [tinyint] NULL,");
      sb.AppendLine($"{TypeToColumnName[SqlType.Decimal]} [decimal]({DecimalPrecision}, {DecimalScale}) NULL,");

      sb.AppendLine($"{TypeToColumnName[SqlType.Float]}  [real] NULL,");
      sb.AppendLine($"{TypeToColumnName[SqlType.Double]} [float] NULL,");

      if (Driver.CoreServerInfo.ServerVersion.Major > 10)
        sb.AppendLine($"{TypeToColumnName[SqlType.DateTime]} [datetime2] NULL,");
      else
        sb.AppendLine($"{TypeToColumnName[SqlType.DateTime]} [datetime] NULL,");
      sb.AppendLine($"{TypeToColumnName[SqlType.DateTimeOffset]} [datetimeoffset] NULL,");
      sb.AppendLine($"{TypeToColumnName[SqlType.Date]} [date] NULL,");
      sb.AppendLine($"{TypeToColumnName[SqlType.Time]} [time] NULL,");

      sb.AppendLine($"{TypeToColumnName[SqlType.Char]} [nchar] ({CharLength}) COLLATE Cyrillic_General_CI_AS NULL,");
      sb.AppendLine($"{TypeToColumnName[SqlType.VarChar]} [nvarchar]({VarCharLength}) COLLATE Cyrillic_General_CI_AS NULL,");
      sb.AppendLine($"{TypeToColumnName[SqlType.VarCharMax]} [nvarchar](max) COLLATE Cyrillic_General_CI_AS NULL,");

      sb.AppendLine($"{TypeToColumnName[SqlType.Binary]} [binary]({BinaryLength}) NULL,");
      sb.AppendLine($"{TypeToColumnName[SqlType.VarBinary]} [varbinary]({VarBinaryLength}) NULL,");
      sb.AppendLine($"{TypeToColumnName[SqlType.VarBinaryMax]} [varbinary] (max) NULL,");

      sb.AppendLine($"{TypeToColumnName[SqlType.Guid]} [uniqueidentifier] NULL"); // = "guid_column";
      sb.AppendLine(");");

      return sb.ToString();     
    }
    protected override string GetTypesExtractionCleanUpScript(string tableName) => $"drop table {tableName};";

    protected override string GetForeignKeyExtractionPrepareScript()
    {
      return "CREATE TABLE B1 (b_id int primary key);" +
        "CREATE TABLE A1 (b_id int references B1(b_id));" +
        "CREATE TABLE B2 (b_id_1 int, b_id_2 int, " +
        "  CONSTRAINT [B2_PK] PRIMARY KEY  CLUSTERED (b_id_1, b_id_2)  ON [PRIMARY]);" +
        "CREATE TABLE A2 (b_id_1 int, b_id_2 int," +
        "  CONSTRAINT [A2_FK] FOREIGN KEY (b_id_1, b_id_2)" +
        "  REFERENCES B2 (b_id_1, b_id_2) ON DELETE CASCADE ON UPDATE NO ACTION);" +
        "CREATE TABLE B3 (b_id_1 int, b_id_2 int, b_id_3 int," +
        "  CONSTRAINT [B3_PK] PRIMARY KEY  CLUSTERED (b_id_1, b_id_2, b_id_3)  ON [PRIMARY]);" +
        "CREATE TABLE A3 (A_col1 int, b_id_3 int, b_id_1 int, b_id_2 int," +
        "  CONSTRAINT [A3_FK] FOREIGN KEY (b_id_1, b_id_2, b_id_3)" +
        "  REFERENCES B3 (b_id_1, b_id_2, b_id_3) ON DELETE NO ACTION ON UPDATE CASCADE);";
    }
    protected override string GetForeignKeyExtractionCleanUpScript() =>
      "drop table A1" +
      "\n drop table A2" +
      "\n drop table A3" +
      "\n drop table B1" +
      "\n drop table B2" +
      "\n drop table B3";

    protected override string GetIndexExtractionPrepareScript(string tableName)
    {
      return
        $"CREATE TABLE {tableName} (column1 int,  column2 int);" +
        $"\n CREATE INDEX {tableName}_index1_desc_asc on {tableName} (column1 desc, column2 asc);" +
        $"\n CREATE UNIQUE INDEX {tableName}_index1_u_asc_desc on {tableName} (column1 asc, column2 desc);" +
        $"\n CREATE UNIQUE INDEX {tableName}_index_with_included_columns on {tableName} (column1 asc) include (column2);";
    }
    protected override string GetIndexExtractionCleanUpScript(string tableName) => $"drop table {tableName};";

    protected override string GetPartialIndexExtractionPrepareScript(string tableName)
    {
      return
        $"CREATE TABLE {tableName} (column1 int,  column2 int);" +
        $"\n CREATE INDEX {tableName}_index1_filtered on {tableName} (column1, column2) WHERE column1 > 10;";
    }
    protected override string GetPartialIndexExtractionCleanUpScript(string tableName) => $"drop table {tableName};";

    protected override string GetFulltextIndexExtractionPrepareScript(string tableName)
    {
      return $"CREATE TABLE {tableName} (Id int NOT NULL," +
        "\n  Name nvarchar(100) NULL," +
        "\n  Comments nvarchar(1000) NULL," +
        $"\n  CONSTRAINT [PK_{tableName}] PRIMARY KEY CLUSTERED (Id)  ON [PRIMARY]);" +
        $"\n CREATE FULLTEXT INDEX ON {tableName}(Name LANGUAGE 1033, Comments LANGUAGE 1033)" +
        $"\n   KEY INDEX PK_{tableName} WITH CHANGE_TRACKING AUTO;";
    }
    protected override string GetFulltextIndexExtractionCleanUpScript(string tableName) => $"drop table {tableName};";

    protected override string GetUniqueConstraintExtractionPrepareScript(string tableName)
    {
      return $"CREATE TABLE {tableName} (" +
         "\n  col_11 int, col_12 int, col_13 int," +
         "\n  col_21 int, col_22 int, col_23 int," +
         "\n  CONSTRAINT A_UNIQUE_1 UNIQUE(col_11,col_12,col_13), " +
         "\n  CONSTRAINT A_UNIQUE_2 UNIQUE(col_21,col_22,col_23));";
    }
    protected override string GetUniqueConstraintExtractionCleanUpScript(string tableName) => $"drop table {tableName}";

    protected override string GetCheckConstraintExtractionPrepareScript(string tableName)
    {
      return $"CREATE TABLE {tableName} (" +
         "\n  col_11 int, col_12 int, col_13 int," +
         "\n  col_21 int, col_22 int, col_23 int," +
         "\n  CONSTRAINT A_CHECK_1 CHECK(col_11 > 0 OR col_12 > 10 OR col_13 > 20), " +
         "\n  CONSTRAINT A_CHECK_2 CHECK(col_21 <0 AND col_22 < 10 AND col_23 < 20));";
    }
    protected override string GetCheckConstraintExtractionCleanUpScript(string tableName) => $"drop table {tableName}";
    #endregion

    #region Additional MS Sql Server specific tests

    protected virtual string GetMSSqlTypesExtractionPrepareScript(string tableName)
    {
      return $"CREATE TABLE {tableName} (" +
        "[int_l4] [int] NULL ," +
        "[binary_l50] [binary] (50) NULL ," +
        "[bit_l1] [bit] NULL , " +
        "[char_10] [char] (10) COLLATE Cyrillic_General_CI_AS NULL ," +
        "[datetime_l8] [datetime] NULL ," +
        "[decimal_p18_s0] [decimal](18, 0) NULL ," +
        "[decimal_p12_s11_l9] [decimal](12, 11) NULL ," +
        "[float_p53] [float] NULL ," +
        "[image_16] [image] NULL ," +
        "[money_p19_s4_l8] [money] NULL ," +
        "[nchar_l100] [nchar] (100) COLLATE Cyrillic_General_CI_AS NULL ," +
        "[ntext] [ntext] COLLATE Cyrillic_General_CI_AS NULL ," +
        "[numeric_p5_s5] [numeric](5, 5) NULL ," +
        "[nvarchar_l50] [nvarchar] (50) COLLATE Cyrillic_General_CI_AS NULL ," +
        "[real_p24_s0_l4] [real] NULL ," +
        "[smalldatetime_l4] [smalldatetime] NULL ," +
        "[smallint_l2] [smallint] NULL ," +
        "[small_money_p10_s4_l4] [smallmoney] NULL ," +
        "[sql_variant_] [sql_variant] NULL ," +
        "[text_16] [text] COLLATE Cyrillic_General_CI_AS NULL ," +
        "[timestamp_l8] [timestamp] NULL ," +
        "[tinyint_1_p3_s0_l1] [tinyint] NULL ," +
        "[uniqueidentifier_l16] [uniqueidentifier] NULL ," +
        "[varbinary_l150] [varbinary] (150) NULL ," +
        "[varchar_l50] [varchar] (50) COLLATE Cyrillic_General_CI_AS NULL)";
    }
    protected virtual string GetMSSqlTypesExtractionCleanUpScrypt(string tableName) => $"drop table {tableName}";

    [Test]
    public void MSSqlTypesExtractionTest()
    {
      var createTableQuery = GetMSSqlTypesExtractionPrepareScript("mssqlDttTable");
      RegisterCleanupScript(GetMSSqlTypesExtractionCleanUpScrypt, "mssqlDttTable");

      ExecuteQuery(createTableQuery);

      var table = ExtractDefaultSchema().Tables["mssqlDttTable"];

      var comparer = StringComparer.InvariantCultureIgnoreCase;

      Assert.IsTrue(table.TableColumns["int_l4"].DataType.Type == SqlType.Int32);
      Assert.IsTrue(table.TableColumns["binary_l50"].DataType.Length == 50);
      Assert.IsTrue(table.TableColumns["binary_l50"].DataType.Type == SqlType.Binary);
      Assert.IsTrue(table.TableColumns["bit_l1"].DataType.Type == SqlType.Boolean);
      Assert.IsTrue(table.TableColumns["char_10"].DataType.Length == 5);
      Assert.IsTrue(table.TableColumns["char_10"].DataType.Type == SqlType.Char);
      Assert.IsTrue(table.TableColumns["datetime_l8"].DataType.Type == SqlType.DateTime);
      Assert.IsTrue(table.TableColumns["decimal_p18_s0"].DataType.Type == SqlType.Decimal);
      Assert.IsTrue(table.TableColumns["decimal_p18_s0"].DataType.Precision == 18);
      Assert.IsTrue(table.TableColumns["decimal_p18_s0"].DataType.Scale == 0);
      Assert.IsTrue(table.TableColumns["decimal_p12_s11_l9"].DataType.Type == SqlType.Decimal);
      Assert.IsTrue(table.TableColumns["decimal_p12_s11_l9"].DataType.Precision == 12);
      Assert.IsTrue(table.TableColumns["decimal_p12_s11_l9"].DataType.Scale == 11);
      Assert.IsTrue(table.TableColumns["float_p53"].DataType.Type == SqlType.Double);
      Assert.IsTrue(table.TableColumns["float_p53"].DataType.Precision == null);
      Assert.IsTrue(table.TableColumns["float_p53"].DataType.Scale == null);
      Assert.IsTrue(table.TableColumns["image_16"].DataType.Type == SqlType.VarBinaryMax);
      Assert.IsTrue(comparer.Compare(table.TableColumns["money_p19_s4_l8"].DataType.TypeName, "money") == 0);
      Assert.IsTrue(table.TableColumns["money_p19_s4_l8"].DataType.Precision == 19);
      Assert.IsTrue(table.TableColumns["money_p19_s4_l8"].DataType.Scale == 4);
      Assert.IsTrue(table.TableColumns["nchar_l100"].DataType.Type == SqlType.Char);
      Assert.IsTrue(table.TableColumns["nchar_l100"].DataType.Length == 100);
      Assert.IsTrue(table.TableColumns["ntext"].DataType.Type == SqlType.VarCharMax);
      Assert.IsTrue(table.TableColumns["numeric_p5_s5"].DataType.Type == SqlType.Decimal);
      Assert.IsTrue(table.TableColumns["numeric_p5_s5"].DataType.Precision == 5);
      Assert.IsTrue(table.TableColumns["nvarchar_l50"].DataType.Type == SqlType.VarChar);
      Assert.IsTrue(table.TableColumns["nvarchar_l50"].DataType.Length == 50);
      Assert.IsTrue(table.TableColumns["real_p24_s0_l4"].DataType.Type == SqlType.Float);
      Assert.IsTrue(table.TableColumns["real_p24_s0_l4"].DataType.Precision == null);
      Assert.IsTrue(table.TableColumns["real_p24_s0_l4"].DataType.Scale == null);
      Assert.IsTrue(table.TableColumns["smalldatetime_l4"].DataType.Type == SqlType.DateTime);
      Assert.IsTrue(table.TableColumns["smallint_l2"].DataType.Type == SqlType.Int16);
      Assert.IsTrue(comparer.Compare(table.TableColumns["small_money_p10_s4_l4"].DataType.TypeName, "smallmoney") == 0);
      Assert.IsTrue(comparer.Compare(table.TableColumns["sql_variant_"].DataType.TypeName, "sql_variant") == 0);
      Assert.IsTrue(table.TableColumns["text_16"].DataType.Type == SqlType.VarCharMax);
      Assert.IsTrue(comparer.Compare(table.TableColumns["timestamp_l8"].DataType.TypeName, "timestamp") == 0);
      Assert.IsTrue(table.TableColumns["tinyint_1_p3_s0_l1"].DataType.Type == SqlType.UInt8);
      Assert.IsTrue(table.TableColumns["uniqueidentifier_l16"].DataType.Type == SqlType.Guid);
      Assert.IsTrue(table.TableColumns["varbinary_l150"].DataType.Type == SqlType.VarBinary);
      Assert.IsTrue(table.TableColumns["varbinary_l150"].DataType.Length == 150);
      Assert.IsTrue(table.TableColumns["varchar_l50"].DataType.Type == SqlType.VarChar);
      Assert.IsTrue(table.TableColumns["varchar_l50"].DataType.Length == 25);
    }

    protected virtual string GetMSSqlViewsExtractionTestPrepareStript()
    {
      return
        " EXEC sp_addrole 'role1';" +
        "\n CREATE TABLE role1.table1(column1 int, column2 int);" +
        "\n CREATE VIEW role1.view1 as Select column1 From role1.table1;" +
        "\n CREATE VIEW role1.view2 as Select column1, column2 From role1.table1;";
    }

    protected virtual string GetMSSqlViewsExtractionTestCleanUpScript()
    {
      return "\n drop table role1.table1;" +
             "\n drop view role1.view1;" +
             "\n drop view role1.view2;" +
             "\n exec sp_droprole role1";
    }

    [Test]
    public virtual void MSSqlViewsExtractionTest()
    {
      var createViewsQuery = GetMSSqlViewsExtractionTestPrepareStript();
      RegisterCleanupScript(GetMSSqlViewsExtractionTestCleanUpScript);

      ExecuteQueryLineByLine(createViewsQuery);

      var schema = ExtractCatalog().Schemas["role1"];

      Assert.IsNotNull(schema);
      Assert.IsNotNull(schema.Views["view1"]);
      Assert.IsNotNull(schema.Views["view2"]);
      Assert.IsNotNull(schema.Views["view1"].ViewColumns["column1"]);
      Assert.IsNotNull(schema.Views["view2"].ViewColumns["column1"]);
      Assert.IsNotNull(schema.Views["view2"].ViewColumns["column2"]);
    }
    #endregion
  }
}