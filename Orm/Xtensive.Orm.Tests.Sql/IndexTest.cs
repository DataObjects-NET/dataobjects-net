// Copyright (C) 2009-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2009.08.31

using System;
using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Sql
{
  [TestFixture]
  public abstract class IndexTest : SqlTest
  {
    protected const string ExpressionIndexTableName = "exp_ind_table";
    protected const string GeneralPurposeTableName = "general_index_table";
    protected const string ExpressionIndexName = "IX_EXPRESSION";
    protected const string FilteredIndexName = "IX_FILTERED";

    private Schema schema;
    private Catalog catalog;
    private Xtensive.Sql.Info.IndexInfo indexInfo;

    protected Table GeneralPurposeTable => schema?.Tables[GeneralPurposeTableName];
    protected Table ExpressionIndexTable => schema?.Tables[ExpressionIndexTableName];

    protected override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      indexInfo = Driver.ServerInfo.Index;

      catalog = ExtractCatalog();
      schema = catalog.DefaultSchema;

      try {
        Connection.BeginTransaction();

        DropTablesIfExist(openTranaction: false);

        CreateGeneralPurposeTable();
        CreateExpressionIndexTable(schema);
        Connection.Commit();
      }
      catch {
        if (Connection.ActiveTransaction != null)
          Connection.Rollback();
        Connection.Close();
        Connection.Dispose();
        throw;
      }
    }

    protected override void TestFixtureTearDown()
    {
      if (Connection is null)
        return;
      if (Connection.State != System.Data.ConnectionState.Open) {
        base.TestFixtureTearDown();
        return;
      }
      if (schema != null) {
        try {
          DropTablesIfExist(openTranaction: true);
        }
        catch {
          if (Connection.ActiveTransaction != null)
            Connection.Rollback();
        }
      }
      base.TestFixtureTearDown();
    }

    private void DropTablesIfExist(bool openTranaction)
    {
      var cTable = schema.Tables[ExpressionIndexTableName];
      var gpTable = schema.Tables[GeneralPurposeTableName];
      if (cTable is null && gpTable is null)
        return;
      if (openTranaction)
        Connection.BeginTransaction();
      try {
        if (cTable is not null) {
          _ = ExecuteNonQuery(SqlDdl.Drop(cTable));
          _ = schema.Tables.Remove(cTable);
        }
        if (gpTable is not null) {
          _ = ExecuteNonQuery(SqlDdl.Drop(gpTable));
          _ = schema.Tables.Remove(gpTable);
        }
      }
      catch {
        if (openTranaction && Connection.ActiveTransaction != null)
          Connection.Rollback();
        throw;
      }
      finally {
        if (openTranaction)
          Connection.Commit();
      }
    }

    protected virtual void CreateExpressionIndexTable(Schema schema)
    {
    }

    protected virtual void CreateGeneralPurposeTable()
    {
      var gpTable = schema.CreateTable(GeneralPurposeTableName);
      var boolType = Driver.TypeMappings[typeof(bool)].MapType();
      var int16Type = Driver.TypeMappings[typeof(short)].MapType();
      var int32Type = Driver.TypeMappings[typeof(int)].MapType();
      var int64Type = Driver.TypeMappings[typeof(long)].MapType();
      var string32Type = Driver.TypeMappings[typeof(string)].MapType(32, null, null);
      var string64Type = Driver.TypeMappings[typeof(string)].MapType(64, null, null);
      var string128Type = Driver.TypeMappings[typeof(string)].MapType(128, null, null);
      var stringMaxType = Driver.TypeMappings[typeof(string)].MapType(int.MaxValue, null, null);

      var c = gpTable.CreateColumn("bool1", boolType);
      c = gpTable.CreateColumn("bool1n", boolType);
      c.IsNullable = true;

      c = gpTable.CreateColumn("bool2", boolType);
      c = gpTable.CreateColumn("bool2n", boolType);
      c.IsNullable = true;

      c = gpTable.CreateColumn("int16_1", int16Type);
      c = gpTable.CreateColumn("int16_1n", int16Type);
      c.IsNullable = true;

      c = gpTable.CreateColumn("int16_2", int16Type);
      c = gpTable.CreateColumn("int16_2n", int16Type);
      c.IsNullable = true;

      c = gpTable.CreateColumn("int32_1", int32Type);
      c = gpTable.CreateColumn("int32_1n", int32Type);
      c.IsNullable = true;

      c = gpTable.CreateColumn("int32_2", int32Type);
      c = gpTable.CreateColumn("int32_2n", int32Type);
      c.IsNullable = true;

      c = gpTable.CreateColumn("int64_1", int64Type);
      c = gpTable.CreateColumn("int64_1n", int64Type);
      c.IsNullable = true;

      c = gpTable.CreateColumn("int64_2", int64Type);
      c = gpTable.CreateColumn("int64_2n", int64Type);
      c.IsNullable = true;

      c = gpTable.CreateColumn("str32", string32Type);
      c = gpTable.CreateColumn("str32n", string32Type);
      c.IsNullable = true;

      c = gpTable.CreateColumn("str64", string64Type);
      c = gpTable.CreateColumn("str64n", string64Type);
      c.IsNullable = true;

      c = gpTable.CreateColumn("str128", string128Type);
      c = gpTable.CreateColumn("str128n", string128Type);
      c.IsNullable = true;

      c = gpTable.CreateColumn("strmax", stringMaxType);
      c = gpTable.CreateColumn("strmaxn", stringMaxType);
      c.IsNullable = true;

      AddCustomColumns(gpTable);

      _ = ExecuteNonQuery(SqlDdl.Create(gpTable));
    }

    protected virtual void AddCustomColumns(Table generalPurposeTable)
    {
    }

    [Test]
    public void CreateSimpleIndexTest()
    {
      var t = GeneralPurposeTable;
      var index1 = t.CreateIndex("ix_gptable_simple");
      _ = index1.CreateIndexColumn(t.TableColumns["int16_1"]);
      _ = ExecuteNonQuery(SqlDdl.Create(index1));

      var index2 = t.CreateIndex("ix_gptable_simple_n");
      _ = index2.CreateIndexColumn(t.TableColumns["int16_1n"]);
      _ = ExecuteNonQuery(SqlDdl.Create(index2));

      var schema = ExtractDefaultSchema();
      var extractedTable = schema.Tables[GeneralPurposeTableName];
      var extractedIndex1 = extractedTable.Indexes["ix_gptable_simple"];
      Assert.That(extractedIndex1, Is.Not.Null);
      Assert.That(extractedIndex1.Columns.Count, Is.EqualTo(1));
      Assert.That(extractedIndex1.Columns[0].Name, Is.EqualTo("int16_1"));

      var extractedIndex2 = extractedTable.Indexes["ix_gptable_simple_n"];
      Assert.That(extractedIndex2, Is.Not.Null);
      Assert.That(extractedIndex2.Columns.Count, Is.EqualTo(1));
      Assert.That(extractedIndex2.Columns[0].Name, Is.EqualTo("int16_1n"));
    }

    [Test]
    public void CreateMulticolumnIndexTest()
    {
      var t = GeneralPurposeTable;
      var index1 = t.CreateIndex("ix_gptable_mc");
      _ = index1.CreateIndexColumn(t.TableColumns["int16_1"]);
      _ = index1.CreateIndexColumn(t.TableColumns["int16_2"]);
      _ = index1.CreateIndexColumn(t.TableColumns["int32_1"]);
      _ = ExecuteNonQuery(SqlDdl.Create(index1));

      var index2 = t.CreateIndex("ix_gptable_mc_n");
      _ = index2.CreateIndexColumn(t.TableColumns["int16_1n"]);
      _ = index2.CreateIndexColumn(t.TableColumns["int16_2n"]);
      _ = index2.CreateIndexColumn(t.TableColumns["int32_1n"]);
      _ = ExecuteNonQuery(SqlDdl.Create(index2));

      var schema = ExtractDefaultSchema();
      var extractedTable = schema.Tables[GeneralPurposeTableName];
      var extractedIndex1 = extractedTable.Indexes["ix_gptable_mc"];
      Assert.That(extractedIndex1, Is.Not.Null);
      Assert.That(extractedIndex1.Columns.Count, Is.EqualTo(3));
      Assert.That(extractedIndex1.Columns[0].Name, Is.EqualTo("int16_1"));
      Assert.That(extractedIndex1.Columns[1].Name, Is.EqualTo("int16_2"));
      Assert.That(extractedIndex1.Columns[2].Name, Is.EqualTo("int32_1"));

      var extractedIndex2 = extractedTable.Indexes["ix_gptable_mc_n"];
      Assert.That(extractedIndex2, Is.Not.Null);
      Assert.That(extractedIndex2.Columns.Count, Is.EqualTo(3));
      Assert.That(extractedIndex2.Columns[0].Name, Is.EqualTo("int16_1n"));
      Assert.That(extractedIndex2.Columns[1].Name, Is.EqualTo("int16_2n"));
      Assert.That(extractedIndex2.Columns[2].Name, Is.EqualTo("int32_1n"));
    }

    [Test]
    public void CreateSimpleIndexAscDescTest()
    {
      CheckSortOrderSupported();

      var t = GeneralPurposeTable;
      var index1 = t.CreateIndex("ix_gptable_simple_so");
      _ = index1.CreateIndexColumn(t.TableColumns["int16_1"], false);
      _ = ExecuteNonQuery(SqlDdl.Create(index1));

      var index2 = t.CreateIndex("ix_gptable_simple_so_n");
      _ = index2.CreateIndexColumn(t.TableColumns["int16_1n"], true);
      _ = ExecuteNonQuery(SqlDdl.Create(index2));

      var schema = ExtractDefaultSchema();
      var extractedTable = schema.Tables[GeneralPurposeTableName];
      var extractedIndex1 = extractedTable.Indexes["ix_gptable_simple_so"];
      Assert.That(extractedIndex1, Is.Not.Null);
      Assert.That(extractedIndex1.Columns.Count, Is.EqualTo(1));
      Assert.That(extractedIndex1.Columns[0].Name, Is.EqualTo("int16_1"));
      Assert.That(extractedIndex1.Columns[0].Ascending, Is.False);

      var extractedIndex2 = extractedTable.Indexes["ix_gptable_simple_so_n"];
      Assert.That(extractedIndex2, Is.Not.Null);
      Assert.That(extractedIndex2.Columns.Count, Is.EqualTo(1));
      Assert.That(extractedIndex2.Columns[0].Name, Is.EqualTo("int16_1n"));
      Assert.That(extractedIndex2.Columns[0].Ascending, Is.True);
    }

    [Test]
    public void CreateMulticolumnIndexAscDescTest()
    {
      CheckSortOrderSupported();

      var t = GeneralPurposeTable;
      var index1 = t.CreateIndex("ix_gptable_mc_so");
      _ = index1.CreateIndexColumn(t.TableColumns["int16_1"], true);
      _ = index1.CreateIndexColumn(t.TableColumns["int16_2"], false);
      _ = index1.CreateIndexColumn(t.TableColumns["int32_1"], true);
      _ = ExecuteNonQuery(SqlDdl.Create(index1));

      var index2 = t.CreateIndex("ix_gptable_mc_so_n");
      _ = index2.CreateIndexColumn(t.TableColumns["int16_1n"], false);
      _ = index2.CreateIndexColumn(t.TableColumns["int16_2n"], true);
      _ = index2.CreateIndexColumn(t.TableColumns["int32_1n"], false);
      _ = ExecuteNonQuery(SqlDdl.Create(index2));

      var schema = ExtractDefaultSchema();
      var extractedTable = schema.Tables[GeneralPurposeTableName];
      var extractedIndex1 = extractedTable.Indexes["ix_gptable_mc_so"];
      Assert.That(extractedIndex1, Is.Not.Null);
      Assert.That(extractedIndex1.Columns.Count, Is.EqualTo(3));
      Assert.That(extractedIndex1.Columns[0].Name, Is.EqualTo("int16_1"));
      Assert.That(extractedIndex1.Columns[0].Ascending, Is.True);
      Assert.That(extractedIndex1.Columns[1].Name, Is.EqualTo("int16_2"));
      Assert.That(extractedIndex1.Columns[1].Ascending, Is.False);
      Assert.That(extractedIndex1.Columns[2].Name, Is.EqualTo("int32_1"));
      Assert.That(extractedIndex1.Columns[2].Ascending, Is.True);

      var extractedIndex2 = extractedTable.Indexes["ix_gptable_mc_so_n"];
      Assert.That(extractedIndex2, Is.Not.Null);
      Assert.That(extractedIndex2.Columns.Count, Is.EqualTo(3));
      Assert.That(extractedIndex2.Columns[0].Name, Is.EqualTo("int16_1n"));
      Assert.That(extractedIndex2.Columns[0].Ascending, Is.False);
      Assert.That(extractedIndex2.Columns[1].Name, Is.EqualTo("int16_2n"));
      Assert.That(extractedIndex2.Columns[1].Ascending, Is.True);
      Assert.That(extractedIndex2.Columns[2].Name, Is.EqualTo("int32_1n"));
      Assert.That(extractedIndex2.Columns[2].Ascending, Is.False);
    }

    [Test]
    public void CreateUniqueSimpleIndexTest()
    {
      CheckUniqueIndexesSupported();

      var t = GeneralPurposeTable;
      var index1 = t.CreateIndex("ixu_gptable_simple");
      index1.IsUnique = true;
      _ = index1.CreateIndexColumn(t.TableColumns["int16_1"]);
      _ = ExecuteNonQuery(SqlDdl.Create(index1));

      var schema = ExtractDefaultSchema();
      var extractedTable = schema.Tables[GeneralPurposeTableName];
      var extractedIndex1 = extractedTable.Indexes["ixu_gptable_simple"];
      Assert.That(extractedIndex1, Is.Not.Null);
      Assert.That(extractedIndex1.IsUnique, Is.True);
      Assert.That(extractedIndex1.Columns.Count, Is.EqualTo(1));
      Assert.That(extractedIndex1.Columns[0].Name, Is.EqualTo("int16_1"));
    }

    [Test]
    public void CreateUniqueMulticolumnIndexTest()
    {
      CheckUniqueIndexesSupported();

      var t = GeneralPurposeTable;
      var index1 = t.CreateIndex("ixu_gptable_mc");
      index1.IsUnique = true;
      _ = index1.CreateIndexColumn(t.TableColumns["int16_1"]);
      _ = index1.CreateIndexColumn(t.TableColumns["int16_1n"]);
      _ = ExecuteNonQuery(SqlDdl.Create(index1));

      var schema = ExtractDefaultSchema();
      var extractedTable = schema.Tables[GeneralPurposeTableName];
      var extractedIndex1 = extractedTable.Indexes["ixu_gptable_mc"];
      Assert.That(extractedIndex1, Is.Not.Null);
      Assert.That(extractedIndex1.IsUnique, Is.True);
      Assert.That(extractedIndex1.Columns.Count, Is.EqualTo(2));
      Assert.That(extractedIndex1.Columns[0].Name, Is.EqualTo("int16_1"));
      Assert.That(extractedIndex1.Columns[1].Name, Is.EqualTo("int16_1n"));
    }

    [Test]
    public void CreateNonKeyColumnIndexTest()
    {
      CheckNonKeyColumnsSupported();

      var t = GeneralPurposeTable;
      var index1 = t.CreateIndex("ix_gptable_simple_nk");
      index1.IsUnique = true;
      _ = index1.CreateIndexColumn(t.TableColumns["int16_1"]);
      index1.NonkeyColumns.Add(t.TableColumns["int32_1"]);
      index1.NonkeyColumns.Add(t.TableColumns["int32_2"]);
      _ = ExecuteNonQuery(SqlDdl.Create(index1));

      var schema = ExtractDefaultSchema();
      var extractedTable = schema.Tables[GeneralPurposeTableName];
      var extractedIndex1 = extractedTable.Indexes["ix_gptable_simple_nk"];
      Assert.That(extractedIndex1, Is.Not.Null);
      Assert.That(extractedIndex1.IsUnique, Is.True);
      Assert.That(extractedIndex1.Columns.Count, Is.EqualTo(1));
      Assert.That(extractedIndex1.Columns[0].Name, Is.EqualTo("int16_1"));
      Assert.That(extractedIndex1.NonkeyColumns[0].Name, Is.EqualTo("int32_1"));
      Assert.That(extractedIndex1.NonkeyColumns[1].Name, Is.EqualTo("int32_2"));
    }

    [Test]
    public virtual void CreateExpressionIndexTest()
    {
      CheckExpressionIndexSupported(); // makes non-supporters ignore this test

      // fail test with exception to force create table in provider-specific test which supports expression
      if (ExpressionIndexTable is null)
        throw new Exception($"Required table does not exist. If it supposes to exist, make sure you implemented {nameof(CreateExpressionIndexTable)}()");
    }

    [Test]
    public virtual void CreateFilteredIndexTest()
    {
      CheckFilteredIndexSupported();

      // Creating index
      var t = schema.Tables[GeneralPurposeTableName];
      var i = t.CreateIndex(FilteredIndexName);
      _ = i.CreateIndexColumn(t.TableColumns["int16_2n"]);
      _ = i.CreateIndexColumn(t.TableColumns["int64_2n"]);
      var tr = SqlDml.TableRef(t);
      i.Where = SqlDml.IsNotNull(tr["int16_2n"]) && SqlDml.IsNotNull(tr["int64_2n"]);
      _ = ExecuteNonQuery(SqlDdl.Create(i));

      // Extracting index and checking its properties
      var c2 = ExtractCatalog();
      var s2 = c2.DefaultSchema;
      var t2 = s2.Tables[GeneralPurposeTableName];
      var i2 = t2.Indexes[FilteredIndexName];
      Assert.That(i2, Is.Not.Null);
      Assert.That(i2.Columns.Count, Is.EqualTo(2));
      Assert.That(i2.Where is not null, Is.True);
    }

    [Test]
    public void CreateNullsDistinctIndex()
    {
      CheckUniqueNullsTreatmentSupported();

      var t = GeneralPurposeTable;
      var index1 = t.CreateIndex("ixu_gptable_simple_nd");
      index1.IsUnique = true;
      index1.NullTreatment = Xtensive.Sql.Model.UniqueNullTreatment.Distinct;
      _ = index1.CreateIndexColumn(t.TableColumns["int16_1n"]);
      _ = ExecuteNonQuery(SqlDdl.Create(index1));

      var index2 = t.CreateIndex("ixu_gptable_simple_nnd");
      index2.IsUnique = true;
      index2.NullTreatment = Xtensive.Sql.Model.UniqueNullTreatment.NotDistinct;
      _ = index2.CreateIndexColumn(t.TableColumns["int16_2n"]);
      _ = ExecuteNonQuery(SqlDdl.Create(index2));

      var index3 = t.CreateIndex("ixu_gptable_simple_us");
      index3.IsUnique = true;
      index3.NullTreatment = Xtensive.Sql.Model.UniqueNullTreatment.Unspecified;
      _ = index3.CreateIndexColumn(t.TableColumns["int32_1n"]);
      _ = ExecuteNonQuery(SqlDdl.Create(index3));


      var index4 = t.CreateIndex("ix_gptable_simple_nd");
      index4.IsUnique = false;
      index4.NullTreatment = Xtensive.Sql.Model.UniqueNullTreatment.Distinct;
      _ = index4.CreateIndexColumn(t.TableColumns["int32_2n"]);
      _ = ExecuteNonQuery(SqlDdl.Create(index4));

      var index5 = t.CreateIndex("ix_gptable_simple_nnd");
      index5.IsUnique = false;
      index5.NullTreatment = Xtensive.Sql.Model.UniqueNullTreatment.NotDistinct;
      _ = index5.CreateIndexColumn(t.TableColumns["int64_1n"]);
      _ = ExecuteNonQuery(SqlDdl.Create(index5));

      var index6 = t.CreateIndex("ix_gptable_simple_us");
      index6.IsUnique = false;
      index6.NullTreatment = Xtensive.Sql.Model.UniqueNullTreatment.Unspecified;
      _ = index6.CreateIndexColumn(t.TableColumns["int64_2n"]);
      _ = ExecuteNonQuery(SqlDdl.Create(index6));


      var schema = ExtractDefaultSchema();
      var extractedTable = schema.Tables[GeneralPurposeTableName];

      var extractedIndex1 = extractedTable.Indexes["ixu_gptable_simple_nd"];
      Assert.That(extractedIndex1, Is.Not.Null);
      Assert.That(extractedIndex1.IsUnique, Is.True);
      Assert.That(extractedIndex1.NullTreatment, Is.EqualTo(Xtensive.Sql.Model.UniqueNullTreatment.Distinct));

      var extractedIndex2 = extractedTable.Indexes["ixu_gptable_simple_nnd"];
      Assert.That(extractedIndex2, Is.Not.Null);
      Assert.That(extractedIndex2.IsUnique, Is.True);
      Assert.That(extractedIndex2.NullTreatment, Is.EqualTo(Xtensive.Sql.Model.UniqueNullTreatment.NotDistinct));

      var extractedIndex3 = extractedTable.Indexes["ixu_gptable_simple_us"];
      Assert.That(extractedIndex3, Is.Not.Null);
      Assert.That(extractedIndex3.IsUnique, Is.True);
      Assert.That(extractedIndex3.NullTreatment, Is.EqualTo(DefaultNullsTreatmentWhenSupported()));

      var extractedIndex4 = extractedTable.Indexes["ix_gptable_simple_nd"];
      Assert.That(extractedIndex4, Is.Not.Null);
      Assert.That(extractedIndex4.IsUnique, Is.False);
      Assert.That(extractedIndex4.NullTreatment, Is.EqualTo(Xtensive.Sql.Model.UniqueNullTreatment.Unspecified));

      var extractedIndex5 = extractedTable.Indexes["ix_gptable_simple_nnd"];
      Assert.That(extractedIndex5, Is.Not.Null);
      Assert.That(extractedIndex5.IsUnique, Is.False);
      Assert.That(extractedIndex5.NullTreatment, Is.EqualTo(Xtensive.Sql.Model.UniqueNullTreatment.Unspecified));

      var extractedIndex6 = extractedTable.Indexes["ix_gptable_simple_us"];
      Assert.That(extractedIndex6, Is.Not.Null);
      Assert.That(extractedIndex6.IsUnique, Is.False);
      Assert.That(extractedIndex6.NullTreatment, Is.EqualTo(Xtensive.Sql.Model.UniqueNullTreatment.Unspecified));
    }

    [Test]
    public void CreateNotNullsDistinctIndex()
    {
      CheckUniqueNullsTreatmentNotSupported();

      var t = GeneralPurposeTable;

      var index1 = t.CreateIndex("ixu_gptable_simple_us");
      index1.IsUnique = true;
      index1.NullTreatment = Xtensive.Sql.Model.UniqueNullTreatment.Unspecified;
      _ = index1.CreateIndexColumn(t.TableColumns["int16_1n"]);
      _ = ExecuteNonQuery(SqlDdl.Create(index1));

      var index2 = t.CreateIndex("ix_gptable_simple_us");
      index2.IsUnique = false;
      index2.NullTreatment = Xtensive.Sql.Model.UniqueNullTreatment.Unspecified;
      _ = index2.CreateIndexColumn(t.TableColumns["int16_2n"]);
      _ = ExecuteNonQuery(SqlDdl.Create(index2));

      var index3 = t.CreateIndex("ixu_gptable_simple_nd");
      index3.IsUnique = true;
      index3.NullTreatment = Xtensive.Sql.Model.UniqueNullTreatment.Distinct;
      _ = index3.CreateIndexColumn(t.TableColumns["int32_1n"]);
      _ = Assert.Throws<NotSupportedException>(() => ExecuteNonQuery(SqlDdl.Create(index3)));

      var index4 = t.CreateIndex("ixu_gptable_simple_nnd");
      index4.IsUnique = true;
      index4.NullTreatment = Xtensive.Sql.Model.UniqueNullTreatment.NotDistinct;
      _ = index4.CreateIndexColumn(t.TableColumns["int32_2n"]);
      _ = Assert.Throws<NotSupportedException>(() => ExecuteNonQuery(SqlDdl.Create(index4)));

      var index5 = t.CreateIndex("ix_gptable_simple_nd");
      index5.IsUnique = false;
      index5.NullTreatment = Xtensive.Sql.Model.UniqueNullTreatment.Distinct;
      _ = index5.CreateIndexColumn(t.TableColumns["int32_2n"]);
      _ = Assert.Throws<NotSupportedException>(() => ExecuteNonQuery(SqlDdl.Create(index5)));

      var index6 = t.CreateIndex("ix_gptable_simple_nnd");
      index6.IsUnique = false;
      index6.NullTreatment = Xtensive.Sql.Model.UniqueNullTreatment.NotDistinct;
      _ = index6.CreateIndexColumn(t.TableColumns["int64_1n"]);
      _ = Assert.Throws<NotSupportedException>(() => ExecuteNonQuery(SqlDdl.Create(index6)));


      var schema = ExtractDefaultSchema();
      var extractedTable = schema.Tables[GeneralPurposeTableName];

      var extractedIndex1 = extractedTable.Indexes["ixu_gptable_simple_us"];
      Assert.That(extractedIndex1, Is.Not.Null);
      Assert.That(extractedIndex1.IsUnique, Is.True);
      Assert.That(extractedIndex1.NullTreatment, Is.EqualTo(Xtensive.Sql.Model.UniqueNullTreatment.Unspecified));

      var extractedIndex2 = extractedTable.Indexes["ix_gptable_simple_us"];
      Assert.That(extractedIndex2, Is.Not.Null);
      Assert.That(extractedIndex2.IsUnique, Is.False);
      Assert.That(extractedIndex2.NullTreatment, Is.EqualTo(Xtensive.Sql.Model.UniqueNullTreatment.Unspecified));

      var extractedIndex3 = extractedTable.Indexes["ixu_gptable_simple_nd"];
      Assert.That(extractedIndex3, Is.Null);

      var extractedIndex4 = extractedTable.Indexes["ixu_gptable_simple_nnd"];
      Assert.That(extractedIndex4, Is.Null);

      var extractedIndex5 = extractedTable.Indexes["ix_gptable_simple_nd"];
      Assert.That(extractedIndex5, Is.Null);

      var extractedIndex6 = extractedTable.Indexes["ix_gptable_simple_nnd"];
      Assert.That(extractedIndex6, Is.Null);
    }

    protected virtual Xtensive.Sql.Model.UniqueNullTreatment DefaultNullsTreatmentWhenSupported() => Xtensive.Sql.Model.UniqueNullTreatment.Distinct;

    private void CheckFilteredIndexSupported()
    {
      if (!indexInfo.Features.HasFlag(Xtensive.Sql.Info.IndexFeatures.Filtered))
        throw NotSupported("filtered indexes");
    }

    private void CheckExpressionIndexSupported()
    {
      if (!indexInfo.Features.HasFlag(Xtensive.Sql.Info.IndexFeatures.Expressions))
        throw NotSupported("expressions in indexes");
    }

    private void CheckSortOrderSupported()
    {
      if (!indexInfo.Features.HasFlag(Xtensive.Sql.Info.IndexFeatures.SortOrder))
        throw NotSupported("sort order in index columns");
    }

    private void CheckNonKeyColumnsSupported()
    {
      if (!indexInfo.Features.HasFlag(Xtensive.Sql.Info.IndexFeatures.NonKeyColumns))
        throw NotSupported("non-key columns in indexes");
    }

    private void CheckUniqueIndexesSupported()
    {
      if (!indexInfo.Features.HasFlag(Xtensive.Sql.Info.IndexFeatures.Unique))
        throw NotSupported("filtered indexes");
    }

    private void CheckUniqueNullsTreatmentSupported()
    {
      if (!indexInfo.Features.HasFlag(Xtensive.Sql.Info.IndexFeatures.UniqueNullTreatment))
        throw NotSupported("unique nulls treatment setting");
    }

    private void CheckUniqueNullsTreatmentNotSupported()
    {
      if (indexInfo.Features.HasFlag(Xtensive.Sql.Info.IndexFeatures.UniqueNullTreatment))
        throw NotSupported("unique nulls treatment setting");
    }

    private static IgnoreException NotSupported(string feature)
      => new IgnoreException($"Provider does not support {feature}");
  }
}