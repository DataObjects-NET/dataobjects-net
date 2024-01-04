// Copyright (C) 2011-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Csaba Beer
// Created:    2011.01.13

using NUnit.Framework;
using System;
using System.Text;
using Xtensive.Sql;

namespace Xtensive.Orm.Tests.Sql.Firebird
{
  public class ExtractorTest : ExtractorTestBase
  {
    protected override bool CheckContstraintExtracted => false;

    protected override void CheckRequirements() => Require.ProviderIs(StorageProvider.Firebird);

    protected override string GetTypesExtractionPrepareScript(string tableName)
    {
      var dataTypes = Driver.ServerInfo.DataTypes;
      var sb = new StringBuilder();
      _ = sb.Append($"CREATE TABLE {tableName} (");
      sb.AppendLine($"{TypeToColumnName[SqlType.Int16]}   smallint,");
      sb.AppendLine($"{TypeToColumnName[SqlType.Int32]}   integer,");
      sb.AppendLine($"{TypeToColumnName[SqlType.Int64]}   bigint,");
      sb.AppendLine($"{TypeToColumnName[SqlType.Decimal]} decimal({DecimalPrecision}, {DecimalScale}),");

      sb.AppendLine($"{TypeToColumnName[SqlType.Float]}  float,");
      sb.AppendLine($"{TypeToColumnName[SqlType.Double]} double precision,");

      sb.AppendLine($"{TypeToColumnName[SqlType.DateTime]} timestamp,");
      sb.AppendLine($"{TypeToColumnName[SqlType.Date]} date,");
      sb.AppendLine($"{TypeToColumnName[SqlType.Time]} time,");

      sb.AppendLine($"{TypeToColumnName[SqlType.Char]} char({CharLength}),");
      sb.AppendLine($"{TypeToColumnName[SqlType.VarChar]} varchar({VarCharLength}),");
      sb.AppendLine($"{TypeToColumnName[SqlType.VarCharMax]} blob sub_type 1,");

      sb.AppendLine($"{TypeToColumnName[SqlType.VarBinaryMax]} blob sub_type 0");
      sb.AppendLine(")");

      return sb.ToString();
    }
    protected override string GetTypesExtractionCleanUpScript(string tableName) => $"drop table {tableName};";

    protected override string GetForeignKeyExtractionPrepareScript()
    {
      return "CREATE TABLE B1 (b_id integer primary key);" +
        "CREATE TABLE A1 (b_id integer references B1(b_id));" +
        "CREATE TABLE B2 (b_id_1 integer, b_id_2 integer, " +
        "  CONSTRAINT B2_PK PRIMARY KEY (b_id_1, b_id_2));" +
        "CREATE TABLE A2 (b_id_1 integer, b_id_2 integer," +
        "  CONSTRAINT A2_FK FOREIGN KEY (b_id_1, b_id_2)" +
        "  REFERENCES B2 (b_id_1, b_id_2) ON DELETE CASCADE ON UPDATE NO ACTION);" +
        "CREATE TABLE B3 (b_id_1 integer, b_id_2 integer, b_id_3 integer," +
        "  CONSTRAINT B3_PK PRIMARY KEY(b_id_1, b_id_2, b_id_3));" +
        "CREATE TABLE A3 (A_col1 integer, b_id_3 integer, b_id_1 integer, b_id_2 integer," +
        "  CONSTRAINT A3_FK FOREIGN KEY (b_id_1, b_id_2, b_id_3)" +
        "  REFERENCES B3 (b_id_1, b_id_2, b_id_3) ON DELETE NO ACTION ON UPDATE CASCADE);";
    }

    protected override string GetForeignKeyExtractionCleanUpScript() =>
      "drop table A1;" +
      "\n drop table A2;" +
      "\n drop table A3;" +
      "\n drop table B1;" +
      "\n drop table B2;" +
      "\n drop table B3;";


    protected override string GetIndexExtractionPrepareScript(string tableName)
    {
      return
         $"CREATE TABLE {tableName} (column1 integer,  column2 integer);" +
         $"\n CREATE ASC INDEX {tableName}_index1_desc_asc on {tableName} (column1, column2);" +
         $"\n CREATE UNIQUE ASC INDEX {tableName}_index1_u_asc_desc on {tableName} (column1, column2);";
    }

    protected override string GetIndexExtractionCleanUpScript(string tableName) => $"drop table {tableName};";


    protected override string GetUniqueConstraintExtractionPrepareScript(string tableName)
    {
      return $"CREATE TABLE {tableName} (" +
         "\n  col_11 integer, col_12 integer, col_13 integer," +
         "\n  col_21 integer, col_22 integer, col_23 integer," +
         "\n  CONSTRAINT A_UNIQUE_1 UNIQUE(col_11,col_12,col_13), " +
         "\n  CONSTRAINT A_UNIQUE_2 UNIQUE(col_21,col_22,col_23));";
    }

    protected override string GetUniqueConstraintExtractionCleanUpScript(string tableName) => $"drop table {tableName};";
  }
}
