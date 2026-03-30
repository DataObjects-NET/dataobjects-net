// Copyright (C) 2009-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.07.29

using System;
using System.Text;
using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Sql.Oracle
{
  public class ExtractorTest : ExtractorTestBase
  {
    protected override void CheckRequirements() => Require.ProviderIs(StorageProvider.Oracle);

    #region Base test class members
    protected override string GetTypesExtractionPrepareScript(string tableName)
    {
      var dataTypes = Driver.ServerInfo.DataTypes;
      var sb = new StringBuilder();
      _ = sb.Append($"CREATE TABLE \"{tableName}\" (");
      sb.AppendLine($"\"{TypeToColumnName[SqlType.Decimal]}\" number({DecimalPrecision}, {DecimalScale}) NULL,");

      if (Driver.CoreServerInfo.ServerVersion.Major >= 10) {
        sb.AppendLine($"\"{TypeToColumnName[SqlType.Float]}\" binary_float NULL,");
        sb.AppendLine($"\"{TypeToColumnName[SqlType.Double]}\" binary_double NULL,");
      }
      else {
        sb.AppendLine($"\"{TypeToColumnName[SqlType.Float]}\" real NULL,");
        sb.AppendLine($"\"{TypeToColumnName[SqlType.Double]}\" float NULL,");
      }
      sb.AppendLine($"\"{TypeToColumnName[SqlType.Interval]}\" interval day to second NULL,");
      sb.AppendLine($"\"{TypeToColumnName[SqlType.DateTime]}\" timestamp NULL,");
      sb.AppendLine($"\"{TypeToColumnName[SqlType.DateTimeOffset]}\" TIMESTAMP WITH TIME ZONE NULL,");
      sb.AppendLine($"\"{TypeToColumnName[SqlType.Date]}\" date NULL,");
      sb.AppendLine($"\"{TypeToColumnName[SqlType.Time]}\" interval day(0) to second NULL,");

      sb.AppendLine($"\"{TypeToColumnName[SqlType.Char]}\" nchar ({CharLength}) NULL,");
      sb.AppendLine($"\"{TypeToColumnName[SqlType.VarChar]}\" nvarchar2({VarCharLength}) NULL,");
      sb.AppendLine($"\"{TypeToColumnName[SqlType.VarCharMax]}\" nclob NULL,");

      sb.AppendLine($"\"{TypeToColumnName[SqlType.VarBinaryMax]}\" blob NULL");
      sb.AppendLine(")");

      return sb.ToString();
    }
    protected override string GetTypesExtractionCleanUpScript(string tableName) => $"drop table \"{tableName}\";";

    protected override string GetForeignKeyExtractionPrepareScript()
    {
      return "CREATE TABLE \"B1\" (\"b_id\" int primary key);" +
        "CREATE TABLE \"B2\" (\"b_id_1\" int, \"b_id_2\" int, CONSTRAINT \"B2_PK\" PRIMARY KEY (\"b_id_1\", \"b_id_2\"));" +
        "CREATE TABLE \"B3\" (\"b_id_1\" int, \"b_id_2\" int, \"b_id_3\" int," +
        "  CONSTRAINT \"B3_PK\" PRIMARY KEY (\"b_id_1\", \"b_id_2\", \"b_id_3\"));" +
        "CREATE TABLE \"A1\" (\"b_id\" int, CONSTRAINT \"A1_FK\" FOREIGN KEY (\"b_id\") REFERENCES \"B1\" (\"b_id\"));" +
        "CREATE TABLE \"A2\" (\"b_id_1\" int, \"b_id_2\" int," +
        "  CONSTRAINT \"A2_FK\" FOREIGN KEY (\"b_id_1\", \"b_id_2\")" +
        "  REFERENCES \"B2\" (\"b_id_1\", \"b_id_2\") ON DELETE SET NULL);" +
        "CREATE TABLE \"A3\" (\"A_col1\" int, \"b_id_3\" int, \"b_id_1\" int, \"b_id_2\" int," +
        "  CONSTRAINT \"A3_FK\" FOREIGN KEY (\"b_id_1\", \"b_id_2\", \"b_id_3\")" +
        "  REFERENCES \"B3\" (\"b_id_1\", \"b_id_2\", \"b_id_3\") ON DELETE CASCADE);";
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
        $"CREATE TABLE \"{tableName}\" (\"column1\" int,  \"column2\" int);" +
        $"\n CREATE INDEX \"{tableName}_index1_desc_asc\" on \"{tableName}\" (\"column1\" desc, \"column2\" asc);" +
        $"\n CREATE UNIQUE INDEX \"{tableName}_index1_u_asc_desc\" on \"{tableName}\" (\"column1\" asc, \"column2\" desc);";
    }
    protected override string GetIndexExtractionCleanUpScript(string tableName) => $"drop table \"{tableName}\";";

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
      return $"CREATE TABLE \"{tableName}\" (" +
         "\n  \"col_11\" int, \"col_12\" int, \"col_13\" int," +
         "\n  \"col_21\" int, \"col_22\" int, \"col_23\" int," +
         "\n  CONSTRAINT \"A_UNIQUE_1\" UNIQUE(\"col_11\", \"col_12\", \"col_13\"), " +
         "\n  CONSTRAINT \"A_UNIQUE_2\" UNIQUE(\"col_21\", \"col_22\", \"col_23\"));";
    }
    protected override string GetUniqueConstraintExtractionCleanUpScript(string tableName) => $"drop table \"{tableName}\"";

    protected override string GetCheckConstraintExtractionPrepareScript(string tableName)
    {
      return $"CREATE TABLE \"{tableName}\" (" +
         "\n  \"col_11\" int, \"col_12\" int, \"col_13\" int," +
         "\n  \"col_21\" int, \"col_22\" int, \"col_23\" int," +
         "\n  CONSTRAINT \"A_CHECK_1\" CHECK(\"col_11\" > 0 OR \"col_12\" > 10 OR \"col_13\" > 20), " +
         "\n  CONSTRAINT \"A_CHECK_2\" CHECK(\"col_21\" <0 AND \"col_22\" < 10 AND \"col_23\" < 20));";
    }
    protected override string GetCheckConstraintExtractionCleanUpScript(string tableName) => $"drop table \"{tableName}\"";

    protected override string GetCheckSequenceExtractionPrepareScript()
    {
      return "CREATE SEQUENCE \"seq1\" START WITH 11 INCREMENT BY 100 MINVALUE 10 MAXVALUE 10000 NOCYCLE;" +
        "CREATE SEQUENCE \"seq2\" START WITH 110 INCREMENT BY 10 MINVALUE 10 MAXVALUE 100000 CYCLE;";
    }

    protected override string GetCheckSequenceExtractionCleanupScript() => "DROP SEQUENCE \"seq1\"; DROP SEQUENCE \"seq2\"";
    #endregion
  }
}