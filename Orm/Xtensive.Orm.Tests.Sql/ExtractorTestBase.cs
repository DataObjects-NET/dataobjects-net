// Copyright (C) 2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic;
using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Sql.Info;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Sql
{
  [TestFixture]
  public abstract class ExtractorTestBase : SqlTest
  {
    protected const int CharLength = 4;
    protected const int VarCharLength = 6;

    protected const int BinaryLength = 3;
    protected const int VarBinaryLength = 6;

    protected const int DecimalScale = 4;
    protected const int DecimalPrecision = 12;

    private readonly List<string> schemasToCheck = new();
    private readonly List<string> cleanups = new();

    protected readonly Dictionary<SqlType, string> TypeToColumnName = new();

    protected virtual bool CheckContstraintExtracted => true;
    protected virtual bool SeqStartEqualsToMin => false;

    protected bool IgnoreTests { get; set; } = false;
    protected bool NonKeyColumnsSupported => Driver.ServerInfo.Index.Features.HasFlag(IndexFeatures.NonKeyColumns);
    protected bool PartialIndexesSupported => Driver.ServerInfo.Index.Features.HasFlag(IndexFeatures.Filtered);
    protected bool FulltextIndexesSupported => Driver.ServerInfo.Index.Features.HasFlag(IndexFeatures.FullText);
    protected bool SortOrderSupported => Driver.ServerInfo.Index.Features.HasFlag(IndexFeatures.SortOrder);


    protected override void TestFixtureSetUp()
    {
      if (IgnoreTests) {
        throw new IgnoreException(string.Empty);
      }
      base.TestFixtureSetUp();

      PopulateTypeToColumnName();
      PopulateSchemasToCheck();
    }

    protected override void TestFixtureTearDown()
    {
      if (!IgnoreTests) {
        foreach (var query in cleanups) {
          ExecuteQueryLineByLine(query, true);
        }
      }

      base.TestFixtureTearDown();
    }

    [Test]
    public void SchemaExtractionTest()
    {
      Assert.That(ExtractDefaultSchema(), Is.Not.Null);

      var catalog = ExtractCatalog();
      if (StorageProviderInfo.Instance.CheckAllFeaturesNotSupported(Providers.ProviderFeatures.Multischema)) {
        Assert.That(schemasToCheck.All(s => catalog.Schemas[s] != null), Is.True);
      }
    }

    protected abstract string GetTypesExtractionPrepareScript(string tableName);
    protected abstract string GetTypesExtractionCleanUpScript(string tableName);

    // Test expects a table with the given name containing column for each supported by
    // the tested storage, column names for each SqlType are in TypeToColumnName dictionary
    [Test]
    public void TypeExtractionTest()
    {
      var createTableQuery = GetTypesExtractionPrepareScript("dataTypesTestTable");
      RegisterCleanupScript(GetTypesExtractionCleanUpScript, "dataTypesTestTable");
      ExecuteQuery(createTableQuery);

      var testTable = ExtractDefaultSchema().Tables["dataTypesTestTable"];

      foreach (var keyValue in TypeToColumnName) {
        var sqlType = keyValue.Key;
        var columnName = keyValue.Value;
        var tableColumn = testTable.TableColumns[columnName];
        if (tableColumn == null) {
          continue;
        }
        Assert.That(tableColumn.DataType.Type, Is.EqualTo(sqlType));
        if (sqlType == SqlType.Char || sqlType == SqlType.VarChar
          || sqlType == SqlType.Binary || sqlType == SqlType.VarBinary) {
          Assert.That(tableColumn.DataType.Length, Is.EqualTo(GetExpectedLength(sqlType)));
        }
        if (sqlType == SqlType.Decimal && StorageProviderInfo.Instance.CheckProviderIsNot(StorageProvider.Sqlite)) {
          Assert.That(tableColumn.DataType.Precision, Is.EqualTo(DecimalPrecision));
          Assert.That(tableColumn.DataType.Scale, Is.EqualTo(DecimalScale));
        }
      }


      static int GetExpectedLength(SqlType sqlType1)
      {
        if (sqlType1 == SqlType.Char) {
          return CharLength;
        }
        else if (sqlType1 == SqlType.VarChar) {
          return VarCharLength;
        }
        else if (sqlType1 == SqlType.Binary) {
          return BinaryLength;
        }
        else if (sqlType1 == SqlType.VarBinary) {
          return VarBinaryLength;
        }
        throw new ArgumentOutOfRangeException(nameof(sqlType1));
      }
    }


    protected abstract string GetForeignKeyExtractionPrepareScript();
    protected abstract string GetForeignKeyExtractionCleanUpScript();

    // Test expects Storage variant of following structure
    //  CREATE TABLE B1 (b_id int primary key);
    //  CREATE TABLE A1 (b_id int references B1(b_id));
    //  CREATE TABLE B2 (b_id_1 int, b_id_2 int,
    //    CONSTRAINT [B2_PK] PRIMARY KEY  CLUSTERED (b_id_1, b_id_2)  ON [PRIMARY]);
    //  CREATE TABLE A2 (b_id_1 int, b_id_2 int,
    //    CONSTRAINT [A2_FK] FOREIGN KEY (b_id_1, b_id_2)
    //    REFERENCES B2 (b_id_1, b_id_2) ON DELETE CASCADE ON UPDATE NO ACTION);
    //  CREATE TABLE B3 (b_id_1 int, b_id_2 int, b_id_3 int,
    //    CONSTRAINT [B3_PK] PRIMARY KEY  CLUSTERED (b_id_1, b_id_2, b_id_3)  ON [PRIMARY]);
    //  CREATE TABLE A3 (A_col1 int, b_id_3 int, b_id_1 int, b_id_2 int,
    //    CONSTRAINT [A3_FK] FOREIGN KEY (b_id_1, b_id_2, b_id_3)
    //    REFERENCES B3 (b_id_1, b_id_2, b_id_3) ON DELETE NO ACTION ON UPDATE CASCADE);
    [Test]
    public void ForeignKeyExtractionTest()
    {
      var query = GetForeignKeyExtractionPrepareScript();
      RegisterCleanupScript(GetForeignKeyExtractionCleanUpScript);
      ExecuteQuery(query);

      var schema = ExtractDefaultSchema();

      // Validating.
      var fk1 = (ForeignKey) schema.Tables["A1"].TableConstraints[0];
      Assert.IsNotNull(fk1);
      Assert.IsTrue(fk1.Columns[0].Name.Equals("b_id", StringComparison.OrdinalIgnoreCase));
      Assert.IsTrue(fk1.ReferencedColumns[0].Name.Equals("b_id", StringComparison.OrdinalIgnoreCase));
      Assert.IsTrue(fk1.ReferencedColumns.Count == 1);
      Assert.IsTrue(fk1.Columns.Count == 1);

      var fk2 = (ForeignKey) schema.Tables["A2"].TableConstraints[0];
      Assert.IsNotNull(fk1);
      Assert.IsTrue(fk2.Columns[0].Name.Equals("b_id_1", StringComparison.OrdinalIgnoreCase));
      Assert.IsTrue(fk2.ReferencedColumns[0].Name.Equals("b_id_1", StringComparison.OrdinalIgnoreCase));
      Assert.IsTrue(fk2.Columns[1].Name.Equals("b_id_2", StringComparison.OrdinalIgnoreCase));
      Assert.IsTrue(fk2.ReferencedColumns[1].Name.Equals("b_id_2", StringComparison.OrdinalIgnoreCase));
      Assert.IsTrue(fk2.ReferencedColumns.Count == 2);
      Assert.IsTrue(fk2.Columns.Count == 2);
      if (StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.Oracle)) {
        Assert.IsTrue(fk2.OnDelete == ReferentialAction.SetNull);
      }
      else {
        Assert.IsTrue(fk2.OnDelete == ReferentialAction.Cascade);
        Assert.IsTrue(fk2.OnUpdate == ReferentialAction.NoAction);
      }
      

      var fk3 = (ForeignKey) schema.Tables["A3"].TableConstraints[0];
      Assert.IsNotNull(fk3);
      Assert.IsTrue(fk3.Columns[0].Name.Equals("b_id_1", StringComparison.OrdinalIgnoreCase));
      Assert.IsTrue(fk3.ReferencedColumns[0].Name.Equals("b_id_1", StringComparison.OrdinalIgnoreCase));
      Assert.IsTrue(fk3.Columns[1].Name.Equals("b_id_2", StringComparison.OrdinalIgnoreCase));
      Assert.IsTrue(fk3.ReferencedColumns[1].Name.Equals("b_id_2", StringComparison.OrdinalIgnoreCase));
      Assert.IsTrue(fk3.Columns[2].Name.Equals("b_id_3", StringComparison.OrdinalIgnoreCase));
      Assert.IsTrue(fk3.ReferencedColumns[2].Name.Equals("b_id_3", StringComparison.OrdinalIgnoreCase));
      Assert.IsTrue(fk3.ReferencedColumns.Count == 3);
      Assert.IsTrue(fk3.Columns.Count == 3);
      if (StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.Oracle)) {
        Assert.IsTrue(fk3.OnDelete == ReferentialAction.Cascade);
      }
      else {
        Assert.IsTrue(fk3.OnDelete == ReferentialAction.NoAction);
        Assert.IsTrue(fk3.OnUpdate == ReferentialAction.Cascade);
      }
    }


    protected abstract string GetIndexExtractionPrepareScript(string tableName);
    protected abstract string GetIndexExtractionCleanUpScript(string tableName);

    // Test expects storage variant of following structure
    // CREATE TABLE table1 (column1 int,  column2 int);
    // CREATE INDEX table1_index1_desc_asc on table1 (column1 desc, column2 asc);
    // CREATE UNIQUE INDEX table1_index1_u_asc_desc on table1 (column1 asc, column2 desc);
    // CREATE UNIQUE INDEX table1_index_with_included_columns on table1 (column1 asc) include (column2);
    //
    // if  non-key columns are not supported then skip their declaration.
    [Test]
    public void IndexExtractionTest()
    {
      var query = GetIndexExtractionPrepareScript("table1");
      RegisterCleanupScript(GetIndexExtractionCleanUpScript, "table1");
      ExecuteQuery(query);

      var schema = ExtractDefaultSchema();

      Assert.IsTrue(schema.Tables["table1"] != null);
      Assert.IsNotNull(schema.Tables["table1"].Indexes["table1_index1_desc_asc"]);
      Assert.IsTrue(schema.Tables["table1"].Indexes["table1_index1_desc_asc"].Columns.Count == 2);
      Assert.IsTrue(schema.Tables["table1"].Indexes["table1_index1_desc_asc"].Columns[0].Name.Equals("column1", StringComparison.OrdinalIgnoreCase));
      Assert.IsTrue(schema.Tables["table1"].Indexes["table1_index1_desc_asc"].Columns[1].Name.Equals("column2", StringComparison.OrdinalIgnoreCase));
      if (SortOrderSupported) {
        Assert.IsTrue(!schema.Tables["table1"].Indexes["table1_index1_desc_asc"].Columns[0].Ascending);
        Assert.IsTrue(schema.Tables["table1"].Indexes["table1_index1_desc_asc"].Columns[1].Ascending);
      }
      else {
        Assert.IsTrue(schema.Tables["table1"].Indexes["table1_index1_desc_asc"].Columns[0].Ascending);
        Assert.IsTrue(schema.Tables["table1"].Indexes["table1_index1_desc_asc"].Columns[1].Ascending);
      }


      Assert.IsNotNull(schema.Tables["table1"].Indexes["table1_index1_u_asc_desc"]);
      Assert.IsTrue(schema.Tables["table1"].Indexes["table1_index1_u_asc_desc"].Columns.Count == 2);
      Assert.IsTrue(schema.Tables["table1"].Indexes["table1_index1_u_asc_desc"].Columns[0].Name.Equals("column1", StringComparison.OrdinalIgnoreCase));
      Assert.IsTrue(schema.Tables["table1"].Indexes["table1_index1_u_asc_desc"].Columns[1].Name.Equals("column2", StringComparison.OrdinalIgnoreCase));

      if (SortOrderSupported) {
        Assert.IsTrue(schema.Tables["table1"].Indexes["table1_index1_u_asc_desc"].Columns[0].Ascending);
        Assert.IsTrue(!schema.Tables["table1"].Indexes["table1_index1_u_asc_desc"].Columns[1].Ascending);
      }
      else {
        Assert.IsTrue(schema.Tables["table1"].Indexes["table1_index1_u_asc_desc"].Columns[0].Ascending);
        Assert.IsTrue(schema.Tables["table1"].Indexes["table1_index1_u_asc_desc"].Columns[1].Ascending);
      }

      if (NonKeyColumnsSupported) {
        Assert.IsNotNull(schema.Tables["table1"].Indexes["table1_index_with_included_columns"]);
        Assert.AreEqual(1, schema.Tables["table1"].Indexes["table1_index_with_included_columns"].Columns.Count,
          "Key columns");
      
        Assert.AreEqual(1, schema.Tables["table1"].Indexes["table1_index_with_included_columns"].NonkeyColumns.Count,
          "Included columns");
      }
    }


    protected virtual string GetPartialIndexExtractionPrepareScript(string tableName) => null;
    protected virtual string GetPartialIndexExtractionCleanUpScript(string tableName) => null;

    [Test]
    public void PartialIndexExtractionTest()
    {
      Require.AllFeaturesSupported(Providers.ProviderFeatures.PartialIndexes);

      var query = GetPartialIndexExtractionPrepareScript("partialIndexTestTable");
      RegisterCleanupScript(GetPartialIndexExtractionCleanUpScript, "partialIndexTestTable");
      ExecuteQuery(query);

      var schema = ExtractDefaultSchema();

      Assert.IsTrue(schema.Tables["partialIndexTestTable"] != null);
      Assert.IsNotNull(schema.Tables["partialIndexTestTable"].Indexes["partialIndexTestTable_index1_filtered"]);
      Assert.IsTrue(schema.Tables["partialIndexTestTable"].Indexes["partialIndexTestTable_index1_filtered"].Columns.Count == 2);
      Assert.IsTrue(schema.Tables["partialIndexTestTable"].Indexes["partialIndexTestTable_index1_filtered"].Columns[0].Name == "column1");
      Assert.IsNotNull(schema.Tables["partialIndexTestTable"].Indexes["partialIndexTestTable_index1_filtered"].Where);
    }


    protected virtual string GetFulltextIndexExtractionPrepareScript(string tableName) => null;
    protected virtual string GetFulltextIndexExtractionCleanUpScript(string tableName) => null;

    // Test expects storage variant of following structure
    // CREATE TABLE fullTextTestTable (Id int NOT NULL,
    //   Name nvarchar(100) NULL,
    //   Comments nvarchar(1000) NULL,
    //   CONSTRAINT [PK_fullTextTestTable] PRIMARY KEY CLUSTERED (Id)  ON [PRIMARY]);
    //
    // CREATE FULLTEXT INDEX ON fullTextTestTable(Name LANGUAGE 1033, Comments LANGUAGE 1033)
    //   KEY INDEX PK_fullTextTestTable;
    [Test]
    public void FulltextIndexExtractionTest()
    {
      Require.AllFeaturesSupported(Providers.ProviderFeatures.FullText);

      var query = GetFulltextIndexExtractionPrepareScript("fullTextTestTable");
      RegisterCleanupScript(GetFulltextIndexExtractionCleanUpScript, "fullTextTestTable");
      ExecuteQuery(query);

      var schema = ExtractDefaultSchema();
      var ftIndex = (FullTextIndex) schema.Tables["fullTextTestTable"].Indexes.FirstOrDefault(i => i.IsFullText);
      Assert.IsNotNull(ftIndex);
      Assert.That(ftIndex.Columns.Count, Is.EqualTo(2));
      Assert.That(
        ftIndex.Columns.All(c => c.Languages.Count == 1 && c.Languages[0].Name.Equals("English", StringComparison.OrdinalIgnoreCase)),
        Is.True);
      if (StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.SqlServer)) {
        Assert.That(ftIndex.FullTextCatalog, Is.Null);
        Assert.That(ftIndex.ChangeTrackingMode, Is.EqualTo(ChangeTrackingMode.Auto));
        Assert.That(ftIndex.UnderlyingUniqueIndex, Is.EqualTo("PK_fullTextTestTable"));
      }
    }


    protected virtual string GetUniqueConstraintExtractionPrepareScript(string tableName) => "";
    protected virtual string GetUniqueConstraintExtractionCleanUpScript(string tableName) => "";

    // Test expects storage variant of following structure
    // CREATE TABLE uniqueConstraintTable (
    //   col_11 int, col_12 int, col_13 int,
    //   col_21 int, col_22 int, col_23 int,
    //   CONSTRAINT A_UNIQUE_1 UNIQUE(col_11,col_12,col_13),
    //   CONSTRAINT A_UNIQUE_2 UNIQUE(col_21,col_22,col_23))
    [Test]
    public void UniqueConstraintExtractionTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite);

      var query = GetUniqueConstraintExtractionPrepareScript("uniqueConstraintTable");
      RegisterCleanupScript(GetUniqueConstraintExtractionCleanUpScript, "uniqueConstraintTable");
      ExecuteQuery(query);

      var schema = ExtractDefaultSchema();

      // Validating.
      var uniqueConstraint = (UniqueConstraint) schema.Tables["uniqueConstraintTable"].TableConstraints["A_UNIQUE_1"];
      Assert.IsNotNull(uniqueConstraint);
      Assert.IsTrue(uniqueConstraint.Columns[0].Name.Equals("col_11", StringComparison.OrdinalIgnoreCase));
      Assert.IsTrue(uniqueConstraint.Columns[1].Name.Equals("col_12", StringComparison.OrdinalIgnoreCase));
      Assert.IsTrue(uniqueConstraint.Columns[2].Name.Equals("col_13", StringComparison.OrdinalIgnoreCase));
      Assert.IsTrue(uniqueConstraint.Columns.Count == 3);

      uniqueConstraint = (UniqueConstraint) schema.Tables["uniqueConstraintTable"].TableConstraints["A_UNIQUE_2"];
      Assert.IsNotNull(uniqueConstraint);
      Assert.IsTrue(uniqueConstraint.Columns[0].Name.Equals("col_21", StringComparison.OrdinalIgnoreCase));
      Assert.IsTrue(uniqueConstraint.Columns[1].Name.Equals("col_22", StringComparison.OrdinalIgnoreCase));
      Assert.IsTrue(uniqueConstraint.Columns[2].Name.Equals("col_23", StringComparison.OrdinalIgnoreCase));
      Assert.IsTrue(uniqueConstraint.Columns.Count == 3);
    }


    protected virtual string GetCheckConstraintExtractionPrepareScript(string tableName) => null;
    protected virtual string GetCheckConstraintExtractionCleanUpScript(string tableName) => null;

    // Test expects storage variant of following structure
    // CREATE TABLE checkConstraintTable (
    //   col_11 int, col_12 int, col_13 int,
    //   col_21 int, col_22 int, col_23 int,
    //   CONSTRAINT A_CHECK_1 CHECK(col_11 > 0 OR col_12 > 10 OR col_13 > 20),
    //   CONSTRAINT A_CHECK_2 CHECK(col_21 <0 AND col_22 < 10 AND col_23 < 20))
    [Test]
    public void CheckConstraintExtractionTest()
    {
      if (!CheckContstraintExtracted)
        throw new IgnoreException("CheckConstraints are not extracted");

      var query = GetCheckConstraintExtractionPrepareScript("checkConstraintTable");
      RegisterCleanupScript(GetCheckConstraintExtractionCleanUpScript, "checkConstraintTable");
      ExecuteQuery(query);

      var schema = ExtractDefaultSchema();

      // Validating.
      var checkConstraint = (CheckConstraint) schema.Tables["checkConstraintTable"].TableConstraints["A_CHECK_1"];
      Assert.IsNotNull(checkConstraint);
      Assert.IsNotNull(checkConstraint.Condition);

      checkConstraint = (CheckConstraint) schema.Tables["checkConstraintTable"].TableConstraints["A_CHECK_2"];
      Assert.IsNotNull(checkConstraint);
      Assert.IsNotNull(checkConstraint.Condition);
    }


    protected virtual string GetCheckSequenceExtractionPrepareScript()
    {
      return "CREATE SEQUENCE \"seq1\" START WITH 11 INCREMENT BY 100 MINVALUE 10 MAXVALUE 10000 NO CYCLE;" +
        "CREATE SEQUENCE \"seq2\" START WITH 110 INCREMENT BY 10 MINVALUE 10 MAXVALUE 100000 CYCLE;";
    }

    protected virtual string GetCheckSequenceExtractionCleanupScript() => "DROP SEQUENCE \"seq1\"; DROP SEQUENCE \"seq2\"";

    [Test]
    public void SequenceExtractionTest()
    {
      Require.AllFeaturesSupported(Providers.ProviderFeatures.Sequences);

      var query = GetCheckSequenceExtractionPrepareScript();
      RegisterCleanupScript(GetCheckSequenceExtractionCleanupScript);
      ExecuteQuery(query);

      var schema = ExtractDefaultSchema();
      Assert.That(schema.Sequences["seq1"], Is.Not.Null);
      var seq1Descriptor = schema.Sequences["seq1"].SequenceDescriptor;
      if (StorageProviderInfo.Instance.CheckProviderIsNot(StorageProvider.Oracle))
        Assert.That(seq1Descriptor.StartValue, Is.EqualTo(SeqStartEqualsToMin ? 10 : 11));
      Assert.That(seq1Descriptor.Increment, Is.EqualTo(100));
      Assert.That(seq1Descriptor.MinValue, Is.EqualTo(10));
      Assert.That(seq1Descriptor.MaxValue, Is.EqualTo(10000));

      Assert.That(seq1Descriptor.IsCyclic, Is.False);

      Assert.That(schema.Sequences["seq2"], Is.Not.Null);
      var seq2Descriptor = schema.Sequences["seq2"].SequenceDescriptor;
      if (StorageProviderInfo.Instance.CheckProviderIsNot(StorageProvider.Oracle))
        Assert.That(seq2Descriptor.StartValue, Is.EqualTo(SeqStartEqualsToMin ? 10 : 110));
      Assert.That(seq2Descriptor.Increment, Is.EqualTo(10));
      Assert.That(seq2Descriptor.MinValue, Is.EqualTo(10));
      Assert.That(seq2Descriptor.MaxValue, Is.EqualTo(100000));

      Assert.That(seq2Descriptor.IsCyclic, Is.True);
    }

    private void PopulateTypeToColumnName()
    {
      TypeToColumnName[SqlType.Boolean] = "boolean_column";
      TypeToColumnName[SqlType.Int8] = "int8_column";
      TypeToColumnName[SqlType.Int16] = "int16_column";
      TypeToColumnName[SqlType.Int32] = "int32_column";
      TypeToColumnName[SqlType.Int64] = "int64_column";
      TypeToColumnName[SqlType.UInt8] = "uint8_column";
      TypeToColumnName[SqlType.UInt16] = "uint16_column";
      TypeToColumnName[SqlType.UInt32] = "uint32_column";
      TypeToColumnName[SqlType.UInt64] = "uint64_column";
      TypeToColumnName[SqlType.Decimal] = $"decimal_p{DecimalPrecision}_s{DecimalScale}_column";

      TypeToColumnName[SqlType.Float] = "float_column";
      TypeToColumnName[SqlType.Double] = "double_column";

      TypeToColumnName[SqlType.Interval] = "interval_column";
      TypeToColumnName[SqlType.DateTime] = "datetime_column";
      TypeToColumnName[SqlType.DateTimeOffset] = "datetimeoffset_column";
      TypeToColumnName[SqlType.Date] = "date_column";
      TypeToColumnName[SqlType.Time] = "time_column";

      TypeToColumnName[SqlType.Char] = $"char_l{CharLength}_column";
      TypeToColumnName[SqlType.VarChar] = $"varchar_l{VarCharLength}_column";
      TypeToColumnName[SqlType.VarCharMax] = "varcharmax_column";

      TypeToColumnName[SqlType.Binary] = $"binary_l{BinaryLength}_column";
      TypeToColumnName[SqlType.VarBinary] = $"varbinary_l{VarBinaryLength}_column";
      TypeToColumnName[SqlType.VarBinaryMax] = "varbinarymax_column";

      TypeToColumnName[SqlType.Guid] = "guid_column";

      PopulateCustomTypeToColumnName();
    }

    protected virtual void PopulateCustomTypeToColumnName()
    {
    }

    private void PopulateSchemasToCheck()
    {
      if (!IsMultischemaSupported) {
        return;
      }

      if (StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.PostgreSql)) {
        schemasToCheck.Add(WellKnownSchemas.PgSqlDefalutSchema);
      }
      else {
        schemasToCheck.Add(WellKnownSchemas.SqlServerDefaultSchema);
      }
      schemasToCheck.Add(WellKnownSchemas.Schema1);
      schemasToCheck.Add(WellKnownSchemas.Schema2);
      schemasToCheck.Add(WellKnownSchemas.Schema3);
      schemasToCheck.Add(WellKnownSchemas.Schema4);
      schemasToCheck.Add(WellKnownSchemas.Schema5);
      schemasToCheck.Add(WellKnownSchemas.Schema6);
      schemasToCheck.Add(WellKnownSchemas.Schema7);
      schemasToCheck.Add(WellKnownSchemas.Schema8);
      schemasToCheck.Add(WellKnownSchemas.Schema9);
      schemasToCheck.Add(WellKnownSchemas.Schema10);
      schemasToCheck.Add(WellKnownSchemas.Schema11);
      schemasToCheck.Add(WellKnownSchemas.Schema12);
    }

    protected void ExecuteQuery(string sqlQuery)
    {
      if (string.IsNullOrEmpty(sqlQuery))
        return;
      if(Driver.ServerInfo.Query.Features.HasFlag(QueryFeatures.DdlBatches)) {
        _ = ExecuteNonQuery(sqlQuery);
      }
      else {
        ExecuteQueryLineByLine(sqlQuery);
      }
    }

    protected void ExecuteQueryLineByLine(string sqlQuery, bool ignoreExceptions = false)
    {
      if (string.IsNullOrEmpty(sqlQuery))
        return;
      foreach (var q in sqlQuery.Split(';')) {
        if (string.IsNullOrEmpty(q))
          continue;
        try {
          _ = ExecuteNonQuery(q);
        }
        catch {
          if (!ignoreExceptions)
            throw;
        }
      }
    }

    protected void RegisterCleanupScript(Func<string> func) => cleanups.Add(func());
    protected void RegisterCleanupScript(Func<string, string> func, string param) => cleanups.Add(func(param));
  }
}
