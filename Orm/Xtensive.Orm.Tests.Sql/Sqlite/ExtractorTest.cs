// Copyright (C) 2011-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Malisa Ncube
// Created:    2011.03.16

using System;
using System.Text;
using NUnit.Framework;
using Xtensive.Sql;

namespace Xtensive.Orm.Tests.Sql.Sqlite
{
  public class ExtractorTest: ExtractorTestBase
  {
    protected override bool CheckContstraintExtracted => false;

    protected override void CheckRequirements() => Require.ProviderIs(StorageProvider.Sqlite);

    #region Base test class members
    protected override string GetTypesExtractionPrepareScript(string tableName)
    {
      var dataTypes = Driver.ServerInfo.DataTypes;
      var sb = new StringBuilder();
      _ = sb.Append($"CREATE TABLE {tableName} (");
      sb.AppendLine($"{TypeToColumnName[SqlType.Boolean]} [bit] NULL,");
      sb.AppendLine($"{TypeToColumnName[SqlType.Int16]}   [smallint] NULL,");
      sb.AppendLine($"{TypeToColumnName[SqlType.Int32]}   [integer] NULL,");
      sb.AppendLine($"{TypeToColumnName[SqlType.Int64]}   [bigint] NULL,");
      sb.AppendLine($"{TypeToColumnName[SqlType.UInt8]}   [tinyint] NULL,");

      sb.AppendLine($"{TypeToColumnName[SqlType.Decimal]} [decimal]({DecimalPrecision}, {DecimalScale}) NULL,");

      sb.AppendLine($"{TypeToColumnName[SqlType.Float]}  [real] NULL,");
      sb.AppendLine($"{TypeToColumnName[SqlType.Double]} [float] NULL,");

      
      sb.AppendLine($"{TypeToColumnName[SqlType.DateTime]} [datetime] NULL,");
      sb.AppendLine($"{TypeToColumnName[SqlType.DateTimeOffset]} [datetimeoffset] NULL,");
#if NET6_0_OR_GREATER
      sb.AppendLine($"{TypeToColumnName[SqlType.Date]} [date] NULL,");
      sb.AppendLine($"{TypeToColumnName[SqlType.Time]} [time] NULL,");
#endif

      sb.AppendLine($"{TypeToColumnName[SqlType.VarCharMax]} [nvarchar] NULL,");

      sb.AppendLine($"{TypeToColumnName[SqlType.VarBinaryMax]} blob NULL,");

      sb.AppendLine($"{TypeToColumnName[SqlType.Guid]} uniqueidentifier NULL");
      sb.AppendLine(");");

      return sb.ToString();
    }
    protected override string GetTypesExtractionCleanUpScript(string tableName) => $"drop table if exists {tableName};";

    protected override string GetForeignKeyExtractionPrepareScript()
    {
      return "CREATE TABLE B1 (b_id int primary key);" +
        "CREATE TABLE A1 (b_id int, CONSTRAINT [A1_FK] FOREIGN KEY (b_id) REFERENCES B1(b_id));" +
        "CREATE TABLE B2 (b_id_1 int, b_id_2 int, " +
        "  CONSTRAINT [B2_PK] PRIMARY KEY (b_id_1, b_id_2));" +
        "CREATE TABLE A2 (b_id_1 int, b_id_2 int," +
        "  CONSTRAINT [A2_FK] FOREIGN KEY (b_id_1, b_id_2)" +
        "  REFERENCES B2 (b_id_1, b_id_2) ON DELETE CASCADE ON UPDATE NO ACTION);" +
        "CREATE TABLE B3 (b_id_1 int, b_id_2 int, b_id_3 int," +
        "  CONSTRAINT [B3_PK] PRIMARY KEY (b_id_1, b_id_2, b_id_3));" +
        "CREATE TABLE A3 (A_col1 int, b_id_3 int, b_id_1 int, b_id_2 int," +
        "  CONSTRAINT [A3_FK] FOREIGN KEY (b_id_1, b_id_2, b_id_3)" +
        "  REFERENCES B3 (b_id_1, b_id_2, b_id_3) ON DELETE NO ACTION ON UPDATE CASCADE);";
    }
    protected override string GetForeignKeyExtractionCleanUpScript() =>
      "drop table if exists A1;" +
      "\n drop table if exists A2;" +
      "\n drop table if exists A3;" +
      "\n drop table if exists B1;" +
      "\n drop table if exists B2;" +
      "\n drop table if exists B3;";

    protected override string GetIndexExtractionPrepareScript(string tableName)
    {
      return
        $"CREATE TABLE {tableName} (column1 int,  column2 int);" +
        $"\n CREATE INDEX {tableName}_index1_desc_asc on {tableName} (column1 desc, column2 asc);" +
        $"\n CREATE UNIQUE INDEX {tableName}_index1_u_asc_desc on {tableName} (column1 asc, column2 desc);";
    }
    protected override string GetIndexExtractionCleanUpScript(string tableName) => $"drop table if exists {tableName};";
    #endregion
  }
}