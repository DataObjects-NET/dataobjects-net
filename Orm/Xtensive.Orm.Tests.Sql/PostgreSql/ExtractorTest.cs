// Copyright (C) 2010-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2010.01.23

using System.Text;
using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Drivers.PostgreSql.v8_0;

namespace Xtensive.Orm.Tests.Sql.PostgreSql
{
  public class ExtractorTest: ExtractorTestBase
  {
    protected override bool CheckContstraintExtracted => true;

    protected override void CheckRequirements() => Require.ProviderIs(StorageProvider.PostgreSql);

    protected override void PopulateCustomTypeToColumnName()
    {
      TypeToColumnName[CustomSqlType.Point] = "pgpoint_column";
      TypeToColumnName[CustomSqlType.LSeg] = "pglsegar_column";
      TypeToColumnName[CustomSqlType.Box] = "pgbox_column";
      TypeToColumnName[CustomSqlType.Path] = "pgpath_column";
      TypeToColumnName[CustomSqlType.Polygon] = "pgpolygon_column";
      TypeToColumnName[CustomSqlType.Circle] = "pgcirgcle_column";
    }

    #region Base test class members
    protected override string GetTypesExtractionPrepareScript(string tableName)
    {
      var dataTypes = Driver.ServerInfo.DataTypes;
      var sb = new StringBuilder();
      _ = sb.Append($"CREATE TABLE \"{tableName}\" (");
      sb.AppendLine($"\"{TypeToColumnName[SqlType.Boolean]}\" boolean NULL,");
      sb.AppendLine($"\"{TypeToColumnName[SqlType.Int16]}\"   smallint NULL,");
      sb.AppendLine($"\"{TypeToColumnName[SqlType.Int32]}\"   integer NULL,");
      sb.AppendLine($"\"{TypeToColumnName[SqlType.Int64]}\"   bigint NULL,");
      sb.AppendLine($"\"{TypeToColumnName[SqlType.Decimal]}\" decimal({DecimalPrecision}, {DecimalScale}) NULL,");

      sb.AppendLine($"\"{TypeToColumnName[SqlType.Float]}\"  real NULL,");
      sb.AppendLine($"\"{TypeToColumnName[SqlType.Double]}\" double precision NULL,");

      sb.AppendLine($"\"{TypeToColumnName[SqlType.Interval]}\" interval NULL,");
      sb.AppendLine($"\"{TypeToColumnName[SqlType.DateTime]}\" timestamp NULL,");
      sb.AppendLine($"\"{TypeToColumnName[SqlType.DateTimeOffset]}\" timestamptz NULL,");
#if NET6_0_OR_GREATER
      sb.AppendLine($"\"{TypeToColumnName[SqlType.Date]}\" date NULL,");
      sb.AppendLine($"\"{TypeToColumnName[SqlType.Time]}\" time NULL,");
#endif

      sb.AppendLine($"\"{TypeToColumnName[SqlType.Char]}\" char ({CharLength}) NULL,");
      sb.AppendLine($"\"{TypeToColumnName[SqlType.VarChar]}\" varchar({VarCharLength}) NULL,");
      sb.AppendLine($"\"{TypeToColumnName[SqlType.VarCharMax]}\" text NULL,");

      sb.AppendLine($"\"{TypeToColumnName[SqlType.VarBinaryMax]}\" bytea NULL,");

      sb.AppendLine($"\"{TypeToColumnName[CustomSqlType.Point]}\" point NULL,");
      sb.AppendLine($"\"{TypeToColumnName[CustomSqlType.LSeg]}\" lseg NULL,");
      sb.AppendLine($"\"{TypeToColumnName[CustomSqlType.Box]}\" box NULL,");
      sb.AppendLine($"\"{TypeToColumnName[CustomSqlType.Path]}\" path NULL,");
      sb.AppendLine($"\"{TypeToColumnName[CustomSqlType.Polygon]}\" polygon NULL,");
      sb.AppendLine($"\"{TypeToColumnName[CustomSqlType.Circle]}\" circle NULL");
      sb.AppendLine(");");

      return sb.ToString();
    }
    protected override string GetTypesExtractionCleanUpScript(string tableName) => $"drop table \"{tableName}\";";

    protected override string GetForeignKeyExtractionPrepareScript()
    {
      return "CREATE TABLE \"B1\" (\"b_id\" int primary key);" +
        "CREATE TABLE \"A1\" (\"b_id\" int, CONSTRAINT \"A1_FK\" FOREIGN KEY (\"b_id\") REFERENCES \"B1\" (\"b_id\"));" +
        "CREATE TABLE \"B2\" (\"b_id_1\" int, \"b_id_2\" int, " +
        "  CONSTRAINT \"B2_PK\" PRIMARY KEY (\"b_id_1\", \"b_id_2\"));" +
        "CREATE TABLE \"A2\" (\"b_id_1\" int, \"b_id_2\" int," +
        "  CONSTRAINT \"A2_FK\" FOREIGN KEY (\"b_id_1\", \"b_id_2\")" +
        "  REFERENCES \"B2\" (\"b_id_1\", \"b_id_2\") ON DELETE CASCADE ON UPDATE NO ACTION);" +
        "CREATE TABLE \"B3\" (\"b_id_1\" int, \"b_id_2\" int, \"b_id_3\" int," +
        "  CONSTRAINT \"B3_PK\" PRIMARY KEY (\"b_id_1\", \"b_id_2\", \"b_id_3\"));" +
        "CREATE TABLE \"A3\" (\"A_col1\" int, \"b_id_3\" int, \"b_id_1\" int, \"b_id_2\" int," +
        "  CONSTRAINT \"A3_FK\" FOREIGN KEY (\"b_id_1\", \"b_id_2\", \"b_id_3\")" +
        "  REFERENCES \"B3\" (\"b_id_1\", \"b_id_2\", \"b_id_3\") ON DELETE NO ACTION ON UPDATE CASCADE);";
    }
    protected override string GetForeignKeyExtractionCleanUpScript() =>
      "drop table \"A1\";" +
      "\n drop table \"A2\";" +
      "\n drop table \"A3\";" +
      "\n drop table \"B1\";" +
      "\n drop table \"B2\";" +
      "\n drop table \"B3\";";

    protected override string GetIndexExtractionPrepareScript(string tableName)
    {
      return
        $"CREATE TABLE \"{tableName}\" (\"column1\" int, \"column2\" int);" +
        $"\n CREATE INDEX \"{tableName}_index1_desc_asc\" on \"{tableName}\" (\"column1\" desc, \"column2\" asc);" +
        $"\n CREATE UNIQUE INDEX \"{tableName}_index1_u_asc_desc\" on \"{tableName}\" (\"column1\" asc, \"column2\" desc);";
    }
    protected override string GetIndexExtractionCleanUpScript(string tableName) => $"drop table \"{tableName}\";";

    protected override string GetPartialIndexExtractionPrepareScript(string tableName)
    {
      return
        $"CREATE TABLE \"{tableName}\" (\"column1\" int,  \"column2\" int);" +
        $"\n CREATE INDEX \"{tableName}_index1_filtered\" on \"{tableName}\" (\"column1\", \"column2\") WHERE \"column1\" > 10;";
    }
    protected override string GetPartialIndexExtractionCleanUpScript(string tableName) => $"drop table \"{tableName}\";";

    protected override string GetFulltextIndexExtractionPrepareScript(string tableName)
    {
      return $"CREATE TABLE \"{tableName}\" (\"Id\" integer NOT NULL, \"Name\" varchar(100), \"Comments\" varchar(1000)," +
        $"  CONSTRAINT \"PK_{tableName}\" PRIMARY KEY (\"Id\"));" +
        $"CREATE INDEX \"FT_{tableName}\" ON \"{tableName}\" USING gin ((to_tsvector('English'::regconfig, (\"Name\")::text) || to_tsvector('English'::regconfig, (\"Comments\")::text)))";
    }
    protected override string GetFulltextIndexExtractionCleanUpScript(string tableName) => $"drop table \"{tableName}\";";

    protected override string GetUniqueConstraintExtractionPrepareScript(string tableName)
    {
      return $"CREATE TABLE \"{tableName}\" (" +
         "\n  \"col_11\" int, \"col_12\" int, \"col_13\" int," +
         "\n  \"col_21\" int, \"col_22\" int, \"col_23\" int," +
         "\n  CONSTRAINT A_UNIQUE_1 UNIQUE(\"col_11\", \"col_12\", \"col_13\"), " +
         "\n  CONSTRAINT A_UNIQUE_2 UNIQUE(\"col_21\", \"col_22\", \"col_23\"));";
    }
    protected override string GetUniqueConstraintExtractionCleanUpScript(string tableName) => $"drop table \"{tableName}\";";

    protected override string GetCheckConstraintExtractionPrepareScript(string tableName)
    {
      return $"CREATE TABLE \"{tableName}\" (" +
         "\n  \"col_11\" int, \"col_12\" int, \"col_13\" int," +
         "\n  \"col_21\" int, \"col_22\" int, \"col_23\" int," +
         "\n  CONSTRAINT \"A_CHECK_1\" CHECK(\"col_11\" > 0 OR \"col_12\" > 10 OR \"col_13\" > 20), " +
         "\n  CONSTRAINT \"A_CHECK_2\" CHECK(\"col_21\" < 0 AND \"col_22\" < 10 AND \"col_23\" < 20));";
    }
    protected override string GetCheckConstraintExtractionCleanUpScript(string tableName) => $"drop table \"{tableName}\";";
    #endregion

    #region Provider-related test cases
    protected virtual string GetExtractDateTimeOffsetFieldsPrepareScript(string tableName)
    {
      return $"CREATE TABLE  \"{tableName}\" (" +
        "\"ID\" bigint PRIMARY KEY NOT NULL," +
        "\"DateTimeOffset0\" timestamp(0) with time zone DEFAULT '0001-01-01 00:00:00+00:00'::timestamp(0) with time zone NOT NULL," +
        "\"DateTimeOffset1\" timestamp(1) with time zone DEFAULT '0001-01-01 00:00:00.0+00:00'::timestamp(1) with time zone NOT NULL," +
        "\"DateTimeOffset2\" timestamp(2) with time zone DEFAULT '0001-01-01 00:00:00.00+00:00'::timestamp(2) with time zone NOT NULL," +
        "\"DateTimeOffset3\" timestamp(3) with time zone DEFAULT '0001-01-01 00:00:00.000+00:00'::timestamp(3) with time zone NOT NULL" +
        ");";
    }

    protected virtual string GetExtractDateTimeOffsetFieldsCleanupScript(string tableName)
    {
      return $"DROP TABLE IF EXISTS \"{tableName}\"";
    }

    [Test]
    public void ExtractDateTimeOffsetFields()
    {
      var createTableQuery = GetExtractDateTimeOffsetFieldsPrepareScript("InteractionLog");
      RegisterCleanupScript(GetExtractDateTimeOffsetFieldsCleanupScript, "InteractionLog");

      ExecuteQueryLineByLine(createTableQuery);

      var defaultSchema = ExtractDefaultSchema();

      var testTable = defaultSchema.Tables["InteractionLog"];
      var tableColumn = testTable.TableColumns["DateTimeOffset0"];
      Assert.That(tableColumn.DataType.Type, Is.EqualTo(SqlType.DateTimeOffset));
      Assert.That(tableColumn.DefaultValue, Is.InstanceOf<SqlNative>());
      var defaultExpression = (SqlNative) tableColumn.DefaultValue;
      Assert.That(defaultExpression.Value, Is.EqualTo("'0001-01-01 04:02:33+04:02:33'::timestamp(0) with time zone"));

      tableColumn = testTable.TableColumns["DateTimeOffset1"];
      Assert.That(tableColumn.DataType.Type, Is.EqualTo(SqlType.DateTimeOffset));
      Assert.That(tableColumn.DefaultValue, Is.InstanceOf<SqlNative>());
      defaultExpression = (SqlNative) tableColumn.DefaultValue;
      Assert.That(defaultExpression.Value, Is.EqualTo("'0001-01-01 04:02:33+04:02:33'::timestamp(1) with time zone"));

      tableColumn = testTable.TableColumns["DateTimeOffset2"];
      Assert.That(tableColumn.DataType.Type, Is.EqualTo(SqlType.DateTimeOffset));
      Assert.That(tableColumn.DefaultValue, Is.InstanceOf<SqlNative>());
      defaultExpression = (SqlNative) tableColumn.DefaultValue;
      Assert.That(defaultExpression.Value, Is.EqualTo("'0001-01-01 04:02:33+04:02:33'::timestamp(2) with time zone"));

      tableColumn = testTable.TableColumns["DateTimeOffset3"];
      Assert.That(tableColumn.DataType.Type, Is.EqualTo(SqlType.DateTimeOffset));
      Assert.That(tableColumn.DefaultValue, Is.InstanceOf<SqlNative>());
      defaultExpression = (SqlNative) tableColumn.DefaultValue;
      Assert.That(defaultExpression.Value, Is.EqualTo("'0001-01-01 04:02:33+04:02:33'::timestamp(3) with time zone"));
    }


    protected virtual string GetExpressionIndexExtractorPrepareScript(string tableName)
    {
      return $"CREATE TABLE \"{tableName}\"(col1 text, col2 text);" +
        $"CREATE INDEX \"{tableName}_indx\" ON \"" + tableName + "\"(col1,col2,(col1||col2));";
    }

    protected virtual string GetExpressionIndexExtractorCleanupScript(string tableName)
    {
      return $"DROP TABLE IF EXISTS \"{tableName}\"";
    }

    [Test]
    public void ExpressionIndexExtractorTest()
    {
      var createTableQuery = GetExpressionIndexExtractorPrepareScript("tableWithIndx");
      RegisterCleanupScript(GetExpressionIndexExtractorCleanupScript, "tableWithIndx");

      ExecuteQueryLineByLine(createTableQuery);

      var schema = ExtractDefaultSchema();

      var table = schema.Tables["tableWithIndx"];
      Assert.AreEqual(1, table.Indexes.Count);
      var index = table.Indexes["tableWithIndx_indx"];
      Assert.AreEqual(3, index.Columns.Count);
      Assert.AreSame(table.Columns[0], index.Columns[0].Column);
      Assert.AreSame(table.Columns[1], index.Columns[1].Column);
      Assert.IsNull(index.Columns[0].Expression);
      Assert.IsNull(index.Columns[1].Expression);
      Assert.IsNull(index.Columns[2].Column);
      Assert.IsNotNull(index.Columns[2].Expression);
    }
    #endregion
  }
}